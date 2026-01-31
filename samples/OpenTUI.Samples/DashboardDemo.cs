using OpenTUI.Core.Colors;
using OpenTUI.Core.Layout;
using OpenTUI.Core.Renderables;
using OpenTUI.Core.Rendering;
using BorderStyle = OpenTUI.Core.Renderables.BorderStyle;

namespace OpenTUI.Samples;

/// <summary>
/// Dashboard demo showing various widgets and layout techniques.
/// </summary>
public static class DashboardDemo
{
    public static void Run()
    {
        // Use alternate screen mode for clean display
        Console.Write("\x1b[?1049h"); // Enter alternate screen
        Console.Write("\x1b[?25l");   // Hide cursor
        
        try
        {
            var dashboard = CreateDashboard();
            dashboard.Layout.CalculateLayout(80, 24);
            
            var buffer = new FrameBuffer(80, 24);
            buffer.Clear(RGBA.FromInts(20, 20, 30));
            dashboard.Render(buffer, 0, 0);
            
            Console.Write("\x1b[H"); // Move to home position
            Console.Write(buffer.ToAnsiString());
            
            Console.ReadKey(true);
        }
        finally
        {
            Console.Write("\x1b[?25h");   // Show cursor
            Console.Write("\x1b[?1049l"); // Exit alternate screen
        }
    }

    private static BoxRenderable CreateDashboard()
    {
        var dashboard = new BoxRenderable { BorderStyle = BorderStyle.None };
        dashboard.BackgroundColor = RGBA.FromInts(20, 20, 30);
        dashboard.Layout.Width = FlexValue.Points(80);
        dashboard.Layout.Height = FlexValue.Points(24);
        dashboard.Layout.FlexDirection = FlexDirection.Column;
        
        // Header
        dashboard.Add(CreateHeader());
        
        // Main content area
        var mainContent = new BoxRenderable { BorderStyle = BorderStyle.None };
        mainContent.Layout.FlexDirection = FlexDirection.Row;
        mainContent.Layout.FlexGrow = 1;
        mainContent.Layout.Gap = 1;
        mainContent.Layout.Padding = new Edges(1);
        
        mainContent.Add(CreateSidebar());
        
        var content = new BoxRenderable { BorderStyle = BorderStyle.None };
        content.Layout.FlexDirection = FlexDirection.Column;
        content.Layout.FlexGrow = 1;
        content.Layout.Gap = 1;
        content.Add(CreateStatsRow());
        content.Add(CreateChartsRow());
        mainContent.Add(content);
        
        dashboard.Add(mainContent);
        dashboard.Add(CreateFooter());
        
        return dashboard;
    }

    private static BoxRenderable CreateHeader()
    {
        var header = new BoxRenderable { BorderStyle = BorderStyle.None };
        header.BackgroundColor = RGBA.FromInts(30, 30, 50);
        header.Layout.FlexDirection = FlexDirection.Row;
        header.Layout.JustifyContent = JustifyContent.SpaceBetween;
        header.Layout.AlignItems = AlignItems.Center;
        header.Layout.Padding = new Edges(0, 2, 0, 2);
        header.Layout.Height = FlexValue.Points(1);
        
        var title = new TextRenderable("📊 OpenTUI Dashboard") { ForegroundColor = RGBA.FromHex("#00aaff") };
        header.Add(title);
        
        var version = new TextRenderable("v1.0.0") { ForegroundColor = RGBA.FromHex("#666666") };
        header.Add(version);
        
        return header;
    }

    private static BoxRenderable CreateSidebar()
    {
        var sidebar = new BoxRenderable
        {
            BorderStyle = BorderStyle.Single,
            BorderColor = RGBA.FromHex("#333355")
        };
        sidebar.Layout.Width = FlexValue.Points(16);
        sidebar.Layout.FlexDirection = FlexDirection.Column;
        sidebar.Layout.Padding = new Edges(1);
        sidebar.Layout.Gap = 1;
        
        sidebar.Add(CreateMenuItem("🏠 Home", true));
        sidebar.Add(CreateMenuItem("📈 Analytics", false));
        sidebar.Add(CreateMenuItem("⚙️ Settings", false));
        sidebar.Add(CreateMenuItem("👤 Profile", false));
        
        return sidebar;
    }

    private static BoxRenderable CreateMenuItem(string text, bool isActive)
    {
        var item = new BoxRenderable { BorderStyle = BorderStyle.None };
        item.BackgroundColor = isActive ? RGBA.FromInts(50, 50, 80) : null;
        item.Layout.Padding = new Edges(0, 1, 0, 1);
        
        var color = isActive ? RGBA.FromHex("#00aaff") : RGBA.FromHex("#888888");
        var label = new TextRenderable(text) { ForegroundColor = color };
        item.Add(label);
        
        return item;
    }

    private static BoxRenderable CreateStatsRow()
    {
        var row = new BoxRenderable { BorderStyle = BorderStyle.None };
        row.Layout.FlexDirection = FlexDirection.Row;
        row.Layout.Gap = 1;
        row.Layout.Height = FlexValue.Points(5);
        
        row.Add(CreateStatCard("Users", "1,234", "+12%", RGBA.FromHex("#00ff88")));
        row.Add(CreateStatCard("Revenue", "$5,678", "+8%", RGBA.FromHex("#00aaff")));
        row.Add(CreateStatCard("Orders", "89", "-3%", RGBA.FromHex("#ff6644")));
        
        return row;
    }

    private static BoxRenderable CreateStatCard(string label, string value, string change, RGBA color)
    {
        var card = new BoxRenderable
        {
            BorderStyle = BorderStyle.Rounded,
            BorderColor = RGBA.FromHex("#333355")
        };
        card.Layout.FlexGrow = 1;
        card.Layout.Padding = new Edges(1);
        card.Layout.FlexDirection = FlexDirection.Column;
        
        card.Add(new TextRenderable(label) { ForegroundColor = RGBA.FromHex("#888888") });
        card.Add(new TextRenderable(value) { ForegroundColor = color });
        
        var changeColor = change.StartsWith("+") ? RGBA.FromHex("#00ff88") : RGBA.FromHex("#ff4444");
        card.Add(new TextRenderable(change) { ForegroundColor = changeColor });
        
        return card;
    }

    private static BoxRenderable CreateChartsRow()
    {
        var row = new BoxRenderable { BorderStyle = BorderStyle.None };
        row.Layout.FlexDirection = FlexDirection.Row;
        row.Layout.FlexGrow = 1;
        row.Layout.Gap = 1;
        
        row.Add(CreateBarChart());
        row.Add(CreateActivityFeed());
        
        return row;
    }

    private static BoxRenderable CreateBarChart()
    {
        var chart = new BoxRenderable
        {
            BorderStyle = BorderStyle.Rounded,
            BorderColor = RGBA.FromHex("#333355")
        };
        chart.Layout.FlexGrow = 1;
        chart.Layout.Padding = new Edges(1);
        chart.Layout.FlexDirection = FlexDirection.Column;
        chart.Layout.Gap = 1;
        
        chart.Add(new TextRenderable("Weekly Traffic") { ForegroundColor = RGBA.White });
        chart.Add(CreateBar("Mon", 0.7));
        chart.Add(CreateBar("Tue", 0.5));
        chart.Add(CreateBar("Wed", 0.9));
        chart.Add(CreateBar("Thu", 0.6));
        
        return chart;
    }

    private static BoxRenderable CreateBar(string label, double value)
    {
        var row = new BoxRenderable { BorderStyle = BorderStyle.None };
        row.Layout.FlexDirection = FlexDirection.Row;
        row.Layout.Gap = 1;
        
        var barWidth = (int)(value * 15);
        var bar = new string('█', barWidth) + new string('░', 15 - barWidth);
        
        row.Add(new TextRenderable(label.PadRight(4)) { ForegroundColor = RGBA.FromHex("#666666") });
        row.Add(new TextRenderable(bar) { ForegroundColor = RGBA.FromHex("#00aaff") });
        
        return row;
    }

    private static BoxRenderable CreateActivityFeed()
    {
        var feed = new BoxRenderable
        {
            BorderStyle = BorderStyle.Rounded,
            BorderColor = RGBA.FromHex("#333355")
        };
        feed.Layout.Width = FlexValue.Points(22);
        feed.Layout.Padding = new Edges(1);
        feed.Layout.FlexDirection = FlexDirection.Column;
        feed.Layout.Gap = 1;
        
        feed.Add(new TextRenderable("Recent Activity") { ForegroundColor = RGBA.White });
        feed.Add(CreateActivityItem("User signed up", "2m"));
        feed.Add(CreateActivityItem("Order placed", "5m"));
        feed.Add(CreateActivityItem("Payment received", "12m"));
        
        return feed;
    }

    private static BoxRenderable CreateActivityItem(string text, string time)
    {
        var row = new BoxRenderable { BorderStyle = BorderStyle.None };
        row.Layout.FlexDirection = FlexDirection.Row;
        row.Layout.JustifyContent = JustifyContent.SpaceBetween;
        
        row.Add(new TextRenderable($"• {text}") { ForegroundColor = RGBA.FromHex("#aaaaaa") });
        row.Add(new TextRenderable(time) { ForegroundColor = RGBA.FromHex("#666666") });
        
        return row;
    }

    private static BoxRenderable CreateFooter()
    {
        var footer = new BoxRenderable { BorderStyle = BorderStyle.None };
        footer.BackgroundColor = RGBA.FromInts(30, 30, 50);
        footer.Layout.Padding = new Edges(0, 2, 0, 2);
        footer.Layout.FlexDirection = FlexDirection.Row;
        footer.Layout.JustifyContent = JustifyContent.Center;
        footer.Layout.Height = FlexValue.Points(1);
        
        footer.Add(new TextRenderable("Press any key to exit") { ForegroundColor = RGBA.FromHex("#666666") });
        
        return footer;
    }
}
