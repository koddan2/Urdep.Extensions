using Microsoft.Extensions.FileSystemGlobbing;
using System.Collections.Generic;
using TrackingCopyTool.Utility;

namespace CleverCopy;

internal class Processor
{
    private readonly Stopwatch _stopwatch;
    private readonly IEnumerable<string> _sourceFilesAbsolutePaths;
    private readonly IEnumerable<string> _targetFilesAbsolutePaths;
    private readonly Manifest _manifestSource;
    private readonly Manifest _manifestTarget;
    private readonly DateTimeOffset _startTime;

    private long _totalFilesCopied = 0;
    private long _totalFilesNotCopied = 0;
    private long _totalBytesCopied = 0;
    private long _totalBytesNotCopied = 0;

    public Processor()
    {
        _stopwatch = Stopwatch.StartNew();
        _stopwatch.Start();

        _startTime = DateTimeOffset.Now;

        Program.Io.Verbose("Checking whether directory {0} exists", Program.Cfg.SourceDirectory);
        if (!Directory.Exists(Program.Cfg.SourceDirectory))
        {
            throw Exns.GeneralError(3, $"The directory {Program.Cfg.SourceDirectory} ({nameof(Program.Cfg.SourceDirectory)}) does not exist.");
        }

        Matcher matcher = new(StringComparison.OrdinalIgnoreCase);
        matcher.AddIncludePatterns(Program.Cfg.GetIncludeGlobs());
        matcher.AddExcludePatterns(Program.Cfg.GetExcludeGlobs());

        _sourceFilesAbsolutePaths = matcher.GetResultsInFullPath(Program.Cfg.SourceDirectory);
        _targetFilesAbsolutePaths = matcher.GetResultsInFullPath(Program.Cfg.TargetDirectory);

        _manifestSource = new(Program.Cfg.SourceDirectory, _sourceFilesAbsolutePaths);
        _manifestTarget = new(Program.Cfg.TargetDirectory, _targetFilesAbsolutePaths, loadExistingManifestOnly: true);
    }

    public void Execute()
    {
        Step0Prepare();
        Step1PerformDiffCheckAndCopy();
        Step2Report();
    }

    private void Step2Report()
    {
        Program.Io.ErrLine("Copied a total of:        {0:f2} kB ({1} files)", _totalBytesCopied / 1000d, _totalFilesCopied);
        Program.Io.ErrLine("Did not copy a total of:  {0:f2} kB ({1} files)", _totalBytesNotCopied / 1000d, _totalFilesNotCopied);
        Program.Io.ErrLine("Duration:                 {0}", _stopwatch.Elapsed);
        Program.Io.ErrLine("Started at:               {0}", _startTime.ToString());
        Program.Io.ErrLine("Ended at:                 {0}", DateTimeOffset.Now.ToString());
    }

    private void Step1PerformDiffCheckAndCopy()
    {
        foreach (var fileHashPair in _manifestSource.Hashes)
        {
            var fileRelPath = fileHashPair.Key;
            var goodHash = fileHashPair.Value;
            if (_manifestTarget.Hashes.TryGetValue(fileRelPath, out var hash))
            {
                if (hash == goodHash)
                {
                    Program.Io.Debug("SAME {0}", fileRelPath);
                    var absPathLocal = Path.Combine(Program.Cfg.SourceDirectory, fileRelPath);
                    _totalBytesNotCopied += new FileInfo(absPathLocal).Length;
                    _totalFilesNotCopied += 1;
                }
                else
                {
                    // files have different hashes, so we do copy
                    Program.Io.Verbose("DIFF {0}", fileRelPath);
                    CopyFileWithReporting(fileRelPath, goodHash);
                }
            }
            else
            {
                // file in target does not exist, so we do copy
                Program.Io.Verbose("MISS {0}", fileRelPath);
                CopyFileWithReporting(fileRelPath, goodHash);
            }
        }
    }

    private void CopyFileWithReporting(string fileRelPath, ulong goodHash)
    {
        var absPathLocal = Path.Combine(Program.Cfg.SourceDirectory, fileRelPath);
        DoCopy(fileRelPath);
        Program.Io.ErrLine("");
        _manifestTarget.Hashes.Add(fileRelPath, goodHash);
        _manifestTarget.WriteToFileSystem(Program.Cfg.GetTargetDirectoryCleverCopyDirectory());
        _totalBytesCopied += new FileInfo(absPathLocal).Length;
        _totalFilesCopied += 1;
    }

    private static void DoCopy(string fileRelPath)
    {
        var srcFullPath = Path.Combine(Program.Cfg.SourceDirectory, fileRelPath);
        var tgtFullPath = Path.Combine(Program.Cfg.TargetDirectory, fileRelPath);
        var tgtContainingDir = Path.GetDirectoryName(tgtFullPath)
            ?? throw Exns.GeneralError(6, "Could not determine directory at target ({0})", tgtFullPath);
        Directory.CreateDirectory(tgtContainingDir);

#pragma warning disable RCS1163 // Unused parameter.
        new FileCopyHelper().XCopyEz(
            srcFullPath,
            tgtFullPath,
            (total, transferred, streamSize, reason) =>
            {
                PrintProgress(total, transferred, fileRelPath);
                return FileCopyHelper.CopyProgressResult.PROGRESS_CONTINUE;
            }
        );
#pragma warning restore RCS1163 // Unused parameter.
    }

    private void Step0Prepare()
    {
        Directory.CreateDirectory(Program.Cfg.GetSourceDirectoryCleverCopyDirectory());
        _manifestSource.WriteToFileSystem(Program.Cfg.GetSourceDirectoryCleverCopyDirectory());

        Directory.CreateDirectory(Program.Cfg.GetTargetDirectoryCleverCopyDirectory());
    }

    private static void PrintProgress(long total, long transferred, string path)
    {
        Program.Io.ErrWriter.Write(
            "\r{0} {1} {2}",
            $"{100f * Math.Max(transferred, 1) / (float)Math.Max(total, 1):f2}%".PadRight(10, ' '),
            $"{total / 1000f:f2} kB".PadRight(20, ' '),
            path
        );
    }
}