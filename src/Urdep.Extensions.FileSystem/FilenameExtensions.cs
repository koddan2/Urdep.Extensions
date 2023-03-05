namespace Urdep.Extensions.FileSystem
{
    public static class FilenameExtensions
    {
        public static string GetTransformedFileNameKeepParentPath(string filename, Func<string,string> renamer)
        {
            var dir = Path.GetDirectoryName(filename);
            var name = Path.GetFileNameWithoutExtension(filename);
            var ext = Path.GetExtension(filename);
            var newName = renamer(name);

            if (dir is not null)
            {
            return Path.Combine(dir, $"{newName}{ext}");
            }
            else
            {
                return $"{newName}{ext}";
            }
        }
    }
}