using System.Diagnostics;
using System.Linq;

namespace Urdep.Extensions.Text;

/// <summary>
/// Enumeration of which side to pad.
/// </summary>
public enum PadSide
{
    /// <summary>
    /// Pad the left side.
    /// </summary>
    Left,
    /// <summary>
    /// Pad the right side.
    /// </summary>
    Right,
}

/// <summary>
/// A CSV column aligner.
/// </summary>
/// <param name="Columns">The dict of columns</param>
/// <param name="Records">The records</param>
/// <param name="PadSide">Which side to pad.</param>
public record CsvColumnAligner(
    // I.e. the Header record, where keys are unchanged and values are padded.
    IDictionary<string, string> Columns,
    // All records
    IEnumerable<IDictionary<string, string>> Records,
    // Which side to pad
    PadSide PadSide
)
{
    /// <summary>
    /// Whether or not this is computed.
    /// </summary>
    public bool Computed { get; init; }
    /// <summary>
    /// The widths of the columns.
    /// </summary>
    public IDictionary<string, int>? ColumnWidths { get; init; }
};

/// <summary>
/// CSV extensions.
/// </summary>
public static class CsvExtensions
{
    /// <summary>
    /// Align the columns.
    /// </summary>
    /// <param name="csv">The CSV</param>
    /// <returns>The aligner instance.</returns>
    public static CsvColumnAligner AlignColumns(this CsvColumnAligner csv)
    {
        if (csv.Computed)
        {
            return csv;
        }

        var columns = csv.Columns.Keys;
        var records = csv.Records.ToList();
        IDictionary<string, int> columnWidths;
        if (csv.ColumnWidths is null)
        {
            columnWidths = columns.ToDictionary(x => x.Trim(), x => x.Trim().Length + 1);
            foreach (var dict in records)
            {
                foreach (var col in columns)
                {
                    var width = (dict[col]?.Trim()?.Length ?? 0) + 1;
                    if (width > columnWidths[col])
                    {
                        columnWidths[col] = width;
                    }
                }
            }
        }
        else
        {
            columnWidths = csv.ColumnWidths;
        }

        DoAlignment(csv, columns, new[] { csv.Columns }, columnWidths);
        DoAlignment(csv, columns, records, columnWidths);

        return csv with
        {
            Records = records,
            Computed = true,
            ColumnWidths = columnWidths,
        };
    }

    private static void DoAlignment(
        CsvColumnAligner csv,
        ICollection<string> columns,
        IList<IDictionary<string, string>> records,
        IDictionary<string, int> columnWidths
    )
    {
        foreach (var dict in records)
        {
            foreach (var col in columns)
            {
                var width = columnWidths[col];
                var val = dict[col]?.Trim() ?? "";
                if (val.Length < width)
                {
                    string newVal;
                    if (csv.PadSide == PadSide.Left)
                    {
                        newVal = val.PadLeft(width, ' ');
                    }
                    else if (csv.PadSide == PadSide.Right)
                    {
                        newVal = val.PadRight(width, ' ');
                    }
                    else
                    {
                        newVal = val;
                    }

                    dict[col] = newVal;
                }
            }
        }
    }
}
