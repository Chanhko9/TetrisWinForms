using TetrisWinForms.Core;

namespace TetrisWinForms.Models;

public sealed record ScoreRecord(
    string PlayerName,
    int Score,
    GameMode Mode,
    DateTime PlayedAt);
