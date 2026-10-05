namespace TetrisWinForms;

public static class AppPaths
{
    public static string BaseDirectory => AppContext.BaseDirectory;
    public static string AssetsDirectory => Path.Combine(BaseDirectory, "Assets");
    public static string AudioDirectory => Path.Combine(AssetsDirectory, "Audio");
    public static string ImagesDirectory => Path.Combine(AssetsDirectory, "Images");
    public static string DataDirectory => Path.Combine(BaseDirectory, "Data");
    public static string ScoreFile => Path.Combine(DataDirectory, "score_history.txt");

    public static string Audio(params string[] parts)
        => Path.Combine(new[] { AudioDirectory }.Concat(parts).ToArray());

    public static string Image(params string[] parts)
        => Path.Combine(new[] { ImagesDirectory }.Concat(parts).ToArray());

    public static void EnsureFolders()
    {
        Directory.CreateDirectory(AudioDirectory);
        Directory.CreateDirectory(ImagesDirectory);
        Directory.CreateDirectory(DataDirectory);
    }
}
