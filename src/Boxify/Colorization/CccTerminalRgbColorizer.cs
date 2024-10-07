using System.Drawing;

namespace PodNet.Boxify.Colorization;

/// <summary>An <see cref="IColorizer"/> that uses a slightly modified version of 
/// <a href="https://en.wikipedia.org/wiki/Color_Cell_Compression"> Color Cell Compression</a> to determine the background 
/// and foreground colors used in terminals for every character in a box-drawing. Only to be used when the output is a 
/// terminal (console) that can handle
/// <a href="https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences#text-formatting">
/// Terminal Escape Sequences</a>.
/// </summary>
/// <remarks>Note that this class is <b>stateful and not thread-safe</b>, and is intended to be used and reused sequentially
/// during the rendering of a single image.</remarks>
/// <param name="context">The context to use when processing the color information.</param>
/// <param name="limitedColorPalette">The limited color palette information to use when matching the colors in the source image.
/// If this parameter is null, the extended full RGB color palette will be used, but support on earlier operating systems is not
/// available on some terminal applications (for example, cmd.exe on earlier Windows 10 versions). Note that providing a limited
/// color palette does <b>not</b> change the palette of rendered colors, but finds the closest match in the palette according to
/// this parameter. If this palette is not the one that will be used in the output terminal, color accuracy will be low.</param>
public sealed class CccTerminalRgbColorizer(RenderContext context, LegacyTerminalColorPalette? limitedColorPalette = null) : IColorizer
{
    private readonly RenderContext _context = context;
    private readonly LegacyTerminalColorPalette? _palette = limitedColorPalette;
    private readonly float[] _luminanceMap = new float[context.PixelPalette.PixelWidth * context.PixelPalette.PixelHeight];

    private float _averageLuminance;
    private readonly ColorTotals _aboveAverageColorTotals = new();
    private readonly ColorTotals _belowAverageColorTotals = new();

    private string? _previousForegroundCode;
    private string? _previousBackgroundCode;

    private class ColorTotals
    {
        public int R { get; private set; }
        public int G { get; private set; }
        public int B { get; private set; }
        public int Count { get; private set; }
        public void Add(Color color)
        {
            R += color.R;
            G += color.G;
            B += color.B;
            Count++;
        }
        public Color Average => Count == 0 ? Color.Black : Color.FromArgb(R / Count, G / Count, B / Count);
        public void Clear() => (R, G, B, Count) = (0, 0, 0, 0);
    }

    /// <summary>Calculates a luminance map and an average luminance for the current block.</summary>
    public void OpenBox()
    {
        var totalLuminance = 0f;
        for (var dy = 0; dy < _context.PixelPalette.PixelHeight && _context.CurrentY + dy < _context.BoxBitmapSource.Height; dy++)
        {
            for (var dx = 0; dx < _context.PixelPalette.PixelWidth && _context.CurrentX + dx < _context.BoxBitmapSource.Width; dx++)
            {
                var pixel = _context.BoxBitmapSource.GetPixel(_context.CurrentX + dx, _context.CurrentY + dy);
                var luminance = 0.3f * pixel.R + 0.59f * pixel.G + 0.11f * pixel.B; // NTSC luminance value
                totalLuminance += luminance;
                _luminanceMap[dy * _context.PixelPalette.PixelWidth + dx] = luminance;
            }
        }
        _averageLuminance = totalLuminance / (_context.PixelPalette.PixelWidth * _context.PixelPalette.PixelHeight);
    }
    
    /// <inheritdoc/>
    /// <summary>Calculates the running average of the foreground and background colors from the sub-pixel.</summary>
    /// <returns>True if the subpixel was above the luminance treshold, false otherwise. Never returns null.</returns>
    public bool? ProcessSubPixel(int dx, int dy, Color pixel, bool isSet, float pixelBrightness)
    {
        var luminance = _luminanceMap[dy * _context.PixelPalette.PixelWidth + dx];
        var aboveLuminanceTreshold = luminance > _averageLuminance;
        (aboveLuminanceTreshold ? _aboveAverageColorTotals : _belowAverageColorTotals)
            .Add(pixel);
        return aboveLuminanceTreshold;
    }

    /// <summary>Calculates the average color of the foreground and background pixels and applies that to the canvas
    /// using text formatting terminal escape sequences <c>\e[{Foreground/Background Color code}m</c>.</summary>
    /// <remarks>Note that full extended RGB colors (<c>\e[38;2;R;G;Bm</c> and <c>\e[48;2;R;G;Bm</c>) will be used
    /// if <b>no limited color palette was provided to this instance</b>.</remarks>
    public void BeforePixel()
    {
        var foreground = _aboveAverageColorTotals.Average;
        var background = _belowAverageColorTotals.Average;
        var (foregroundCode, backgroundCode) = _palette switch
        {
            null or [] => ($"\e[38;2;{foreground.R};{foreground.G};{foreground.B}m", $"\e[48;2;{background.R};{background.G};{background.B}m"),
            _ => ($"\e[{FindClosestMatch(foreground, _palette.Value).ForegroundCode}m", $"\e[{FindClosestMatch(background, _palette.Value).BackgroundCode}m")
        };

        if (foregroundCode != _previousForegroundCode)
            _context.Canvas.Append(_previousForegroundCode = foregroundCode);
        if (backgroundCode != _previousBackgroundCode)
            _context.Canvas.Append(_previousBackgroundCode = backgroundCode);

        static LegacyTerminalColor FindClosestMatch(Color target, LegacyTerminalColorPalette palette)
        {
            var result = palette[0];
            var distance = GetDistance(target, result.Color);
            for (var i = 1; i < palette.Count; i++)
            {
                var current = palette[i];
                var currentDistance = GetDistance(target, current.Color);
                if (currentDistance < distance)
                    (result, distance) = (current, currentDistance);
            }
            return result;

            static int GetDistance(Color c1, Color c2)
            {
                var rDiff = c1.R - c2.R;
                var gDiff = c1.G - c2.G;
                var bDiff = c1.B - c2.B;
                return rDiff * rDiff + gDiff * gDiff + bDiff * bDiff;
            }
        }

    }

    /// <summary>Noop.</summary>
    public void AfterPixel() { }

    /// <summary>Clears the running average color variables so that this instance can be reused for calculating
    /// colors of another box.</summary>
    public void CloseBox()
    {
        _aboveAverageColorTotals.Clear();
        _belowAverageColorTotals.Clear();
    }

    /// <summary>Appends <c>"\e[39m\e[49m"</c> to the canvas to restore the default background and foreground
    /// colors and resets the previous token indicators.</summary>
    public void BeforeRow()
    {
        _context.Canvas.Append("\e[39m\e[49m");
        (_previousForegroundCode, _previousBackgroundCode) = (null, null);
    }

    /// <summary>Appends <c>"\e[39m\e[49m"</c> to the canvas to restore the default background and foreground
    /// colors.</summary>
    public void AfterRow()
    {
        _context.Canvas.Append("\e[39m\e[49m");
    }
}