namespace TetrisWinForms.Managers;

public static class ImageManager
{
    private static readonly Dictionary<string, Image> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object Sync = new();

    public static Image? Get(string fileName)
    {
        string path = AppPaths.Image(fileName);
        if (!File.Exists(path)) return null;

        lock (Sync)
        {
            if (Cache.TryGetValue(path, out Image? cached))
                return cached;

            // Clone the bitmap so the source PNG is never kept locked.
            using var source = Image.FromFile(path);
            var copy = new Bitmap(source);
            Cache[path] = copy;
            return copy;
        }
    }
}
