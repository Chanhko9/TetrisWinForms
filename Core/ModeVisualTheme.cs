namespace TetrisWinForms.Core;

public sealed record ModeVisualTheme(
    GameMode Mode,
    Color Accent,
    Color Secondary,
    Color Danger,
    string BackgroundImage,
    string ThumbnailImage)
{
    public static readonly IReadOnlyDictionary<GameMode, ModeVisualTheme> All =
        new Dictionary<GameMode, ModeVisualTheme>
        {
            [GameMode.Basic] = new(
                GameMode.Basic,
                Color.DeepSkyBlue,
                Color.MediumPurple,
                Color.Gold,
                "bg_basic.png",
                "mode_basic.png"),

            [GameMode.ArcaneChaos] = new(
                GameMode.ArcaneChaos,
                Color.MediumPurple,
                Color.DeepSkyBlue,
                Color.HotPink,
                "bg_arcane_chaos.png",
                "mode_arcane_chaos.png"),

            [GameMode.PhantomFog] = new(
                GameMode.PhantomFog,
                Color.CadetBlue,
                Color.MediumPurple,
                Color.PaleTurquoise,
                "bg_phantom_fog.png",
                "mode_phantom_fog.png"),

            [GameMode.ZeroGRift] = new(
                GameMode.ZeroGRift,
                Color.DodgerBlue,
                Color.Orchid,
                Color.Cyan,
                "bg_zero_g_rift.png",
                "mode_zero_g_rift.png"),

            [GameMode.CursedTide] = new(
                GameMode.CursedTide,
                Color.Goldenrod,
                Color.Teal,
                Color.OrangeRed,
                "bg_cursed_tide.png",
                "mode_cursed_tide.png"),

            [GameMode.DuelArena] = new(
                GameMode.DuelArena,
                Color.DeepSkyBlue,
                Color.OrangeRed,
                Color.Gold,
                "bg_basic.png",
                "mode_basic.png")
        };

    public static ModeVisualTheme Get(GameMode mode) => All[mode];
}
