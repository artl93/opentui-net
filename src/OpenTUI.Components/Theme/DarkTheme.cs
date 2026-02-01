using OpenTUI.Core.Colors;

namespace OpenTUI.Components.Theme;

/// <summary>
/// Dark theme inspired by OpenCode's dark mode.
/// </summary>
public static class DarkTheme
{
    public static Theme Instance { get; } = new Theme("Dark", isDark: true, new Dictionary<ColorToken, RGBA>
    {
        // Text
        [ColorToken.TextStrong] = RGBA.FromHex("#ffffff"),
        [ColorToken.TextBase] = RGBA.FromHex("#e4e4e7"),
        [ColorToken.TextWeak] = RGBA.FromHex("#a1a1aa"),
        [ColorToken.TextDisabled] = RGBA.FromHex("#52525b"),
        [ColorToken.TextOnPrimary] = RGBA.FromHex("#ffffff"),
        [ColorToken.TextOnCritical] = RGBA.FromHex("#ffffff"),
        [ColorToken.TextOnSuccess] = RGBA.FromHex("#ffffff"),
        
        // Surfaces
        [ColorToken.SurfaceBase] = RGBA.FromHex("#09090b"),
        [ColorToken.SurfaceElevated] = RGBA.FromHex("#18181b"),
        [ColorToken.SurfaceOverlay] = RGBA.FromHex("#27272a"),
        [ColorToken.SurfaceSunken] = RGBA.FromHex("#000000"),
        [ColorToken.InputBase] = RGBA.FromHex("#18181b"),
        [ColorToken.InputHover] = RGBA.FromHex("#27272a"),
        [ColorToken.InputFocus] = RGBA.FromHex("#27272a"),
        
        // Borders
        [ColorToken.BorderStrong] = RGBA.FromHex("#52525b"),
        [ColorToken.BorderBase] = RGBA.FromHex("#3f3f46"),
        [ColorToken.BorderWeak] = RGBA.FromHex("#27272a"),
        [ColorToken.BorderSelected] = RGBA.FromHex("#3b82f6"),
        [ColorToken.BorderCritical] = RGBA.FromHex("#ef4444"),
        [ColorToken.BorderSuccess] = RGBA.FromHex("#22c55e"),
        
        // Primary (blue)
        [ColorToken.PrimaryBase] = RGBA.FromHex("#3b82f6"),
        [ColorToken.PrimaryHover] = RGBA.FromHex("#2563eb"),
        [ColorToken.PrimaryActive] = RGBA.FromHex("#1d4ed8"),
        [ColorToken.PrimaryWeak] = RGBA.FromHex("#1e3a5f"),
        
        // Secondary (gray)
        [ColorToken.SecondaryBase] = RGBA.FromHex("#3f3f46"),
        [ColorToken.SecondaryHover] = RGBA.FromHex("#52525b"),
        [ColorToken.SecondaryActive] = RGBA.FromHex("#71717a"),
        
        // Ghost
        [ColorToken.GhostHover] = RGBA.FromHex("#27272a"),
        [ColorToken.GhostActive] = RGBA.FromHex("#3f3f46"),
        
        // Status
        [ColorToken.SuccessBase] = RGBA.FromHex("#22c55e"),
        [ColorToken.SuccessWeak] = RGBA.FromHex("#14532d"),
        [ColorToken.WarningBase] = RGBA.FromHex("#f59e0b"),
        [ColorToken.WarningWeak] = RGBA.FromHex("#78350f"),
        [ColorToken.CriticalBase] = RGBA.FromHex("#ef4444"),
        [ColorToken.CriticalWeak] = RGBA.FromHex("#7f1d1d"),
        [ColorToken.InfoBase] = RGBA.FromHex("#3b82f6"),
        [ColorToken.InfoWeak] = RGBA.FromHex("#1e3a5f"),
        
        // Icons
        [ColorToken.IconStrong] = RGBA.FromHex("#ffffff"),
        [ColorToken.IconBase] = RGBA.FromHex("#a1a1aa"),
        [ColorToken.IconWeak] = RGBA.FromHex("#71717a"),
        [ColorToken.IconDisabled] = RGBA.FromHex("#52525b"),
        
        // Accent
        [ColorToken.AccentBase] = RGBA.FromHex("#8b5cf6"),
        [ColorToken.AccentWeak] = RGBA.FromHex("#4c1d95"),
    });
}
