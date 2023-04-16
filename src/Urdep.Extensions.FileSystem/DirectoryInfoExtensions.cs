namespace Urdep.Extensions.FileSystem
{
    public static class DirectoryInfoExtensions
    {
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
