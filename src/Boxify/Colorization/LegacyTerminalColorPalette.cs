using System.Collections;
using System.Drawing;
using static System.Drawing.Color;

namespace PodNet.Boxify.Colorization;

/// <summary>A color palette that represent the limited 16-color range of virtual terminals.
/// See <see cref="LegacyCmd"/> and <see cref="Campbell"/> for reference implementations.</summary>
public readonly record struct LegacyTerminalColorPalette(Color Black, Color Red, Color Green, Color Yellow,
    Color Blue, Color Magenta, Color Cyan, Color White,
    Color BrightBlack, Color BrightRed, Color BrightGreen, Color BrightYellow,
    Color BrightBlue, Color BrightMagenta, Color BrightCyan, Color BrightWhite) : IReadOnlyList<LegacyTerminalColor>
{
    /// <inheritdoc/>
    public readonly int Count => 16;

    /// <inheritdoc/>
    public readonly LegacyTerminalColor this[int index] => index switch
    {
        0 => new(Black, 30),
        1 => new(Red, 31),
        2 => new(Green, 32),
        3 => new(Yellow, 33),
        4 => new(Blue, 34),
        5 => new(Magenta, 35),
        6 => new(Cyan, 36),
        7 => new(White, 37),
        8 => new(BrightBlack, 90),
        9 => new(BrightRed, 91),
        10 => new(BrightGreen, 92),
        11 => new(BrightYellow, 93),
        12 => new(BrightBlue, 94),
        13 => new(BrightMagenta, 95),
        14 => new(BrightCyan, 96),
        15 => new(BrightWhite, 97),
        _ => throw new IndexOutOfRangeException($"Index {index} was out of range of the supported values: [0, {Count-1}]")
    };

    /// <inheritdoc/>
    public readonly IEnumerator<LegacyTerminalColor> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>The default legacy palette used in Windows (pre-Windows Terminal's <see cref="Campbell"/> scheme).</summary>
    public static LegacyTerminalColorPalette LegacyCmd { get; } = new(
        FromArgb(0, 0, 0), FromArgb(128, 0, 0), FromArgb(0, 128, 0), FromArgb(128, 128, 0),
        FromArgb(0, 0, 128), FromArgb(128, 0, 128), FromArgb(0, 128, 128), FromArgb(192, 192, 192),
        FromArgb(128, 128, 128), FromArgb(255, 0, 0), FromArgb(0, 255, 0), FromArgb(255, 255, 0),
        FromArgb(0, 0, 255), FromArgb(255, 0, 255), FromArgb(0, 255, 255), FromArgb(255, 255, 255));

    /// <summary>Windows Terminal's modern Campbell color palette.</summary>
    public static LegacyTerminalColorPalette Campbell { get; } = new(
        FromArgb(0, 0, 0), FromArgb(197, 15, 31), FromArgb(19, 161, 14), FromArgb(193, 156, 0),
        FromArgb(0, 55, 218), FromArgb(136, 23, 152), FromArgb(58, 150, 221), FromArgb(204, 204, 204),
        FromArgb(118, 118, 118), FromArgb(231, 72, 86), FromArgb(22, 198, 12), FromArgb(249, 241, 165),
        FromArgb(59, 120, 255), FromArgb(180, 0, 158), FromArgb(97, 214, 214), FromArgb(255, 255, 255));
}