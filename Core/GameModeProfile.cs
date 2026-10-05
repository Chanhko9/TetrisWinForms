namespace TetrisWinForms.Core;

public sealed record GameModeProfile(
    GameMode Mode,
    string DisplayName,
    string Subtitle,
    string MusicFile,
    string AccentText,
    string Difficulty)
{
    public static readonly IReadOnlyDictionary<GameMode, GameModeProfile> All =
        new Dictionary<GameMode, GameModeProfile>
        {
            [GameMode.Basic] = new(
                GameMode.Basic,
                "BASIC",
                "Classic Tetris",
                "basic_theme.mp3",
                "Classic rules",
                "EASY"),

            [GameMode.ArcaneChaos] = new(
                GameMode.ArcaneChaos,
                "ARCANE CHAOS",
                "Relics and magical destruction",
                "arcane_chaos.mp3",
                "Fantasy / Magic",
                "EASY+"),

            [GameMode.PhantomFog] = new(
                GameMode.PhantomFog,
                "PHANTOM FOG",
                "Danger hidden in the mist",
                "phantom_fog.mp3",
                "Dark / Haunted",
                "HARD+"),

            [GameMode.ZeroGRift] = new(
                GameMode.ZeroGRift,
                "ZERO-G RIFT",
                "Rotating gravity field",
                "zero_g_rift.mp3",
                "Space / Cosmic",
                "EXTREME"),

            [GameMode.CursedTide] = new(
                GameMode.CursedTide,
                "CURSED TIDE",
                "Rising cursed floor",
                "cursed_tide.mp3",
                "Pirate / Adventure",
                "HARD"),

            [GameMode.DuelArena] = new(
                GameMode.DuelArena,
                "DUEL ARENA",
                "Two players - five minute score battle",
                "basic_theme.mp3",
                "Versus / Local Battle",
                "MEDIUM+")
        };

    public static GameModeProfile Get(GameMode mode) => All[mode];
}
