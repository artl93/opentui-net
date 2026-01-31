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
            var width = size.Width;
            var height = size.Height;
            
            var buffer = new FrameBuffer(width, height);
            
            // Fill background
            buffer.FillRect(0, 0, width, height, RGBA.FromValues(0.05f, 0.05f, 0.1f));
            
            // Draw main form border
            DrawBox(buffer, 0, 0, width, height, "User Registration Form", RGBA.FromHex("#5588ff"));
            
            // Calculate form content area
            var formWidth = Math.Min(width - 4, 70);
            var formX = (width - formWidth) / 2;
            
            // Form fields
            int y = 3;
            int fieldWidth = formWidth - 14;
            
            // Name field
            DrawLabel(buffer, formX, y, "Name:");
            DrawInputBox(buffer, formX + 12, y, fieldWidth, "John Doe");
            y += 3;
            
            // Email field
            DrawLabel(buffer, formX, y, "Email:");
            DrawInputBox(buffer, formX + 12, y, fieldWidth, "john@example.com");
            y += 3;
            
            // Password field
            DrawLabel(buffer, formX, y, "Password:");
            DrawInputBox(buffer, formX + 12, y, fieldWidth, "••••••••");
            y += 3;
            
            // Country selector
            DrawLabel(buffer, formX, y, "Country:");
            DrawSelectBox(buffer, formX + 12, y, fieldWidth, "United States ▼");
            y += 4;
            
            // Checkboxes
            DrawCheckbox(buffer, formX, y, "Subscribe to newsletter", true);
            y += 2;
            DrawCheckbox(buffer, formX, y, "I agree to the terms and conditions", false);
            y += 3;
            
            // Separator
            buffer.DrawText(new string('─', formWidth), formX, y, RGBA.FromHex("#444444"));
            y += 2;
            
            // Buttons - positioned relative to form width
            var cancelX = formX + formWidth - 22;
            var submitX = formX + formWidth - 10;
            DrawButton(buffer, cancelX, y, "Cancel", RGBA.FromHex("#888888"));
            DrawButton(buffer, submitX, y, "Submit", RGBA.FromHex("#00cc00"));
            
            // Status bar at bottom
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
        buffer.DrawText(label.PadRight(11), x, y, RGBA.FromHex("#aaaaaa"));
    }

    private static void DrawInputBox(FrameBuffer buffer, int x, int y, int w, string value)
    {
        var boxColor = RGBA.FromHex("#555555");
        
        // Box border (single line, not 3 rows)
        buffer.SetCell(x, y, new Cell("│", boxColor));
        buffer.SetCell(x + w - 1, y, new Cell("│", boxColor));
        
        // Top and bottom borders
        for (int i = 1; i < w - 1; i++)
        {
            buffer.SetCell(x + i, y - 1, new Cell("─", boxColor));
            buffer.SetCell(x + i, y + 1, new Cell("─", boxColor));
        }
        
        // Corners
        buffer.SetCell(x, y - 1, new Cell("┌", boxColor));
        buffer.SetCell(x + w - 1, y - 1, new Cell("┐", boxColor));
        buffer.SetCell(x, y + 1, new Cell("└", boxColor));
        buffer.SetCell(x + w - 1, y + 1, new Cell("┘", boxColor));
        
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
        var w = text.Length + 2;
        buffer.DrawText($"[{text}]", x, y, color);
    }
}
