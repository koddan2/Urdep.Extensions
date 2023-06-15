namespace Urdep.Extensions.FileSystem
{
    /// <summary>
    /// Extensions for DirectoryInfo.
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Walks up the directory hierarchy until the test is satisfied.
        /// </summary>
        /// <param name="directoryInfo">The current directory.</param>
        /// <param name="test">The test.</param>
        /// <param name="maxLevels">The max levels to walk.</param>
        /// <returns>The directory, if found.</returns>
        public static DirectoryInfo? WalkUpUntil(
            this DirectoryInfo directoryInfo,
            Func<DirectoryInfo, bool> test,
            int maxLevels = 10
        )
        {
            var counter = 0;
            var di = directoryInfo;
            while (counter++ < maxLevels && di is not null && !test(di))
            {
                di = di.Parent;
            }

            return di;
        }
    }
}
