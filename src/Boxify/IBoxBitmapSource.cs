using System.Drawing;

namespace PodNet.Boxify;

/// <summary>A source that provides access to bitmap data. Used for rendering box art from any underlying data sources.
/// The default library contains no reference implementations to keep it lightweight. You should either supply your own 
/// or use a surrogate library that provides appropriate implementations. See <a href="https://www.nuget.org/packages/PodNet.Boxify.Svg/">PodNet.Boxify.Svg</a> or <a href="https://www.nuget.org/packages/PodNet.Boxify.Bmp/">PodNet.Boxify.Bmp</a>.</summary>
public interface IBoxBitmapSource
{
    /// <summary>The width of the bitmap.</summary>
    public int Width { get; }

    /// <summary>The height of the bitmap.</summary>
    public int Height { get; }

    /// <summary>Gets the pixel at the given coordinate in the bitmap.</summary>
    /// <param name="x">The coordinate of the pixel from the left of the bitmap.</param>
    /// <param name="y">The coordinate of the pixel from the top of the bitmap.</param>
    /// <returns>The pixel color data (RBGA)</returns>
    public Color GetPixel(int x, int y);
}

