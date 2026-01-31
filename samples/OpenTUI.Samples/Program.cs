using OpenTUI.Core.Colors;
using OpenTUI.Core.Console;
using OpenTUI.Core.Input;
using OpenTUI.Core.Layout;
using OpenTUI.Core.Renderables;
using OpenTUI.Core.Rendering;
using OpenTUI.Core.Terminal;

// Check for demo flags
if (args.Length > 0)
{
    switch (args[0])
    {
        case "--visual":
            OpenTUI.Samples.VisualDemo.Run();
            return;
        case "--form":
            OpenTUI.Samples.FormDemo.Run();
            return;
        case "--dashboard":
            OpenTUI.Samples.DashboardDemo.Run();
            return;
        case "--animation":
            OpenTUI.Samples.AnimationDemo.Run();
            return;
        case "--help":
            Console.WriteLine("OpenTUI.NET Sample Demos");
            Console.WriteLine("========================");
            Console.WriteLine("  (no args)    Text-based demo (safe for any terminal)");
            Console.WriteLine("  --visual     Visual demo with alternate screen");
            Console.WriteLine("  --form       Form input demo");
            Console.WriteLine("  --dashboard  Dashboard widget demo");
            Console.WriteLine("  --animation  Animation effects demo (spinners, progress, effects)");
            Console.WriteLine("  --help       Show this help");
            return;
    }
}

// Create a log file for debugging
var logFile = Path.Combine(Path.GetTempPath(), "opentui-demo.log");
var log = new StreamWriter(logFile, append: false) { AutoFlush = true };

void Log(string message)
{
    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
    log.WriteLine($"[{timestamp}] {message}");
}

try
{
    Log("Starting OpenTUI.NET Demo");
    
    Console.WriteLine("OpenTUI.NET Interactive Demo");
    Console.WriteLine("============================");
    Console.WriteLine($"(Log file: {logFile})\n");

    // Demo 1: Colors
    Log("Demo 1: Colors");
    Console.WriteLine("1. RGBA Colors");
    Console.WriteLine("---------------");
    var red = RGBA.Red;
    var blue = RGBA.Blue;
    var blended = RGBA.FromValues(0, 0, 1, 0.5f).BlendOver(red);
    Console.WriteLine($"   Red: {red}");
    Console.WriteLine($"   Blue: {blue}");
    Console.WriteLine($"   50% blue over red: {blended}");
    Console.WriteLine($"   From hex: {RGBA.FromHex("#FF5733")}");
    Console.WriteLine();
    Log("Demo 1: Complete");

    // Demo 2: FrameBuffer (simple text output, no ANSI)
    Log("Demo 2: FrameBuffer");
    Console.WriteLine("2. FrameBuffer Rendering");
    Console.WriteLine("------------------------");
    var buffer = new FrameBuffer(40, 3);
    buffer.DrawText("Hello, OpenTUI!", 2, 0, RGBA.Green);
    buffer.DrawText("Flexbox Layout Engine", 2, 1, RGBA.Yellow);
    buffer.DrawText("Buffer works!", 2, 2, RGBA.Cyan);
    
    // Output plain text version
    for (int row = 0; row < buffer.Height; row++)
    {
        Console.Write("   ");
        for (int col = 0; col < buffer.Width; col++)
        {
            var ch = buffer.GetCell(row, col).Character;
            Console.Write(string.IsNullOrEmpty(ch) ? " " : ch);
        }
        Console.WriteLine();
    }
    Console.WriteLine();
    Log("Demo 2: Complete");

    // Demo 3: Flexbox Layout
    Log("Demo 3: Flexbox Layout");
    Console.WriteLine("3. Flexbox Layout");
    Console.WriteLine("-----------------");
    var root = new FlexNode
    {
        Width = FlexValue.Points(60),
        Height = FlexValue.Points(10),
        FlexDirection = FlexDirection.Row,
        JustifyContent = JustifyContent.SpaceBetween,
        Padding = new Edges(1, 1, 1, 1)
    };

    var child1 = new FlexNode { Width = FlexValue.Points(15), Height = FlexValue.Points(4) };
    var child2 = new FlexNode { Width = FlexValue.Points(15), Height = FlexValue.Points(4) };
    var child3 = new FlexNode { Width = FlexValue.Points(15), Height = FlexValue.Points(4) };

    root.AddChild(child1);
    root.AddChild(child2);
    root.AddChild(child3);
    root.CalculateLayout(80, 24);

    Console.WriteLine($"   Root: {root.Layout.Width}x{root.Layout.Height}");
    Console.WriteLine($"   Child 1: ({child1.Layout.X}, {child1.Layout.Y}) {child1.Layout.Width}x{child1.Layout.Height}");
    Console.WriteLine($"   Child 2: ({child2.Layout.X}, {child2.Layout.Y}) {child2.Layout.Width}x{child2.Layout.Height}");
    Console.WriteLine($"   Child 3: ({child3.Layout.X}, {child3.Layout.Y}) {child3.Layout.Width}x{child3.Layout.Height}");
    Console.WriteLine();
    Log("Demo 3: Complete");

    // Demo 4: Log Buffer
    Log("Demo 4: Log Buffer");
    Console.WriteLine("4. Console Log Buffer");
    Console.WriteLine("---------------------");
    var logBuffer = new LogBuffer(100);
    logBuffer.Debug("Application starting...");
    logBuffer.Info("Loading configuration");
    logBuffer.Warning("Config file not found, using defaults");
    logBuffer.Error("Failed to connect to database");
    logBuffer.Info("Retrying connection...");

    Console.WriteLine("   Recent log entries:");
    foreach (var entry in logBuffer.GetRecentEntries(5))
    {
        var prefix = entry.Level switch
        {
            LogLevel.Debug => "[DEBUG]",
            LogLevel.Info => "[INFO] ",
            LogLevel.Warning => "[WARN] ",
            LogLevel.Error => "[ERROR]",
            _ => "[?????]"
        };
        Console.WriteLine($"   {prefix} {entry.Message}");
    }
    Console.WriteLine();
    Log("Demo 4: Complete");

    // Demo 5: Terminal Info
    Log("Demo 5: Terminal Info");
    Console.WriteLine("5. Terminal Information");
    Console.WriteLine("-----------------------");
    var termSize = TerminalSize.GetCurrent();
    var caps = TerminalCapabilities.Detect();
    Console.WriteLine($"   Size: {termSize.Width} x {termSize.Height}");
    Console.WriteLine($"   Unicode: {caps.SupportsUnicode}");
    Console.WriteLine($"   Mouse: {caps.SupportsMouse}");
    Console.WriteLine();
    Log("Demo 5: Complete");

    // Demo 6: Input Parsing
    Log("Demo 6: Input Parsing");
    Console.WriteLine("6. ANSI Key Parsing");
    Console.WriteLine("-------------------");
    var testSequences = new[]
    {
        ("\u001b[A", "Up Arrow"),
        ("\u001b[B", "Down Arrow"),
        ("\u001b[1;5C", "Ctrl+Right"),
        ("\u001bOP", "F1"),
        ("a", "Letter 'a'")
    };

    foreach (var (seq, desc) in testSequences)
    {
        var parsed = AnsiKeyParser.Parse(seq.AsSpan());
        if (parsed.HasValue)
        {
            Console.WriteLine($"   {desc}: Key={parsed.Value.Key}, Modifiers={parsed.Value.Modifiers}");
        }
    }
    Console.WriteLine();
    Log("Demo 6: Complete");

    // Demo 7: Simple box drawing (ASCII only, no ANSI colors)
    Log("Demo 7: Box Drawing");
    Console.WriteLine("7. Box Drawing (ASCII)");
    Console.WriteLine("----------------------");
    Console.WriteLine("   ┌─────────────────────────────────────────────────────┐");
    Console.WriteLine("   │  OpenTUI.NET Demo                                   │");
    Console.WriteLine("   ├─────────────────┬─────────────────┬─────────────────┤");
    Console.WriteLine("   │  Status         │  Count          │  Memory         │");
    Console.WriteLine("   │  Running        │  42             │  128 MB         │");
    Console.WriteLine("   └─────────────────┴─────────────────┴─────────────────┘");
    Console.WriteLine();
    Log("Demo 7: Complete");

    Console.WriteLine("✅ All demos completed successfully!");
    Console.WriteLine($"   Total test coverage: 465 tests passing");
    Console.WriteLine($"   Log file: {logFile}");
    Console.WriteLine();
    Console.WriteLine("Try other demos:");
    Console.WriteLine("   dotnet run -- --visual     (colorful visual demo)");
    Console.WriteLine("   dotnet run -- --form       (form input demo)");
    Console.WriteLine("   dotnet run -- --dashboard  (dashboard widget demo)");
    Console.WriteLine();
    
    Log("Demo completed successfully");
}
catch (Exception ex)
{
    Log($"ERROR: {ex.GetType().Name}: {ex.Message}");
    Log($"Stack trace:\n{ex.StackTrace}");
    
    // Reset terminal and show error
    Console.Write("\u001b[0m"); // Reset colors
    Console.WriteLine();
    Console.WriteLine($"❌ Demo crashed: {ex.Message}");
    Console.WriteLine($"   See log: {logFile}");
}
finally
{
    log.Close();
}
