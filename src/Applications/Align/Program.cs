using CsvHelper.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Data.Common;
using System.Diagnostics;
using Urdep.Extensions.Text;

namespace Align;
internal class Program
{
    internal static readonly Arguments arguments = new();

    private static int Main(string[] args)
    {
        try
        {
            var status = ParseArgs(args);
            if (status != 0)
            {
                return status;
            }

            var sw = Stopwatch.StartNew();

            Matcher matcher = new();
            matcher.AddIncludePatterns(arguments.Includes);
            matcher.AddExcludePatterns(arguments.Excludes);

            DoAlignment(matcher);

            Console.WriteLine("Finished in {0}", sw.Elapsed);
            return status;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static void DoAlignment(Matcher matcher)
    {
        foreach (var dir in arguments.Directories)
        {
            var matchingFiles = matcher.GetResultsInFullPath(dir);
            foreach (var file in matchingFiles)
            {
                string[] columns;
                var records = new List<Dictionary<string, string>>();
                string delimiter;

                {
                    using var streamReader = new StreamReader(file, arguments.FileEncoding);
                    var cfg = new CsvConfiguration(arguments.FileCulture) { TrimOptions = TrimOptions.Trim };
                    using var csv = new CsvHelper.CsvReader(streamReader, cfg);
                    delimiter = csv.Context.Parser.Delimiter;

                    csv.Read();
                    csv.ReadHeader();

                    columns = csv.HeaderRecord
                        ?? Enumerable.Range(1, csv.ColumnCount).Select(x => x.ToString(arguments.FileCulture)).ToArray();
                    while (csv.Read())
                    {
                        var record = new Dictionary<string, string>();
                        records.Add(record);
                        for (int i = 0; i < columns.Length; ++i)
                        {
                            var value = csv.GetField(i);
                            record[columns[i]] = value ?? "";
                        }
                    }
                }

                var aligner = new CsvColumnAligner(columns.ToDictionary(x => x, x => x), records, PadSide.Right);
                aligner.AlignColumns();

                {
                    using var streamWriter = new StreamWriter(file, false, arguments.FileEncoding);
                    var cfg = new CsvConfiguration(arguments.FileCulture) { Delimiter = delimiter, ShouldQuote = c => c.Field.Contains(delimiter), };
                    using var csv = new CsvHelper.CsvWriter(streamWriter, cfg);
                    foreach (var column in aligner.Columns.Values)
                    {
                        csv.WriteField(column);
                    }
                    csv.NextRecord();
                    foreach (var record in aligner.Records)
                    {
                        foreach (var column in aligner.Columns.Keys)
                        {
                            csv.WriteField(record[column]);
                        }
                        csv.NextRecord();
                    }
                    csv.Flush();
                }
            }
        }
    }

    private static int ParseArgs(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--directory":
                case "-d":
                    arguments.Directories.Add(Path.GetFullPath(args[++i]));
                    continue;

                case "--include":
                case "-i":
                    arguments.Includes.Add(args[++i]);
                    continue;

                case "--exclude":
                case "-x":
                    arguments.Excludes.Add(args[++i]);
                    continue;

                default:
                    Console.WriteLine("Unknown argument: {0}", args[i]);
                    return 2;
            }
        }

        return 0;
    }
}
