namespace OpenTUI.Components.Theme;

/// <summary>
/// Semantic color tokens for theming, matching OpenCode's design system.
/// </summary>
public enum ColorToken
{
    // === Text ===
    /// <summary>Primary/emphasized text</summary>
    TextStrong,
    /// <summary>Default body text</summary>
    TextBase,
    /// <summary>Secondary/muted text</summary>
    TextWeak,
    /// <summary>Disabled text</summary>
    TextDisabled,
    /// <summary>Text on primary color backgrounds</summary>
    TextOnPrimary,
    /// <summary>Text on critical/error backgrounds</summary>
    TextOnCritical,
    /// <summary>Text on success backgrounds</summary>
    TextOnSuccess,

    // === Surfaces ===
    /// <summary>Default background</summary>
    SurfaceBase,
    /// <summary>Elevated surfaces (cards, dialogs)</summary>
    SurfaceElevated,
    /// <summary>Overlay backgrounds (modals)</summary>
    SurfaceOverlay,
    /// <summary>Sunken/inset surfaces</summary>
    SurfaceSunken,
    /// <summary>Input field background</summary>
    InputBase,
    /// <summary>Input field background on hover</summary>
    InputHover,
    /// <summary>Input field background when focused</summary>
    InputFocus,

    // === Borders ===
    /// <summary>Emphasized borders</summary>
    BorderStrong,
    /// <summary>Default borders</summary>
    BorderBase,
    /// <summary>Subtle borders</summary>
    BorderWeak,
    /// <summary>Focus/selected state borders</summary>
    BorderSelected,
    /// <summary>Error state borders</summary>
    BorderCritical,
    /// <summary>Success state borders</summary>
    BorderSuccess,

    // === Primary (main action color) ===
    /// <summary>Primary action color</summary>
    PrimaryBase,
    /// <summary>Primary hover state</summary>
    PrimaryHover,
    /// <summary>Primary active/pressed state</summary>
    PrimaryActive,
    /// <summary>Primary weak/subtle variant</summary>
    PrimaryWeak,

    // === Secondary ===
    /// <summary>Secondary action color</summary>
    SecondaryBase,
    /// <summary>Secondary hover state</summary>
    SecondaryHover,
    /// <summary>Secondary active state</summary>
    SecondaryActive,

    // === Ghost (transparent) ===
    /// <summary>Ghost button hover background</summary>
    GhostHover,
    /// <summary>Ghost button active background</summary>
    GhostActive,

    // === Status Colors ===
    /// <summary>Success/positive state</summary>
    SuccessBase,
    /// <summary>Success weak/subtle</summary>
    SuccessWeak,
    /// <summary>Warning state</summary>
    WarningBase,
    /// <summary>Warning weak/subtle</summary>
    WarningWeak,
    /// <summary>Critical/error/danger state</summary>
    CriticalBase,
    /// <summary>Critical weak/subtle</summary>
    CriticalWeak,
    /// <summary>Info/neutral state</summary>
    InfoBase,
    /// <summary>Info weak/subtle</summary>
    InfoWeak,

    // === Icons ===
    /// <summary>Emphasized icons</summary>
    IconStrong,
    /// <summary>Default icons</summary>
    IconBase,
    /// <summary>Muted icons</summary>
    IconWeak,
    /// <summary>Disabled icons</summary>
    IconDisabled,

    // === Accent (for highlights, selections) ===
    /// <summary>Accent color for selections</summary>
    AccentBase,
    /// <summary>Accent color weak variant</summary>
    AccentWeak
}
