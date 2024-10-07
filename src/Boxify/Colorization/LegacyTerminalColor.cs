using System.Drawing;

namespace PodNet.Boxify.Colorization;

/// <summary>Pairs a <see cref="System.Drawing.Color"/> with its corresponding
/// <a href="https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences#text-formatting">
/// Terminal Escape Sequence Codes</a>.</summary>
/// <param name="Color">The color to be used in a palette.</param>
/// <param name="ForegroundCode">The code when used as a foreground color as described in
/// <a href="https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences#text-formatting">
/// Terminal Escape Sequence Codes</a>. The <see cref="BackgroundCode"/> is derived by adding 10 to this value.</param>
public record struct LegacyTerminalColor(Color Color, int ForegroundCode)
{
    /// <summary>A terminal escape sequence derived from <see cref="ForegroundCode"/> by adding the constant 10 value to it.</summary>
    public readonly int BackgroundCode => ForegroundCode + 10;
}
