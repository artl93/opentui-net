using SysConsole = System.Console;

namespace OpenTUI.Core.Input;

/// <summary>
/// Reads keyboard input from the console.
/// </summary>
public class KeyReader : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    /// <summary>Event raised when a key is pressed.</summary>
    public event EventHandler<KeyEvent>? KeyPressed;

    /// <summary>Event raised when text is pasted (bracketed paste mode).</summary>
#pragma warning disable CS0067 // Not yet implemented
    public event EventHandler<string>? TextPasted;
#pragma warning restore CS0067

    /// <summary>
    /// Reads a single key synchronously.
    /// </summary>
    public KeyEvent? ReadKey(bool intercept = true)
    {
        if (!SysConsole.KeyAvailable)
            return null;

        var keyInfo = SysConsole.ReadKey(intercept);
        return AnsiKeyParser.FromConsoleKeyInfo(keyInfo);
    }

    /// <summary>
    /// Reads a key asynchronously with timeout.
    /// </summary>
    public async Task<KeyEvent?> ReadKeyAsync(int timeoutMs = -1, CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);

        try
        {
            var timeout = timeoutMs > 0 ? TimeSpan.FromMilliseconds(timeoutMs) : Timeout.InfiniteTimeSpan;

            return await Task.Run<KeyEvent?>(async () =>
            {
                var deadline = timeoutMs > 0 ? DateTime.UtcNow.Add(timeout) : DateTime.MaxValue;

                while (!linkedCts.Token.IsCancellationRequested)
                {
                    if (SysConsole.KeyAvailable)
                    {
                        var keyInfo = SysConsole.ReadKey(true);
                        return AnsiKeyParser.FromConsoleKeyInfo(keyInfo);
                    }

                    if (DateTime.UtcNow >= deadline)
                        return null;

                    await Task.Delay(10, linkedCts.Token);
                }

                return null;
            }, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    /// <summary>
    /// Starts reading keys in the background and raising events.
    /// </summary>
    public void StartReading()
    {
        Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var keyEvent = await ReadKeyAsync(100, _cts.Token);
                if (keyEvent.HasValue)
                {
                    KeyPressed?.Invoke(this, keyEvent.Value);
                }
            }
        });
    }

    /// <summary>
    /// Stops reading keys.
    /// </summary>
    public void StopReading()
    {
        _cts.Cancel();
    }

    /// <summary>
    /// Checks if a key is available without blocking.
    /// </summary>
    public bool KeyAvailable => SysConsole.KeyAvailable;

    /// <summary>
    /// Reads all available input and parses it.
    /// Handles escape sequences that may span multiple reads.
    /// </summary>
    public IEnumerable<KeyEvent> ReadAvailableKeys()
    {
        var buffer = new List<char>();

        while (SysConsole.KeyAvailable)
        {
            var keyInfo = SysConsole.ReadKey(true);
            buffer.Add(keyInfo.KeyChar);
        }

        if (buffer.Count == 0)
            yield break;

        // Parse the collected input
        var input = new string(buffer.ToArray());
        var index = 0;

        while (index < input.Length)
        {
            // Check for escape sequence
            if (input[index] == '\x1b' && index + 1 < input.Length)
            {
                // Find the end of the escape sequence
                var seqEnd = FindEscapeSequenceEnd(input, index);
                var sequence = input.AsSpan(index, seqEnd - index);
                var keyEvent = AnsiKeyParser.Parse(sequence);
                if (keyEvent.HasValue)
                    yield return keyEvent.Value;
                index = seqEnd;
            }
            else
            {
                // Single character
                var keyEvent = AnsiKeyParser.Parse(input.AsSpan(index, 1));
                if (keyEvent.HasValue)
                    yield return keyEvent.Value;
                index++;
            }
        }
    }

    private static int FindEscapeSequenceEnd(string input, int start)
    {
        if (start + 1 >= input.Length)
            return start + 1;

        var next = input[start + 1];

        // Alt+key
        if (next != '[' && next != 'O')
            return start + 2;

        // CSI or SS3 sequence
        var end = start + 2;
        while (end < input.Length)
        {
            var c = input[end];
            // CSI sequences end with a letter
            if (c >= 0x40 && c <= 0x7E)
            {
                return end + 1;
            }
            end++;
        }

        return end;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _cts.Cancel();
            _cts.Dispose();
            _disposed = true;
        }
    }
}
