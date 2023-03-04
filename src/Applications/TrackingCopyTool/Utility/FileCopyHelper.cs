using System.Runtime.InteropServices;

namespace TrackingCopyTool.Utility
{
    internal class FileCopyHelper
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CopyFileEx(
            string lpExistingFileName,
            string lpNewFileName,
            CopyProgressRoutine lpProgressRoutine,
            IntPtr lpData,
            ref Int32 pbCancel,
            CopyFileFlags dwCopyFlags
        );

        delegate CopyProgressResult CopyProgressRoutine(
            long TotalFileSize,
            long TotalBytesTransferred,
            long StreamSize,
            long StreamBytesTransferred,
            uint dwStreamNumber,
            CopyProgressCallbackReason dwCallbackReason,
            IntPtr hSourceFile,
            IntPtr hDestinationFile,
            IntPtr lpData
        );

        private int _pbCancel;

        public enum CopyProgressResult : uint
        {
            PROGRESS_CONTINUE = 0,
            PROGRESS_CANCEL = 1,
            PROGRESS_STOP = 2,
            PROGRESS_QUIET = 3
        }

        public enum CopyProgressCallbackReason : uint
        {
            CALLBACK_CHUNK_FINISHED = 0x00000000,
            CALLBACK_STREAM_SWITCH = 0x00000001
        }

        [Flags]
        enum CopyFileFlags : uint
        {
            COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
            COPY_FILE_RESTARTABLE = 0x00000002,
            COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
            COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008
        }

        public void XCopyEz(
            string sourceFile,
            string targetFile,
            Func<long, long, long, CopyProgressCallbackReason, CopyProgressResult> cb)
        {

            Func<long, long, long, long, uint, CopyProgressCallbackReason, IntPtr, IntPtr, IntPtr, CopyProgressResult> wrapper =
                (total, transferred, streamSize, _, _, reason, _, _, _) => cb(total, transferred, streamSize,reason);
            CopyFileEx(
                sourceFile,
                targetFile,
                new CopyProgressRoutine(wrapper),
                IntPtr.Zero,
                ref _pbCancel,
                CopyFileFlags.COPY_FILE_RESTARTABLE
            );
        }

        public void XCopy(
            string sourceFile,
            string targetFile,
            Func<long, long, long, long, uint, CopyProgressCallbackReason, IntPtr, IntPtr, IntPtr, CopyProgressResult> cb)
        {
            CopyFileEx(
                sourceFile,
                targetFile,
                new CopyProgressRoutine(cb),
                IntPtr.Zero,
                ref _pbCancel,
                CopyFileFlags.COPY_FILE_RESTARTABLE
            );
        }

        private CopyProgressResult CopyProgressHandler(
            long total,
            long transferred,
            long streamSize,
            long StreamByteTrans,
            uint dwStreamNumber,
            CopyProgressCallbackReason reason,
            IntPtr hSourceFile,
            IntPtr hDestinationFile,
            IntPtr lpData
        )
        {
            return CopyProgressResult.PROGRESS_CONTINUE;
        }
    }
}
