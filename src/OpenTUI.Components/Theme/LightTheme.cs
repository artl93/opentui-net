using OpenTUI.Core.Colors;

namespace OpenTUI.Components.Theme;

/// <summary>
/// Light theme inspired by OpenCode's light mode.
/// </summary>
public static class LightTheme
{
    public static Theme Instance { get; } = new Theme("Light", isDark: false, new Dictionary<ColorToken, RGBA>
    {
        // Text
        [ColorToken.TextStrong] = RGBA.FromHex("#09090b"),
        [ColorToken.TextBase] = RGBA.FromHex("#18181b"),
        [ColorToken.TextWeak] = RGBA.FromHex("#71717a"),
        [ColorToken.TextDisabled] = RGBA.FromHex("#a1a1aa"),
        [ColorToken.TextOnPrimary] = RGBA.FromHex("#ffffff"),
        [ColorToken.TextOnCritical] = RGBA.FromHex("#ffffff"),
        [ColorToken.TextOnSuccess] = RGBA.FromHex("#ffffff"),

        // Surfaces
        [ColorToken.SurfaceBase] = RGBA.FromHex("#ffffff"),
        [ColorToken.SurfaceElevated] = RGBA.FromHex("#fafafa"),
        [ColorToken.SurfaceOverlay] = RGBA.FromHex("#f4f4f5"),
        [ColorToken.SurfaceSunken] = RGBA.FromHex("#e4e4e7"),
        [ColorToken.InputBase] = RGBA.FromHex("#ffffff"),
        [ColorToken.InputHover] = RGBA.FromHex("#f4f4f5"),
        [ColorToken.InputFocus] = RGBA.FromHex("#ffffff"),

        // Borders
        [ColorToken.BorderStrong] = RGBA.FromHex("#71717a"),
        [ColorToken.BorderBase] = RGBA.FromHex("#d4d4d8"),
        [ColorToken.BorderWeak] = RGBA.FromHex("#e4e4e7"),
        [ColorToken.BorderSelected] = RGBA.FromHex("#3b82f6"),
        [ColorToken.BorderCritical] = RGBA.FromHex("#ef4444"),
        [ColorToken.BorderSuccess] = RGBA.FromHex("#22c55e"),

        // Primary (blue)
        [ColorToken.PrimaryBase] = RGBA.FromHex("#3b82f6"),
        [ColorToken.PrimaryHover] = RGBA.FromHex("#2563eb"),
        [ColorToken.PrimaryActive] = RGBA.FromHex("#1d4ed8"),
        [ColorToken.PrimaryWeak] = RGBA.FromHex("#dbeafe"),

        // Secondary (gray)
        [ColorToken.SecondaryBase] = RGBA.FromHex("#e4e4e7"),
        [ColorToken.SecondaryHover] = RGBA.FromHex("#d4d4d8"),
        [ColorToken.SecondaryActive] = RGBA.FromHex("#a1a1aa"),

        // Ghost
        [ColorToken.GhostHover] = RGBA.FromHex("#f4f4f5"),
        [ColorToken.GhostActive] = RGBA.FromHex("#e4e4e7"),

        // Status
        [ColorToken.SuccessBase] = RGBA.FromHex("#22c55e"),
        [ColorToken.SuccessWeak] = RGBA.FromHex("#dcfce7"),
        [ColorToken.WarningBase] = RGBA.FromHex("#f59e0b"),
        [ColorToken.WarningWeak] = RGBA.FromHex("#fef3c7"),
        [ColorToken.CriticalBase] = RGBA.FromHex("#ef4444"),
        [ColorToken.CriticalWeak] = RGBA.FromHex("#fee2e2"),
        [ColorToken.InfoBase] = RGBA.FromHex("#3b82f6"),
        [ColorToken.InfoWeak] = RGBA.FromHex("#dbeafe"),

        // Icons
        [ColorToken.IconStrong] = RGBA.FromHex("#09090b"),
        [ColorToken.IconBase] = RGBA.FromHex("#52525b"),
        [ColorToken.IconWeak] = RGBA.FromHex("#a1a1aa"),
        [ColorToken.IconDisabled] = RGBA.FromHex("#d4d4d8"),

        // Accent
        [ColorToken.AccentBase] = RGBA.FromHex("#8b5cf6"),
        [ColorToken.AccentWeak] = RGBA.FromHex("#ede9fe"),
    });
}
