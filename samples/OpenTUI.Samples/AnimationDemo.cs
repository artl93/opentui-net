using OpenTUI.Core.Animation;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;
using OpenTUI.Core.Terminal;

namespace OpenTUI.Samples;

/// <summary>
/// Animation demo showcasing the reusable animation primitives.
/// </summary>
public static class AnimationDemo
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
            
            // Create reusable animation components
            var dotsSpinner = Spinner.Dots("Loading...");
            var lineSpinner = Spinner.Line("Processing");
            var arrowSpinner = Spinner.Arrow("Syncing");
            var bouncingSpinner = Spinner.BouncingBall();
            
            var downloadBar = new ProgressBar(30) { Label = "Download", FilledColor = RGBA.Green };
            var uploadBar = new ProgressBar(30) { Label = "Upload", FilledColor = RGBA.FromHex("#00aaff") };
            var installBar = ProgressBar.Indeterminate(30);
            installBar.FilledColor = RGBA.FromHex("#ff44aa");
            
            var startTime = DateTime.Now;
            var running = true;
            
            Console.CancelKeyPress += (_, e) => { running = false; e.Cancel = true; };
            
            while (running)
            {
                var buffer = new FrameBuffer(width, height);
                var elapsed = (DateTime.Now - startTime).TotalSeconds;
                
                // Background with gradient
                for (int y = 0; y < height; y++)
                {
                    var bgIntensity = 0.02f + (y / (float)height) * 0.03f;
                    buffer.FillRect(0, y, width, 1, RGBA.FromValues(bgIntensity, bgIntensity, bgIntensity + 0.02f));
                }
                
                // Title with shimmer effect
                var titleChars = TextEffects.Shimmer("✨ OpenTUI.NET Animation Demo ✨", elapsed);
                var titleX = (width - titleChars.Length) / 2;
                for (int i = 0; i < titleChars.Length && titleX + i < width; i++)
                {
                    buffer.SetCell(titleX + i, 1, new Cell(titleChars[i].ch.ToString(), titleChars[i].color));
                }
                
                // === Spinners Section ===
                DrawBox(buffer, 2, 3, 30, 10, "Spinners", RGBA.FromHex("#00aaff"));
                
                dotsSpinner.Update();
                buffer.DrawText(dotsSpinner.ToString(), 4, 5, RGBA.FromHex("#00ff88"));
                
                lineSpinner.Update();
                buffer.DrawText(lineSpinner.ToString(), 4, 7, RGBA.Yellow);
                
                arrowSpinner.Update();
                buffer.DrawText(arrowSpinner.ToString(), 4, 9, RGBA.Cyan);
                
                bouncingSpinner.Update();
                buffer.DrawText(bouncingSpinner.CurrentFrame, 4, 11, RGBA.Magenta);
                
                // === Progress Bars Section ===
                var progressBoxX = 34;
                var progressBoxWidth = Math.Min(width - progressBoxX - 2, 50);
                var progressBoxMaxX = progressBoxX + progressBoxWidth - 2; // Leave room for right border
                var progressBarWidth = progressBoxWidth - 20; // Account for label, brackets, percentage, padding
                var indeterminateBarWidth = progressBoxWidth - 6; // No label or percentage, just brackets + padding
                DrawBox(buffer, progressBoxX, 3, progressBoxWidth, 10, "Progress Bars", RGBA.FromHex("#ff6644"));
                
                // Update bar widths to fit
                downloadBar.Width = progressBarWidth;
                uploadBar.Width = progressBarWidth;
                installBar.Width = indeterminateBarWidth;
                
                // Determinate progress bars
                downloadBar.Progress = (Math.Sin(elapsed * 0.5) + 1) / 2;
                DrawProgressBar(buffer, progressBoxX + 2, 5, downloadBar, progressBoxMaxX);
                
                uploadBar.Progress = (elapsed % 5) / 5;
                DrawProgressBar(buffer, progressBoxX + 2, 7, uploadBar, progressBoxMaxX);
                
                // Indeterminate progress bar
                installBar.Update();
                DrawProgressBar(buffer, progressBoxX + 2, 9, installBar, progressBoxMaxX);
                buffer.DrawText("Installing...", progressBoxX + 2, 10, RGBA.FromHex("#888888"));
                
                // === Text Effects Section ===
                DrawBox(buffer, 2, 14, width - 4, 10, "Text Effects", RGBA.FromHex("#aa44ff"));
                
                // Rainbow text
                var rainbowChars = TextEffects.Rainbow("Rainbow gradient text - cycling through hues", elapsed);
                for (int i = 0; i < rainbowChars.Length && 4 + i < width - 4; i++)
                {
                    buffer.SetCell(4 + i, 16, new Cell(rainbowChars[i].ch.ToString(), rainbowChars[i].color));
                }
                
                // Shimmer text
                var shimmerChars = TextEffects.Shimmer("Shimmering glimmer effect", elapsed, RGBA.FromHex("#4488ff"));
                for (int i = 0; i < shimmerChars.Length && 4 + i < width - 4; i++)
                {
                    buffer.SetCell(4 + i, 18, new Cell(shimmerChars[i].ch.ToString(), shimmerChars[i].color));
                }
                
                // Pulsing text
                var pulseColor = TextEffects.Pulse(elapsed, RGBA.FromHex("#ff8844"));
                buffer.DrawText("Pulsing brightness effect", 4, 20, pulseColor);
                
                // Typewriter effect
                var typewriterText = TextEffects.Typewriter("Typewriter effect reveals text gradually...", elapsed, 8);
                buffer.DrawText(typewriterText, 4, 22, RGBA.FromHex("#00ff00"));
                if (TextEffects.CursorVisible(elapsed) && typewriterText.Length < 44)
                {
                    buffer.DrawText("▌", 4 + typewriterText.Length, 22, RGBA.FromHex("#00ff00"));
                }
                
                // Glowing text on right side
                var glowColor = TextEffects.Glow(elapsed, RGBA.FromHex("#ffaa00"));
                var glowText = "★ Glowing Star ★";
                buffer.DrawText(glowText, width - glowText.Length - 6, 18, glowColor);
                
                // Wave text
                var waveChars = TextEffects.Wave("Wave intensity", elapsed, RGBA.FromHex("#44aaff"));
                for (int i = 0; i < waveChars.Length && width - 20 + i < width - 4; i++)
                {
                    buffer.SetCell(width - 20 + i, 20, new Cell(waveChars[i].ch.ToString(), waveChars[i].color));
                }
                
                // === Easing Visualization ===
                var easingBoxWidth = 28;
                DrawBox(buffer, 2, height - 8, easingBoxWidth, 6, "Easing Demo", RGBA.FromHex("#88ff88"));
                var t = (float)((elapsed % 2) / 2); // 0-1 over 2 seconds
                var maxDotPos = easingBoxWidth - 14; // "Linear:  " is 9 chars, leave room for ● and border
                var linearPos = (int)(Easing.Linear(t) * maxDotPos);
                var bouncePos = (int)(Easing.OutBounce(t) * maxDotPos);
                var elasticPos = (int)(Math.Clamp(Easing.OutElastic(t), 0, 1) * maxDotPos);
                
                buffer.DrawText("Linear:  " + new string(' ', linearPos) + "●", 4, height - 6, RGBA.White);
                buffer.DrawText("Bounce:  " + new string(' ', bouncePos) + "●", 4, height - 5, RGBA.Yellow);
                buffer.DrawText("Elastic: " + new string(' ', elasticPos) + "●", 4, height - 4, RGBA.Cyan);
                
                // Status bar
                buffer.FillRect(0, height - 2, width, 1, RGBA.FromValues(0.15f, 0.15f, 0.2f));
                buffer.DrawText($"Elapsed: {elapsed:F1}s  |  Press any key to exit", 2, height - 2, RGBA.FromHex("#888888"));
                buffer.DrawText("Using OpenTUI.Core.Animation", width - 32, height - 2, RGBA.FromHex("#00ff88"));
                
                // Render
                Console.Write("\x1b[H");
                Console.Write(buffer.ToAnsiString());
                Console.Out.Flush();
                
                Thread.Sleep(33); // ~30 FPS
                
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

    private static void DrawProgressBar(FrameBuffer buffer, int x, int y, ProgressBar bar, int maxX)
    {
        var (barPart, filledCount) = bar.GetBarParts();
        
        // Draw label
        if (!string.IsNullOrEmpty(bar.Label))
        {
            buffer.DrawText($"{bar.Label}:", x, y, RGBA.FromHex("#aaaaaa"));
            x += bar.Label.Length + 2;
        }
        
        // Draw bar with colors, respecting maxX boundary
        if (x < maxX) buffer.DrawText("[", x, y, RGBA.White);
        for (int i = 0; i < barPart.Length && x + 1 + i < maxX; i++)
        {
            var color = i < filledCount ? bar.FilledColor : bar.EmptyColor;
            buffer.SetCell(x + 1 + i, y, new Cell(barPart[i].ToString(), color));
        }
        var closeBracketX = x + barPart.Length + 1;
        if (closeBracketX < maxX) buffer.DrawText("]", closeBracketX, y, RGBA.White);
        
        // Draw percentage
        if (bar.ShowPercentage && !bar.IsIndeterminate)
        {
            var pctX = x + barPart.Length + 2;
            if (pctX < maxX - 4)
                buffer.DrawText($" {bar.Progress * 100:F0}%", pctX, y, RGBA.FromHex("#888888"));
        }
    }
}
