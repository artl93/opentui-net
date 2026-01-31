using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;
using OpenTUI.Core.Terminal;

namespace OpenTUI.Samples;

/// <summary>
/// Dashboard demo showing various widgets and layout techniques.
/// </summary>
public static class DashboardDemo
{
    public static void Run()
    {
        var state = new TerminalState();
        
        try
        {
            state.EnterAlternateScreen();
            state.HideCursor();
            
            var size = TerminalSize.GetCurrent();
            var width = Math.Min(size.Width, 80);
            var height = Math.Min(size.Height, 24);
            
            var buffer = new FrameBuffer(width, height);
            
            // Fill background
            buffer.FillRect(0, 0, width, height, RGBA.FromInts(20, 20, 30));
            
            // Header
            buffer.FillRect(0, 0, width, 1, RGBA.FromInts(30, 30, 50));
            buffer.DrawText("📊 OpenTUI Dashboard", 2, 0, RGBA.FromHex("#00aaff"));
            buffer.DrawText("v1.0.0", width - 8, 0, RGBA.FromHex("#666666"));
            
            // Sidebar
            DrawBox(buffer, 1, 2, 16, height - 4, "Menu", RGBA.FromHex("#333355"));
            DrawMenuItem(buffer, 2, 4, "🏠 Home", true);
            DrawMenuItem(buffer, 2, 5, "📈 Analytics", false);
            DrawMenuItem(buffer, 2, 6, "⚙️ Settings", false);
            DrawMenuItem(buffer, 2, 7, "👤 Profile", false);
            DrawMenuItem(buffer, 2, 8, "❓ Help", false);
            
            // Stats cards
            var cardWidth = (width - 24) / 3;
            DrawStatCard(buffer, 19, 2, cardWidth, "Users", "1,234", "+12%", RGBA.FromHex("#00ff88"));
            DrawStatCard(buffer, 20 + cardWidth, 2, cardWidth, "Revenue", "$5,678", "+8%", RGBA.FromHex("#00aaff"));
            DrawStatCard(buffer, 21 + cardWidth * 2, 2, cardWidth, "Orders", "89", "-3%", RGBA.FromHex("#ff6644"));
            
            // Bar chart
            DrawBox(buffer, 19, 8, width - 42, height - 12, "Weekly Traffic", RGBA.FromHex("#333355"));
            DrawBar(buffer, 21, 10, "Mon", 0.7);
            DrawBar(buffer, 21, 11, "Tue", 0.5);
            DrawBar(buffer, 21, 12, "Wed", 0.9);
            DrawBar(buffer, 21, 13, "Thu", 0.6);
            DrawBar(buffer, 21, 14, "Fri", 0.8);
            
            // Activity feed
            DrawBox(buffer, width - 22, 8, 21, height - 12, "Activity", RGBA.FromHex("#333355"));
            DrawActivityItem(buffer, width - 20, 10, "User signed up", "2m");
            DrawActivityItem(buffer, width - 20, 11, "Order placed", "5m");
            DrawActivityItem(buffer, width - 20, 12, "Payment recv", "12m");
            DrawActivityItem(buffer, width - 20, 13, "Item shipped", "1h");
            DrawActivityItem(buffer, width - 20, 14, "Review added", "2h");
            
            // Footer
            buffer.FillRect(0, height - 1, width, 1, RGBA.FromInts(30, 30, 50));
            buffer.DrawText("Press any key to exit", (width - 21) / 2, height - 1, RGBA.FromHex("#666666"));
            
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

    private static void DrawMenuItem(FrameBuffer buffer, int x, int y, string text, bool isActive)
    {
        var color = isActive ? RGBA.FromHex("#00aaff") : RGBA.FromHex("#888888");
        if (isActive)
        {
            buffer.FillRect(x, y, 14, 1, RGBA.FromInts(50, 50, 80));
        }
        buffer.DrawText(text, x + 1, y, color);
    }

    private static void DrawStatCard(FrameBuffer buffer, int x, int y, int w, string label, string value, string change, RGBA color)
    {
        DrawBox(buffer, x, y, w, 5, "", RGBA.FromHex("#333355"));
        buffer.DrawText(label, x + 2, y + 1, RGBA.FromHex("#888888"));
        buffer.DrawText(value, x + 2, y + 2, color);
        
        var changeColor = change.StartsWith("+") ? RGBA.FromHex("#00ff88") : RGBA.FromHex("#ff4444");
        buffer.DrawText(change, x + 2, y + 3, changeColor);
    }

    private static void DrawBar(FrameBuffer buffer, int x, int y, string label, double value)
    {
        buffer.DrawText(label.PadRight(4), x, y, RGBA.FromHex("#666666"));
        
        var barWidth = 15;
        var filledWidth = (int)(value * barWidth);
        var bar = new string('█', filledWidth) + new string('░', barWidth - filledWidth);
        buffer.DrawText(bar, x + 5, y, RGBA.FromHex("#00aaff"));
    }

    private static void DrawActivityItem(FrameBuffer buffer, int x, int y, string text, string time)
    {
        buffer.DrawText("•", x, y, RGBA.FromHex("#666666"));
        buffer.DrawText(text, x + 2, y, RGBA.FromHex("#aaaaaa"));
        buffer.DrawText(time, x + 16, y, RGBA.FromHex("#666666"));
    }
}
