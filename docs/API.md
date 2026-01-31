# OpenTUI.NET API Reference

This document provides detailed API documentation for all public types in OpenTUI.NET.

## Table of Contents

- [Core](#core)
  - [RGBA](#rgba)
  - [Cell](#cell)
  - [FrameBuffer](#framebuffer)
- [Layout](#layout)
  - [FlexValue](#flexvalue)
  - [FlexNode](#flexnode)
  - [Edges](#edges)
- [Renderables](#renderables)
  - [IRenderable](#irenderable)
  - [Renderable](#renderable)
  - [TextRenderable](#textrenderable)
  - [BoxRenderable](#boxrenderable)
  - [InputRenderable](#inputrenderable)
  - [SelectRenderable](#selectrenderable)
  - [SliderRenderable](#sliderrenderable)
  - [ScrollBoxRenderable](#scrollboxrenderable)
- [Rendering](#rendering)
  - [CliRenderer](#clirenderer)
- [Input](#input)
  - [Key](#key)
  - [KeyModifiers](#keymodifiers)
  - [KeyEvent](#keyevent)
  - [InputHandler](#inputhandler)
- [Reactive](#reactive)
  - [State\<T\>](#statet)
  - [Computed\<T\>](#computedt)
  - [Effect](#effect)
  - [Component](#component)

---

## Core

### RGBA

Represents an RGBA color with normalized float components (0.0 to 1.0).

```csharp
namespace OpenTUI.Core;

public readonly struct RGBA
{
    public float R { get; }  // Red component (0.0-1.0)
    public float G { get; }  // Green component (0.0-1.0)
    public float B { get; }  // Blue component (0.0-1.0)
    public float A { get; }  // Alpha component (0.0-1.0)
}
```

#### Static Factory Methods

| Method | Description |
|--------|-------------|
| `FromValues(float r, float g, float b, float a = 1.0f)` | Create from normalized float values |
| `FromRgb(byte r, byte g, byte b)` | Create from 0-255 byte values |
| `FromRgba(byte r, byte g, byte b, byte a)` | Create from 0-255 byte values with alpha |
| `FromHex(string hex)` | Parse from hex string (#RGB, #RRGGBB, #RRGGBBAA) |

#### Instance Methods

| Method | Description |
|--------|-------------|
| `BlendOver(RGBA background)` | Alpha-blend this color over a background |
| `ToAnsi()` | Convert to ANSI escape sequence for foreground color |
| `ToAnsiBackground()` | Convert to ANSI escape sequence for background color |
| `ToByte()` | Get components as (R, G, B, A) bytes |

#### Predefined Colors

```csharp
RGBA.White       // #FFFFFF
RGBA.Black       // #000000
RGBA.Transparent // #00000000
RGBA.Red         // #FF0000
RGBA.Green       // #00FF00
RGBA.Blue        // #0000FF
RGBA.Yellow      // #FFFF00
RGBA.Cyan        // #00FFFF
RGBA.Magenta     // #FF00FF
RGBA.Gray        // #808080
```

---

### Cell

Represents a single terminal cell with character and styling.

```csharp
namespace OpenTUI.Core;

public readonly struct Cell
{
    public string? Character { get; }
    public RGBA Foreground { get; }
    public RGBA Background { get; }
    public CellStyle Style { get; }
}

[Flags]
public enum CellStyle
{
    None = 0,
    Bold = 1,
    Italic = 2,
    Underline = 4,
    Strikethrough = 8,
    Dim = 16
}
```

---

### FrameBuffer

A 2D buffer of cells for terminal rendering.

```csharp
namespace OpenTUI.Core;

public class FrameBuffer
{
    public FrameBuffer(int width, int height);
    
    public int Width { get; }
    public int Height { get; }
    
    public Cell GetCell(int row, int col);
    public void SetCell(int col, int row, Cell cell);
    public void SetCell(int col, int row, string character, RGBA foreground);
    public void SetCell(int col, int row, string character, RGBA foreground, RGBA background);
    
    public void Clear();
    public void Clear(RGBA background);
    
    public void DrawText(string text, int x, int y, RGBA color);
    public void DrawText(string text, int x, int y, RGBA foreground, RGBA background);
    
    public void DrawRect(int x, int y, int width, int height, RGBA color);
    public void DrawBox(int x, int y, int width, int height, RGBA color, BorderStyle style);
    
    public void Blit(FrameBuffer source, int destX, int destY);
    
    public string ToAnsiString();       // With cursor positioning
    public string ToSimpleAnsiString(); // Without cursor positioning
}
```

---

## Layout

### FlexValue

Represents a flexible dimension value.

```csharp
namespace OpenTUI.Layout;

public readonly struct FlexValue
{
    public FlexValueType Type { get; }
    public float Value { get; }
}

public enum FlexValueType
{
    Auto,     // Size to content
    Fixed,    // Fixed size in characters
    Percent,  // Percentage of parent
    Flex      // Flexible distribution
}
```

#### Static Factory Methods

| Method | Description |
|--------|-------------|
| `FlexValue.Auto` | Size based on content |
| `FlexValue.Fixed(float value)` | Fixed character width |
| `FlexValue.Points(float value)` | Alias for Fixed |
| `FlexValue.Percent(float value)` | Percentage (0-100) of parent |
| `FlexValue.Flex(float factor)` | Flexible with grow factor |

---

### FlexNode

A node in the flexbox layout tree.

```csharp
namespace OpenTUI.Layout;

public class FlexNode
{
    // Dimensions
    public FlexValue Width { get; set; }
    public FlexValue Height { get; set; }
    public FlexValue MinWidth { get; set; }
    public FlexValue MinHeight { get; set; }
    public FlexValue MaxWidth { get; set; }
    public FlexValue MaxHeight { get; set; }
    
    // Flex container properties
    public FlexDirection FlexDirection { get; set; }
    public JustifyContent JustifyContent { get; set; }
    public AlignItems AlignItems { get; set; }
    public AlignContent AlignContent { get; set; }
    public FlexWrap FlexWrap { get; set; }
    public float Gap { get; set; }
    public float RowGap { get; set; }
    public float ColumnGap { get; set; }
    
    // Flex item properties
    public FlexValue FlexGrow { get; set; }
    public FlexValue FlexShrink { get; set; }
    public FlexValue FlexBasis { get; set; }
    public AlignSelf AlignSelf { get; set; }
    
    // Spacing
    public Edges Padding { get; set; }
    public Edges Margin { get; set; }
    
    // Positioning
    public Position Position { get; set; }
    public float? Left { get; set; }
    public float? Right { get; set; }
    public float? Top { get; set; }
    public float? Bottom { get; set; }
    
    // Children
    public IReadOnlyList<FlexNode> Children { get; }
    public void AddChild(FlexNode child);
    public void RemoveChild(FlexNode child);
    
    // Layout calculation
    public void CalculateLayout(float containerWidth, float containerHeight);
    
    // Computed layout (after CalculateLayout)
    public LayoutResult Layout { get; }
}

public readonly struct LayoutResult
{
    public float X { get; }
    public float Y { get; }
    public float Width { get; }
    public float Height { get; }
}
```

---

### Edges

Represents spacing on all four sides.

```csharp
namespace OpenTUI.Layout;

public readonly struct Edges
{
    public float Top { get; }
    public float Right { get; }
    public float Bottom { get; }
    public float Left { get; }
    
    public Edges(float all);
    public Edges(float vertical, float horizontal);
    public Edges(float top, float right, float bottom, float left);
}
```

---

## Renderables

### IRenderable

Base interface for all renderable elements.

```csharp
namespace OpenTUI.Rendering;

public interface IRenderable
{
    FlexNode LayoutNode { get; }
    IReadOnlyList<IRenderable> Children { get; }
    
    void Render(FrameBuffer buffer, int x, int y, int width, int height);
}
```

---

### Renderable

Abstract base class with common renderable functionality.

```csharp
namespace OpenTUI.Rendering;

public abstract class Renderable : IRenderable
{
    // Layout properties (delegated to internal FlexNode)
    public FlexValue Width { get; set; }
    public FlexValue Height { get; set; }
    public FlexValue Flex { get; set; }
    public FlexDirection FlexDirection { get; set; }
    public JustifyContent JustifyContent { get; set; }
    public AlignItems AlignItems { get; set; }
    public AlignSelf AlignSelf { get; set; }
    public float Gap { get; set; }
    public Edges Padding { get; set; }
    public Edges Margin { get; set; }
    
    // Children
    public IList<IRenderable> Children { get; }
    
    // Rendering
    public abstract void Render(FrameBuffer buffer, int x, int y, int width, int height);
}
```

---

### TextRenderable

Renders styled text.

```csharp
namespace OpenTUI.Renderables;

public class TextRenderable : Renderable
{
    public TextRenderable();
    public TextRenderable(string text);
    
    public string Text { get; set; }
    public RGBA Color { get; set; }
    public RGBA? BackgroundColor { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public bool Strikethrough { get; set; }
    public TextAlign TextAlign { get; set; }
    public TextWrap Wrap { get; set; }
}

public enum TextAlign { Left, Center, Right }
public enum TextWrap { None, Word, Character }
```

---

### BoxRenderable

A container with optional border and background.

```csharp
namespace OpenTUI.Renderables;

public class BoxRenderable : Renderable
{
    public BorderStyle BorderStyle { get; set; }
    public RGBA BorderColor { get; set; }
    public RGBA BackgroundColor { get; set; }
    public string? Title { get; set; }
    public TitleAlign TitleAlign { get; set; }
}

public enum BorderStyle { None, Single, Double, Rounded, Heavy, Dashed, Custom }
public enum TitleAlign { Left, Center, Right }
```

---

### InputRenderable

A text input field.

```csharp
namespace OpenTUI.Renderables;

public class InputRenderable : Renderable
{
    public string Value { get; set; }
    public string Placeholder { get; set; }
    public bool Password { get; set; }
    public int? MaxLength { get; set; }
    public bool Focused { get; set; }
    public int CursorPosition { get; set; }
    public RGBA TextColor { get; set; }
    public RGBA PlaceholderColor { get; set; }
    public RGBA CursorColor { get; set; }
    
    public event EventHandler<string>? ValueChanged;
    public event EventHandler? Submit;
}
```

---

### SelectRenderable

A dropdown/select control.

```csharp
namespace OpenTUI.Renderables;

public class SelectRenderable : Renderable
{
    public IList<string> Options { get; set; }
    public int SelectedIndex { get; set; }
    public string? SelectedValue { get; }
    public bool Open { get; set; }
    public bool Focused { get; set; }
    public RGBA HighlightColor { get; set; }
    public RGBA TextColor { get; set; }
    
    public event EventHandler<int>? SelectionChanged;
}
```

---

### SliderRenderable

A horizontal slider/progress bar.

```csharp
namespace OpenTUI.Renderables;

public class SliderRenderable : Renderable
{
    public float Value { get; set; }
    public float Min { get; set; }
    public float Max { get; set; }
    public float Step { get; set; }
    public bool Focused { get; set; }
    public RGBA FilledColor { get; set; }
    public RGBA EmptyColor { get; set; }
    public RGBA HandleColor { get; set; }
    public bool ShowValue { get; set; }
    
    public event EventHandler<float>? ValueChanged;
}
```

---

### ScrollBoxRenderable

A scrollable container.

```csharp
namespace OpenTUI.Renderables;

public class ScrollBoxRenderable : Renderable
{
    public int ScrollOffset { get; set; }
    public int MaxScroll { get; }
    public bool ShowScrollbar { get; set; }
    public RGBA ScrollbarColor { get; set; }
    public RGBA ScrollbarTrackColor { get; set; }
    
    public void ScrollTo(int offset);
    public void ScrollBy(int delta);
}
```

---

## Rendering

### CliRenderer

Renders renderable trees to the terminal.

```csharp
namespace OpenTUI.Rendering;

public class CliRenderer
{
    public CliRenderer();
    public CliRenderer(TextWriter output);
    
    public void Render(IRenderable renderable);
    public void Render(IRenderable renderable, int width, int height);
    
    public FrameBuffer RenderToBuffer(IRenderable renderable, FlexNode layoutNode);
}
```

---

## Input

### Key

Enumeration of keyboard keys.

```csharp
namespace OpenTUI.Input;

public enum Key
{
    None,
    
    // Letters
    A, B, C, D, E, F, G, H, I, J, K, L, M,
    N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
    
    // Numbers
    D0, D1, D2, D3, D4, D5, D6, D7, D8, D9,
    
    // Function keys
    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
    
    // Navigation
    Up, Down, Left, Right,
    Home, End, PageUp, PageDown,
    
    // Editing
    Enter, Escape, Backspace, Delete, Tab,
    Insert, Space,
    
    // Punctuation
    Comma, Period, Semicolon, Quote, Slash, Backslash,
    LeftBracket, RightBracket, Minus, Equals, Backtick,
    
    // Other
    PrintScreen, ScrollLock, Pause,
    NumLock, CapsLock
}
```

---

### KeyModifiers

Modifier key flags.

```csharp
namespace OpenTUI.Input;

[Flags]
public enum KeyModifiers
{
    None = 0,
    Shift = 1,
    Alt = 2,
    Control = 4,
    Meta = 8  // Windows/Command key
}
```

---

### KeyEvent

Represents a keyboard event.

```csharp
namespace OpenTUI.Input;

public readonly struct KeyEvent
{
    public Key Key { get; }
    public KeyModifiers Modifiers { get; }
    public char? Character { get; }
    
    public bool HasShift { get; }
    public bool HasAlt { get; }
    public bool HasControl { get; }
    public bool HasMeta { get; }
}
```

---

### InputHandler

Handles keyboard input from the terminal.

```csharp
namespace OpenTUI.Input;

public class InputHandler : IDisposable
{
    public InputHandler();
    
    public event EventHandler<KeyEvent>? OnKey;
    
    public Task StartAsync(CancellationToken cancellationToken = default);
    public void Stop();
    
    public void Dispose();
}
```

---

## Reactive

### State\<T\>

A reactive state container that triggers re-renders on change.

```csharp
namespace OpenTUI.Reactive.Primitives;

public class State<T>
{
    public State(T initialValue);
    
    public T Value { get; set; }
    
    public void Set(T value);
    public void Update(Func<T, T> updater);
    
    public event EventHandler<StateChangedEventArgs<T>>? Changed;
    
    public IDisposable Subscribe(Action<T> callback);
    
    public static implicit operator T(State<T> state);
}

public class StateChangedEventArgs<T> : EventArgs
{
    public T OldValue { get; }
    public T NewValue { get; }
}
```

---

### Computed\<T\>

A computed/derived value that automatically updates.

```csharp
namespace OpenTUI.Reactive.Primitives;

public class Computed<T>
{
    public Computed(Func<T> computation, params State[] dependencies);
    
    public T Value { get; }
    
    public event EventHandler<StateChangedEventArgs<T>>? Changed;
    
    public void Dispose();
}
```

---

### Effect

Runs side effects when dependencies change.

```csharp
namespace OpenTUI.Reactive.Primitives;

public class Effect : IDisposable
{
    public Effect(Func<Action?> effect, params State[] dependencies);
    
    public void Run();
    public void Dispose();
}
```

---

### Component

Base class for reactive components.

```csharp
namespace OpenTUI.Reactive.Components;

public abstract class Component
{
    public bool IsMounted { get; }
    
    public void Mount();
    public void Unmount();
    
    public IRenderable GetRenderedContent();
    
    protected abstract IRenderable Render();
    
    protected virtual void OnMount();
    protected virtual void OnUnmount();
    protected virtual bool ShouldRender();
    
    protected State<T> UseState<T>(T initialValue);
    protected void UseEffect(Func<Action?> effect, params State[] dependencies);
}
```

---

## Version History

| Version | Changes |
|---------|---------|
| 1.0.0   | Initial release with full flexbox layout, renderables, input handling, and reactive system |
