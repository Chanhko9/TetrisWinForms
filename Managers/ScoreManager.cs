using System.Globalization;
using TetrisWinForms.Core;
using TetrisWinForms.Models;

namespace TetrisWinForms.Managers;

public static class ScoreManager
{
    public static void Save(ScoreRecord record)
    {
        try
        {
            AppPaths.EnsureFolders();

            string safeName = (record.PlayerName ?? "Player")
                .Replace("|", "-")
                .Replace(Environment.NewLine, " ")
                .Trim();

            if (string.IsNullOrWhiteSpace(safeName)) safeName = "Player";

            string line = string.Join('|',
                safeName,
                record.Score,
                record.Mode,
                record.PlayedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));

            File.AppendAllText(AppPaths.ScoreFile, line + Environment.NewLine);
        }
        catch (IOException)
        {
            // Keep the game running if the history file is temporarily unavailable.
        }
        catch (UnauthorizedAccessException)
        {
            // Keep the game running if Windows denies write access.
        }
    }

    public static List<ScoreRecord> LoadAll()
    {
        var result = new List<ScoreRecord>();

        try
        {
            if (!File.Exists(AppPaths.ScoreFile)) return result;

            foreach (string line in File.ReadLines(AppPaths.ScoreFile))
            {
                string[] p = line.Split('|');
                if (p.Length != 4) continue;
                if (!int.TryParse(p[1], out int score)) continue;
                if (!Enum.TryParse(p[2], out GameMode mode)) continue;
                if (!DateTime.TryParseExact(
                        p[3], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime playedAt)) continue;

                result.Add(new ScoreRecord(p[0], score, mode, playedAt));
            }
        }
        catch (IOException)
        {
            return new List<ScoreRecord>();
        }
        catch (UnauthorizedAccessException)
        {
            return new List<ScoreRecord>();
        }

        return result.OrderByDescending(x => x.Score).ToList();
    }
}
