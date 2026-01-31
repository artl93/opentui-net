using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;
using OpenTUI.Core.Terminal;

namespace OpenTUI.Samples;

/// <summary>
/// Interactive form demo with multiple input types.
/// </summary>
public static class FormDemo
{
    public static void Run()
    {
        var state = new TerminalState();
        
        try
        {
            state.EnterAlternateScreen();
            state.HideCursor();
            
            var size = TerminalSize.GetCurrent();
            var width = Math.Min(size.Width, 60);
            var height = Math.Min(size.Height, 22);
            
            var buffer = new FrameBuffer(width, height);
            
            // Fill background
            buffer.FillRect(0, 0, width, height, RGBA.FromValues(0.05f, 0.05f, 0.1f));
            
            // Draw main form border
            DrawBox(buffer, 0, 0, width, height, "User Registration Form", RGBA.FromHex("#5588ff"));
            
            // Form fields
            int y = 3;
            
            // Name field
            DrawLabel(buffer, 2, y, "Name:");
            DrawInputBox(buffer, 14, y, width - 16, "John Doe");
            y += 3;
            
            // Email field
            DrawLabel(buffer, 2, y, "Email:");
            DrawInputBox(buffer, 14, y, width - 16, "john@example.com");
            y += 3;
            
            // Password field
            DrawLabel(buffer, 2, y, "Password:");
            DrawInputBox(buffer, 14, y, width - 16, "••••••••");
            y += 3;
            
            // Country selector
            DrawLabel(buffer, 2, y, "Country:");
            DrawSelectBox(buffer, 14, y, width - 16, "United States ▼");
            y += 3;
            
            // Checkboxes
            DrawCheckbox(buffer, 2, y, "Subscribe to newsletter", true);
            y += 2;
            DrawCheckbox(buffer, 2, y, "I agree to the terms", false);
            y += 2;
            
            // Separator
            buffer.DrawText(new string('─', width - 4), 2, y, RGBA.FromHex("#444444"));
            y += 2;
            
            // Buttons
            DrawButton(buffer, width - 24, y, "Cancel", RGBA.FromHex("#666666"));
            DrawButton(buffer, width - 12, y, "Submit", RGBA.FromHex("#00aa00"));
            
            // Status bar
            buffer.FillRect(1, height - 2, width - 2, 1, RGBA.FromValues(0.15f, 0.15f, 0.2f));
            buffer.DrawText("Tab: Next field | Enter: Submit | Esc: Cancel", 2, height - 2, RGBA.FromHex("#888888"));
            
            // Render
            Console.Write(buffer.ToAnsiString());
            Console.Out.Flush();
            
            Console.ReadKey(true);
        }
        finally
        {
            state.ShowCursor();
            state.ExitAlternateScreen();
        }
    }

    private static void DrawBox(FrameBuffer buffer, int x, int y, int w, int h, string title, RGBA color)
    {
        buffer.SetCell(x, y, new Cell("╭", color));
        buffer.SetCell(x + w - 1, y, new Cell("╮", color));
        buffer.SetCell(x, y + h - 1, new Cell("╰", color));
        buffer.SetCell(x + w - 1, y + h - 1, new Cell("╯", color));
        
        for (int i = 1; i < w - 1; i++)
        {
            buffer.SetCell(x + i, y, new Cell("─", color));
            buffer.SetCell(x + i, y + h - 1, new Cell("─", color));
        }
        
        for (int j = 1; j < h - 1; j++)
        {
            buffer.SetCell(x, y + j, new Cell("│", color));
            buffer.SetCell(x + w - 1, y + j, new Cell("│", color));
        }
        
        if (!string.IsNullOrEmpty(title) && w > title.Length + 4)
        {
            buffer.DrawText($" {title} ", x + 2, y, color);
        }
    }

    private static void DrawLabel(FrameBuffer buffer, int x, int y, string label)
    {
        buffer.DrawText(label.PadRight(12), x, y, RGBA.FromHex("#aaaaaa"));
    }

    private static void DrawInputBox(FrameBuffer buffer, int x, int y, int w, string value)
    {
        // Box border
        buffer.SetCell(x, y - 1, new Cell("┌", RGBA.FromHex("#666666")));
        buffer.SetCell(x + w - 1, y - 1, new Cell("┐", RGBA.FromHex("#666666")));
        buffer.SetCell(x, y + 1, new Cell("└", RGBA.FromHex("#666666")));
        buffer.SetCell(x + w - 1, y + 1, new Cell("┘", RGBA.FromHex("#666666")));
        
        for (int i = 1; i < w - 1; i++)
        {
            buffer.SetCell(x + i, y - 1, new Cell("─", RGBA.FromHex("#666666")));
            buffer.SetCell(x + i, y + 1, new Cell("─", RGBA.FromHex("#666666")));
        }
        buffer.SetCell(x, y, new Cell("│", RGBA.FromHex("#666666")));
        buffer.SetCell(x + w - 1, y, new Cell("│", RGBA.FromHex("#666666")));
        
        // Value
        var displayValue = value.Length > w - 4 ? value[..(w - 4)] : value;
        buffer.DrawText(displayValue, x + 2, y, RGBA.White);
    }

    private static void DrawSelectBox(FrameBuffer buffer, int x, int y, int w, string value)
    {
        DrawInputBox(buffer, x, y, w, value);
    }

    private static void DrawCheckbox(FrameBuffer buffer, int x, int y, string label, bool isChecked)
    {
        var checkmark = isChecked ? "☑" : "☐";
        var color = isChecked ? RGBA.FromHex("#00ff00") : RGBA.FromHex("#666666");
        buffer.DrawText(checkmark, x, y, color);
        buffer.DrawText(label, x + 2, y, RGBA.FromHex("#dddddd"));
    }

    private static void DrawButton(FrameBuffer buffer, int x, int y, string text, RGBA color)
    {
        var w = text.Length + 4;
        buffer.SetCell(x, y, new Cell("╭", color));
        buffer.SetCell(x + w - 1, y, new Cell("╮", color));
        buffer.SetCell(x, y + 1, new Cell("│", color));
        buffer.SetCell(x + w - 1, y + 1, new Cell("│", color));
        buffer.SetCell(x, y + 2, new Cell("╰", color));
        buffer.SetCell(x + w - 1, y + 2, new Cell("╯", color));
        
        for (int i = 1; i < w - 1; i++)
        {
            buffer.SetCell(x + i, y, new Cell("─", color));
            buffer.SetCell(x + i, y + 2, new Cell("─", color));
        }
        
        buffer.DrawText(text, x + 2, y + 1, color);
    }
}
