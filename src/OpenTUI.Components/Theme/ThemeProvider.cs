namespace OpenTUI.Components.Theme;

/// <summary>
/// Provides the current theme to the component tree.
/// Uses ambient context pattern for easy access.
/// </summary>
public static class ThemeProvider
{
    private static readonly AsyncLocal<Theme> _currentTheme = new();
    
    /// <summary>
    /// The current theme. Defaults to Dark if not set.
    /// </summary>
    public static Theme Current
    {
        get => _currentTheme.Value ?? Theme.Dark;
        set => _currentTheme.Value = value;
    }
    
    /// <summary>
    /// Sets the theme for the duration of the action.
    /// </summary>
    public static void Use(Theme theme, Action action)
    {
        var previous = _currentTheme.Value;
        try
        {
            _currentTheme.Value = theme;
            action();
        }
        finally
        {
            _currentTheme.Value = previous;
        }
    }
    
    /// <summary>
    /// Sets the theme for the duration of the async action.
    /// </summary>
    public static async Task UseAsync(Theme theme, Func<Task> action)
    {
        var previous = _currentTheme.Value;
        try
        {
            _currentTheme.Value = theme;
            await action();
        }
        finally
        {
            _currentTheme.Value = previous;
        }
    }
    
    /// <summary>
    /// Resets to the default dark theme.
    /// </summary>
    public static void Reset() => _currentTheme.Value = Theme.Dark;
}
