using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;
using OpenTUI.Core.Terminal;

namespace OpenTUI.Samples;

/// <summary>
/// Animation demo showcasing progress bars, spinners, and text effects.
/// </summary>
public static class AnimationDemo
{
    private static readonly string[] Spinners = new[]
    {
        "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏"  // Dots
    };
    
    private static readonly string[] BouncingBar = new[]
    {
        "[    ●    ]",
        "[   ●     ]",
        "[  ●      ]",
        "[ ●       ]",
        "[●        ]",
        "[ ●       ]",
        "[  ●      ]",
        "[   ●     ]",
        "[    ●    ]",
        "[     ●   ]",
        "[      ●  ]",
        "[       ● ]",
        "[        ●]",
        "[       ● ]",
        "[      ●  ]",
        "[     ●   ]"
    };
    
    private static readonly string[] BlockSpinner = new[]
    {
        "▖", "▘", "▝", "▗"
    };
    
    private static readonly string[] ArrowSpinner = new[]
    {
        "←", "↖", "↑", "↗", "→", "↘", "↓", "↙"
    };

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
            
            var startTime = DateTime.Now;
            var running = true;
            
            // Check for key press without blocking
            Console.CancelKeyPress += (_, e) => { running = false; e.Cancel = true; };
            
            int frame = 0;
            while (running)
            {
                var buffer = new FrameBuffer(width, height);
                var elapsed = (DateTime.Now - startTime).TotalSeconds;
                
                // Fill background with subtle gradient effect
                for (int y = 0; y < height; y++)
                {
                    var bgIntensity = 0.02f + (y / (float)height) * 0.03f;
                    buffer.FillRect(0, y, width, 1, RGBA.FromValues(bgIntensity, bgIntensity, bgIntensity + 0.02f));
                }
                
                // Title with shimmer effect
                DrawShimmeringText(buffer, "✨ OpenTUI.NET Animation Demo ✨", (width - 34) / 2, 1, elapsed);
                
                // Spinners section
                DrawBox(buffer, 2, 3, 30, 10, "Spinners", RGBA.FromHex("#00aaff"));
                
                var spinnerIdx = frame % Spinners.Length;
                buffer.DrawText($"{Spinners[spinnerIdx]} Loading...", 4, 5, RGBA.FromHex("#00ff88"));
                
                var blockIdx = (frame / 2) % BlockSpinner.Length;
                buffer.DrawText($"{BlockSpinner[blockIdx]} Processing", 4, 7, RGBA.Yellow);
                
                var arrowIdx = frame % ArrowSpinner.Length;
                buffer.DrawText($"{ArrowSpinner[arrowIdx]} Syncing", 4, 9, RGBA.Cyan);
                
                buffer.DrawText(BouncingBar[(frame / 2) % BouncingBar.Length], 4, 11, RGBA.Magenta);
                
                // Progress bars section
                DrawBox(buffer, 34, 3, width - 36, 10, "Progress Bars", RGBA.FromHex("#ff6644"));
                
                // Determinate progress bar
                var progress1 = (Math.Sin(elapsed * 0.5) + 1) / 2; // 0-1 oscillating
                DrawProgressBar(buffer, 36, 5, width - 42, progress1, "Download", RGBA.Green);
                
                // Faster progress bar
                var progress2 = (elapsed % 5) / 5; // 0-1 over 5 seconds
                DrawProgressBar(buffer, 36, 7, width - 42, progress2, "Upload", RGBA.FromHex("#00aaff"));
                
                // Indeterminate progress bar
                DrawIndeterminateBar(buffer, 36, 9, width - 42, frame, RGBA.FromHex("#ff44aa"));
                buffer.DrawText("Installing...", 36, 10, RGBA.FromHex("#888888"));
                
                // Text effects section
                DrawBox(buffer, 2, 14, width - 4, 10, "Text Effects", RGBA.FromHex("#aa44ff"));
                
                // Rainbow text
                DrawRainbowText(buffer, "Rainbow gradient text - cycling through hues", 4, 16, elapsed);
                
                // Wave text (color wave, not position wave)
                DrawWaveText(buffer, "Wave intensity text effect", 4, 18, elapsed);
                
                // Pulsing text
                DrawPulsingText(buffer, "Pulsing brightness", 4, 20, elapsed);
                
                // Typewriter effect
                DrawTypewriterText(buffer, "Typewriter effect simulates typing...", 4, 22, elapsed);
                
                // Glowing text on right side
                DrawGlowingText(buffer, "★ Glowing Star Text ★", width - 26, 18, elapsed);
                
                // Matrix rain effect in a small area
                DrawBox(buffer, 2, height - 10, 30, 8, "Matrix Rain", RGBA.Green);
                DrawMatrixRain(buffer, 3, height - 9, 28, 6, frame);
                
                // Status section
                buffer.FillRect(0, height - 2, width, 1, RGBA.FromValues(0.15f, 0.15f, 0.2f));
                buffer.DrawText($"Frame: {frame}  |  Elapsed: {elapsed:F1}s  |  Press Ctrl+C to exit", 2, height - 2, RGBA.FromHex("#888888"));
                buffer.DrawText($"FPS: ~30", width - 12, height - 2, RGBA.FromHex("#00ff88"));
                
                // Render
                Console.Write("\x1b[H"); // Move to home
                Console.Write(buffer.ToAnsiString());
                Console.Out.Flush();
                
                frame++;
                Thread.Sleep(33); // ~30 FPS
                
                // Check for any key press
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    running = false;
                }
            }
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

    private static void DrawProgressBar(FrameBuffer buffer, int x, int y, int w, double progress, string label, RGBA color)
    {
        var filled = (int)(progress * (w - 2));
        var bar = new string('█', filled) + new string('░', w - 2 - filled);
        
        buffer.DrawText($"{label}: [{bar}] {progress * 100:F0}%", x, y, color);
    }

    private static void DrawIndeterminateBar(FrameBuffer buffer, int x, int y, int w, int frame, RGBA color)
    {
        var barWidth = w - 2;
        var highlightWidth = 6;
        var pos = (frame * 2) % (barWidth + highlightWidth) - highlightWidth;
        
        var bar = "";
        for (int i = 0; i < barWidth; i++)
        {
            if (i >= pos && i < pos + highlightWidth)
                bar += "█";
            else
                bar += "░";
        }
        
        buffer.DrawText($"[{bar}]", x, y, color);
    }

    private static void DrawShimmeringText(FrameBuffer buffer, string text, int x, int y, double time)
    {
        for (int i = 0; i < text.Length; i++)
        {
            var shimmer = (float)(Math.Sin(time * 3 + i * 0.3) + 1) / 2;
            var r = 0.5f + shimmer * 0.5f;
            var g = 0.7f + shimmer * 0.3f;
            var b = 1.0f;
            buffer.SetCell(x + i, y, new Cell(text[i].ToString(), RGBA.FromValues(r, g, b)));
        }
    }

    private static void DrawRainbowText(FrameBuffer buffer, string text, int x, int y, double time)
    {
        for (int i = 0; i < text.Length; i++)
        {
            var hue = (time * 50 + i * 10) % 360;
            var color = HsvToRgb(hue, 1.0, 1.0);
            buffer.SetCell(x + i, y, new Cell(text[i].ToString(), color));
        }
    }

    private static void DrawPulsingText(FrameBuffer buffer, string text, int x, int y, double time)
    {
        var intensity = (float)(Math.Sin(time * 4) + 1) / 2 * 0.7f + 0.3f;
        var color = RGBA.FromValues(intensity, intensity * 0.5f, intensity);
        buffer.DrawText(text, x, y, color);
    }

    private static void DrawTypewriterText(FrameBuffer buffer, string text, int x, int y, double time)
    {
        var charsToShow = (int)(time * 8) % (text.Length + 10);
        var displayText = text[..Math.Min(charsToShow, text.Length)];
        buffer.DrawText(displayText, x, y, RGBA.FromHex("#00ff00"));
        
        // Blinking cursor
        if (charsToShow < text.Length && (int)(time * 4) % 2 == 0)
        {
            buffer.DrawText("▌", x + displayText.Length, y, RGBA.FromHex("#00ff00"));
        }
    }

    private static void DrawWaveText(FrameBuffer buffer, string text, int x, int y, double time)
    {
        // Wave effect using color intensity instead of position to avoid overlap
        for (int i = 0; i < text.Length; i++)
        {
            var wave = (float)(Math.Sin(time * 3 + i * 0.5) + 1) / 2;
            var color = RGBA.FromValues(0.3f + wave * 0.7f, 0.6f + wave * 0.4f, 1.0f);
            buffer.SetCell(x + i, y, new Cell(text[i].ToString(), color));
        }
    }

    private static void DrawGlowingText(FrameBuffer buffer, string text, int x, int y, double time)
    {
        var glow = (float)(Math.Sin(time * 2) + 1) / 2;
        var color = RGBA.FromValues(1.0f, 0.8f + glow * 0.2f, 0.2f + glow * 0.3f);
        buffer.DrawText(text, x, y, color);
    }

    private static void DrawMatrixRain(FrameBuffer buffer, int x, int y, int w, int h, int frame)
    {
        // Use ASCII chars to avoid double-width character issues
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@#$%&*";
        var random = new Random(42); // Fixed seed for consistent columns
        
        for (int col = 0; col < w; col++)
        {
            // Each column has its own speed and phase
            var colSeed = random.Next(1000);
            var speed = 1 + (colSeed % 3);
            var phase = colSeed % 20;
            var headPos = ((frame * speed / 2) + phase) % (h + 8) - 4;
            
            for (int row = 0; row < h; row++)
            {
                var charRandom = new Random(frame / 3 + col * 100 + row);
                var ch = chars[charRandom.Next(chars.Length)];
                var distFromHead = headPos - row;
                
                if (distFromHead >= 0 && distFromHead < 6)
                {
                    RGBA color;
                    if (distFromHead == 0)
                        color = RGBA.FromValues(0.9f, 1.0f, 0.9f); // Bright head
                    else
                    {
                        var fade = 1.0f - (distFromHead / 6.0f);
                        color = RGBA.FromValues(0, fade * 0.8f, 0);
                    }
                    buffer.SetCell(x + col, y + row, new Cell(ch.ToString(), color));
                }
                else if (distFromHead >= 6 && distFromHead < 12)
                {
                    // Fading tail
                    var fade = 1.0f - ((distFromHead - 6) / 6.0f);
                    buffer.SetCell(x + col, y + row, new Cell(ch.ToString(), RGBA.FromValues(0, fade * 0.3f, 0)));
                }
            }
        }
    }

    private static RGBA HsvToRgb(double h, double s, double v)
    {
        var hi = (int)(h / 60) % 6;
        var f = h / 60 - (int)(h / 60);
        var p = v * (1 - s);
        var q = v * (1 - f * s);
        var t = v * (1 - (1 - f) * s);

        return hi switch
        {
            0 => RGBA.FromValues((float)v, (float)t, (float)p),
            1 => RGBA.FromValues((float)q, (float)v, (float)p),
            2 => RGBA.FromValues((float)p, (float)v, (float)t),
            3 => RGBA.FromValues((float)p, (float)q, (float)v),
            4 => RGBA.FromValues((float)t, (float)p, (float)v),
            _ => RGBA.FromValues((float)v, (float)p, (float)q)
        };
    }
}
