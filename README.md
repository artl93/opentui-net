# OpenTUI.NET

A pure C# terminal UI library with flexbox layout, inspired by [OpenTUI](https://github.com/anomalyco/opentui). Build rich, interactive terminal applications with a React-like component model. This is experimental right now and written using GitHub Copilot CLI. 

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

![OpenTUI-Net Demos](https://github.com/user-attachments/assets/e10fed14-a95d-4011-a580-28da96c9e27f)

## Features

- **Pure C# Flexbox Layout** - No native dependencies, cross-platform
- **Buffer-Based Rendering** - Efficient double-buffered terminal output
- **Rich Renderables** - Text, Box, Input, Select, Slider, ScrollBox, and more
- **Reactive Components** - State management with automatic re-rendering
- **Full Color Support** - True color (24-bit), 256-color, and 16-color modes
- **Input Handling** - Keyboard events with modifier key support
- **Console Overlay** - Capture and display Console.WriteLine output

## Installation - 

Not available right now as this is an experoment - clone the repo

> **Note**: Requires .NET 10.0 or later

## Quick Start

### Hello World

```csharp
using OpenTUI.Renderables;
using OpenTUI.Rendering;
using OpenTUI.Layout;

// Create a simple text renderable
var text = new TextRenderable("Hello, OpenTUI!");

// Create a renderer and draw
var renderer = new CliRenderer();
renderer.Render(text);
```

### Styled Box with Text

```csharp
using OpenTUI.Core;
using OpenTUI.Renderables;
using OpenTUI.Layout;

var box = new BoxRenderable
{
    BorderStyle = BorderStyle.Rounded,
    BorderColor = RGBA.FromHex("#00ff00"),
    Padding = new Edges(1, 2, 1, 2),
    Children = 
    {
        new TextRenderable("Welcome to OpenTUI.NET!")
        {
            Color = RGBA.FromHex("#ffffff")
        }
    }
};

var renderer = new CliRenderer();
renderer.Render(box);
```

### Interactive Input

```csharp
using OpenTUI.Renderables;
using OpenTUI.Input;

var input = new InputRenderable
{
    Placeholder = "Enter your name...",
    Width = FlexValue.Fixed(30)
};

var handler = new InputHandler();
handler.OnKey += (sender, e) =>
{
    if (e.Key == Key.Enter)
    {
        Console.WriteLine($"Hello, {input.Value}!");
    }
};

// Run input loop
await handler.StartAsync();
```

## Layout System

OpenTUI.NET uses a flexbox-inspired layout system:

```csharp
var container = new BoxRenderable
{
    FlexDirection = FlexDirection.Row,
    JustifyContent = JustifyContent.SpaceBetween,
    AlignItems = AlignItems.Center,
    Width = FlexValue.Percent(100),
    Height = FlexValue.Fixed(10),
    Children =
    {
        new TextRenderable("Left"),
        new TextRenderable("Center"),
        new TextRenderable("Right")
    }
};
```

### FlexValue Types

| Type | Example | Description |
|------|---------|-------------|
| `Auto` | `FlexValue.Auto` | Size to content |
| `Fixed` | `FlexValue.Fixed(20)` | Fixed character width |
| `Percent` | `FlexValue.Percent(50)` | Percentage of parent |
| `Flex` | `FlexValue.Flex(1)` | Flexible space distribution |

### Flex Properties

- `FlexDirection` - Row, Column, RowReverse, ColumnReverse
- `JustifyContent` - FlexStart, FlexEnd, Center, SpaceBetween, SpaceAround, SpaceEvenly
- `AlignItems` - FlexStart, FlexEnd, Center, Stretch, Baseline
- `AlignSelf` - Override parent's AlignItems for specific child
- `FlexWrap` - NoWrap, Wrap, WrapReverse
- `Gap` - Spacing between children

## Renderables

### TextRenderable

```csharp
var text = new TextRenderable("Hello World")
{
    Color = RGBA.FromHex("#ff0000"),
    Bold = true,
    Italic = true,
    Underline = true
};
```

### BoxRenderable

```csharp
var box = new BoxRenderable
{
    BorderStyle = BorderStyle.Double,  // None, Single, Double, Rounded, Heavy, Custom
    BorderColor = RGBA.White,
    BackgroundColor = RGBA.FromRgb(30, 30, 30),
    Padding = new Edges(1),  // All sides
    Margin = new Edges(0, 1, 0, 1),  // Top, Right, Bottom, Left
};
```

### InputRenderable

```csharp
var input = new InputRenderable
{
    Value = "Initial value",
    Placeholder = "Type here...",
    Password = true,  // Mask input with dots
    MaxLength = 50
};
```

### SelectRenderable

```csharp
var select = new SelectRenderable
{
    Options = new[] { "Option 1", "Option 2", "Option 3" },
    SelectedIndex = 0,
    HighlightColor = RGBA.FromHex("#00ff00")
};
```

### SliderRenderable

```csharp
var slider = new SliderRenderable
{
    Value = 50,
    Min = 0,
    Max = 100,
    Width = FlexValue.Fixed(20),
    FilledColor = RGBA.Green,
    EmptyColor = RGBA.Gray
};
```

### ScrollBoxRenderable

```csharp
var scrollBox = new ScrollBoxRenderable
{
    Height = FlexValue.Fixed(10),
    ScrollOffset = 0,
    Children = { /* many children */ }
};
```

## Reactive Components

OpenTUI.NET includes a React-inspired reactive system:

### State Management

```csharp
using OpenTUI.Reactive.Primitives;

var count = new State<int>(0);

count.Changed += (sender, args) =>
{
    Console.WriteLine($"Count changed from {args.OldValue} to {args.NewValue}");
};

count.Value = 1;  // Triggers Changed event
count.Set(2);     // Also triggers Changed event
```

### Computed Values

```csharp
var firstName = new State<string>("John");
var lastName = new State<string>("Doe");

var fullName = new Computed<string>(
    () => $"{firstName.Value} {lastName.Value}",
    firstName, lastName  // Dependencies
);

Console.WriteLine(fullName.Value);  // "John Doe"
firstName.Value = "Jane";
Console.WriteLine(fullName.Value);  // "Jane Doe" (auto-updated)
```

### Effects

```csharp
var searchQuery = new State<string>("");

var effect = new Effect(() =>
{
    var query = searchQuery.Value;
    if (!string.IsNullOrEmpty(query))
    {
        // Perform search...
        Console.WriteLine($"Searching for: {query}");
    }
    
    // Return cleanup function
    return () => Console.WriteLine("Effect cleanup");
}, searchQuery);
```

### Components

```csharp
using OpenTUI.Reactive.Components;

public class CounterComponent : Component
{
    private State<int> _count = null!;

    protected override void OnMount()
    {
        _count = UseState(0);
    }

    protected override IRenderable Render()
    {
        return new BoxRenderable
        {
            Children =
            {
                new TextRenderable($"Count: {_count.Value}"),
                new TextRenderable("Press +/- to change")
            }
        };
    }

    public void Increment() => _count.Value++;
    public void Decrement() => _count.Value--;
}
```

## Color Support

```csharp
// From hex
var red = RGBA.FromHex("#ff0000");
var green = RGBA.FromHex("#00ff00ff");  // With alpha

// From RGB (0-255)
var blue = RGBA.FromRgb(0, 0, 255);
var semiTransparent = RGBA.FromRgba(255, 255, 255, 128);

// From normalized values (0.0-1.0)
var color = RGBA.FromValues(1.0f, 0.5f, 0.0f, 1.0f);

// Predefined colors
var white = RGBA.White;
var black = RGBA.Black;
var transparent = RGBA.Transparent;

// Convert to ANSI
string ansi = color.ToAnsi();  // "\x1b[38;2;255;128;0m"
```

## Console Overlay

Capture Console.WriteLine output within your TUI:

```csharp
using OpenTUI.Core.Console;

var overlay = new ConsoleOverlay(maxLines: 100);

// Start capturing
overlay.StartCapture();

// These will be captured
Console.WriteLine("Log message 1");
Console.WriteLine("Log message 2");

// Get captured content
var logs = overlay.GetContent();

// Stop capturing
overlay.StopCapture();
```

## Running the Demos

```bash
# Text-based demo (safe for any terminal)
dotnet run --project samples/OpenTUI.Samples

# Visual demo (uses alternate screen mode)
dotnet run --project samples/OpenTUI.Samples -- --visual
```

## Architecture

```
OpenTUI.NET
├── OpenTUI.Core          # Colors, ANSI, FrameBuffer, Console overlay
├── OpenTUI.Layout        # Flexbox layout engine
├── OpenTUI.Rendering     # Renderable base, CliRenderer
├── OpenTUI.Renderables   # Text, Box, Input, Select, etc.
├── OpenTUI.Input         # Keyboard handling
└── OpenTUI.Reactive      # State, Computed, Effects, Components
```

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

Current test coverage: **465 tests** across all projects.

## License

MIT License - see [LICENSE](LICENSE) for details.

## Acknowledgments

- Inspired by [OpenTUI](https://github.com/anomalyco/opentui) by Anomaly Co.
- Layout system inspired by CSS Flexbox
- Reactive system inspired by React hooks and Solid.js signals

## Contributing

Contributions are welcome! Please read our contributing guidelines before submitting PRs.
