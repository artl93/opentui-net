using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;
using OpenTUI.Core.Terminal;

namespace OpenTUI.Samples;

public static class VisualDemo
{
    public static void Run()
    {
        var state = new TerminalState();
        
        try
        {
            // Enter alternate screen and hide cursor
            state.EnterAlternateScreen();
            state.HideCursor();
            
            var size = TerminalSize.GetCurrent();
            var width = Math.Min(size.Width, 80);
            var height = Math.Min(size.Height, 24);
            
            var buffer = new FrameBuffer(width, height);
            
            // Fill background
            buffer.FillRect(0, 0, width, height, RGBA.FromValues(0.1f, 0.1f, 0.15f));
            
            // Draw main border
            DrawBox(buffer, 0, 0, width, height, "OpenTUI.NET Visual Demo", RGBA.Cyan);
            
            // Draw status boxes
            var boxWidth = (width - 8) / 3;
            DrawBox(buffer, 2, 2, boxWidth, 5, "Status", RGBA.Green);
            DrawBox(buffer, 4 + boxWidth, 2, boxWidth, 5, "Counter", RGBA.Yellow);
            DrawBox(buffer, 6 + boxWidth * 2, 2, boxWidth, 5, "Memory", RGBA.Magenta);
            
            // Fill box contents
            buffer.DrawText("● Online", 4, 4, RGBA.Green);
            buffer.DrawText("12345", 6 + boxWidth + boxWidth/2 - 2, 4, RGBA.White);
            buffer.DrawText("256 MB", 8 + boxWidth * 2 + boxWidth/2 - 3, 4, RGBA.White);
            
            // Draw a color palette
            DrawBox(buffer, 2, 8, width - 4, 6, "Color Palette", RGBA.White);
            var colors = new[] { RGBA.Red, RGBA.Green, RGBA.Blue, RGBA.Yellow, RGBA.Cyan, RGBA.Magenta };
            var colorNames = new[] { "Red", "Green", "Blue", "Yellow", "Cyan", "Magenta" };
            var colWidth = (width - 8) / colors.Length;
            for (int i = 0; i < colors.Length; i++)
            {
                var x = 4 + i * colWidth;
                buffer.FillRect(x, 10, colWidth - 1, 2, colors[i]);
                buffer.DrawText(colorNames[i], x, 12, colors[i]);
            }
            
            // Draw log area
            DrawBox(buffer, 2, 15, width - 4, 7, "Log Output", RGBA.FromValues(0.5f, 0.5f, 0.5f));
            buffer.DrawText("[INFO]  Application started", 4, 17, RGBA.FromValues(0.7f, 0.7f, 0.7f));
            buffer.DrawText("[WARN]  Config file missing", 4, 18, RGBA.Yellow);
            buffer.DrawText("[ERROR] Connection failed", 4, 19, RGBA.Red);
            buffer.DrawText("[INFO]  Retrying...", 4, 20, RGBA.FromValues(0.7f, 0.7f, 0.7f));
            
            // Status bar
            buffer.FillRect(1, height - 2, width - 2, 1, RGBA.FromValues(0.2f, 0.2f, 0.3f));
            buffer.DrawText("Press any key to exit", 3, height - 2, RGBA.White);
            buffer.DrawText("OpenTUI.NET v0.1", width - 20, height - 2, RGBA.Cyan);
            
            // Render to screen
            Console.Write(buffer.ToAnsiString());
            Console.Out.Flush();
            
            // Wait for key
            Console.ReadKey(true);
        }
        finally
        {
            // Always restore terminal
            state.ShowCursor();
            state.ExitAlternateScreen();
        }
    }
    
    private static void DrawBox(FrameBuffer buffer, int x, int y, int w, int h, string title, RGBA color)
    {
        // Corners
        buffer.SetCell(x, y, new Cell("╭", color));
        buffer.SetCell(x + w - 1, y, new Cell("╮", color));
        buffer.SetCell(x, y + h - 1, new Cell("╰", color));
        buffer.SetCell(x + w - 1, y + h - 1, new Cell("╯", color));
        
        // Horizontal edges
        for (int i = 1; i < w - 1; i++)
        {
            buffer.SetCell(x + i, y, new Cell("─", color));
            buffer.SetCell(x + i, y + h - 1, new Cell("─", color));
        }
        
        // Vertical edges
        for (int j = 1; j < h - 1; j++)
        {
            buffer.SetCell(x, y + j, new Cell("│", color));
            buffer.SetCell(x + w - 1, y + j, new Cell("│", color));
        }
        
        // Title
        if (!string.IsNullOrEmpty(title) && w > title.Length + 4)
        {
            buffer.DrawText($" {title} ", x + 2, y, color);
        }
    }
}
