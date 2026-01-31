using OpenTUI.Core.Colors;
using OpenTUI.Core.Layout;
using OpenTUI.Core.Renderables;
using OpenTUI.Core.Rendering;
using BorderStyle = OpenTUI.Core.Renderables.BorderStyle;

namespace OpenTUI.Samples;

/// <summary>
/// Interactive form demo with multiple input types.
/// </summary>
public static class FormDemo
{
    public static void Run()
    {
        Console.Clear();
        Console.WriteLine("=== OpenTUI.NET Form Demo ===\n");

        // Create form layout
        var form = CreateForm();
        
        // Calculate layout
        form.Layout.CalculateLayout(60, 20);
        
        // Render to buffer
        var buffer = new FrameBuffer(60, 20);
        form.Render(buffer, 0, 0);
        Console.WriteLine(buffer.ToSimpleAnsiString());
        
        Console.WriteLine("\n(This is a static preview - interactive mode coming soon)");
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    private static BoxRenderable CreateForm()
    {
        var form = new BoxRenderable
        {
            BorderStyle = BorderStyle.Rounded,
            BorderColor = RGBA.FromHex("#5588ff")
        };
        form.Layout.Width = FlexValue.Points(58);
        form.Layout.Height = FlexValue.Auto;
        form.Layout.FlexDirection = FlexDirection.Column;
        form.Layout.Padding = new Edges(1, 2, 1, 2);
        form.Layout.Gap = 1;
        
        // Title
        var title = new TextRenderable("User Registration Form") { ForegroundColor = RGBA.White };
        form.Add(title);
        
        var divider = new TextRenderable("─────────────────────────────────") { ForegroundColor = RGBA.FromHex("#444444") };
        form.Add(divider);
        
        // Fields
        form.Add(CreateField("Name:", "John Doe"));
        form.Add(CreateField("Email:", "john@example.com"));
        form.Add(CreateField("Password:", "••••••••"));
        
        // Checkboxes
        form.Add(CreateCheckbox("Subscribe to newsletter", true));
        form.Add(CreateCheckbox("I agree to the terms", false));
        
        var divider2 = new TextRenderable("─────────────────────────────────") { ForegroundColor = RGBA.FromHex("#444444") };
        form.Add(divider2);
        
        // Button row
        form.Add(CreateButtonRow());
        
        return form;
    }

    private static BoxRenderable CreateField(string label, string value)
    {
        var row = new BoxRenderable { BorderStyle = BorderStyle.None };
        row.Layout.FlexDirection = FlexDirection.Row;
        row.Layout.Gap = 1;
        
        var labelText = new TextRenderable(label.PadRight(12)) { ForegroundColor = RGBA.FromHex("#aaaaaa") };
        row.Add(labelText);
        
        var valueBox = new BoxRenderable { BorderStyle = BorderStyle.Single, BorderColor = RGBA.FromHex("#666666") };
        valueBox.Layout.Padding = new Edges(0, 1, 0, 1);
        
        var valueText = new TextRenderable(value.PadRight(25)) { ForegroundColor = RGBA.White };
        valueBox.Add(valueText);
        row.Add(valueBox);
        
        return row;
    }

    private static BoxRenderable CreateCheckbox(string label, bool isChecked)
    {
        var row = new BoxRenderable { BorderStyle = BorderStyle.None };
        row.Layout.FlexDirection = FlexDirection.Row;
        row.Layout.Gap = 1;
        
        var checkmark = isChecked ? "☑" : "☐";
        var color = isChecked ? RGBA.FromHex("#00ff00") : RGBA.FromHex("#666666");
        
        var check = new TextRenderable(checkmark) { ForegroundColor = color };
        row.Add(check);
        
        var labelText = new TextRenderable(label) { ForegroundColor = RGBA.FromHex("#dddddd") };
        row.Add(labelText);
        
        return row;
    }

    private static BoxRenderable CreateButtonRow()
    {
        var row = new BoxRenderable { BorderStyle = BorderStyle.None };
        row.Layout.FlexDirection = FlexDirection.Row;
        row.Layout.JustifyContent = JustifyContent.FlexEnd;
        row.Layout.Gap = 2;
        
        row.Add(CreateButton("Cancel", RGBA.FromHex("#666666")));
        row.Add(CreateButton("Submit", RGBA.FromHex("#00aa00")));
        
        return row;
    }

    private static BoxRenderable CreateButton(string text, RGBA color)
    {
        var button = new BoxRenderable
        {
            BorderStyle = BorderStyle.Rounded,
            BorderColor = color
        };
        button.Layout.Padding = new Edges(0, 2, 0, 2);
        
        var label = new TextRenderable(text) { ForegroundColor = color };
        button.Add(label);
        
        return button;
    }
}
