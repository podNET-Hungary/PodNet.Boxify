using System.Drawing;

namespace PodNet.Boxify.Colorization;

/// <summary>An interface that describes the functionality of a colorizer component that can be used to
/// modify, enhance or augment the rendering process with color information. The methods act as hooks, called by the 
/// <see cref="Renderer"/> at the appropriate phase in the rendering process.</summary>
public interface IColorizer
{
    /// <summary>Invoked by the <see cref="Renderer"/> when starting to process a "pixel".
    /// The pixel is either a 1:1 pixel in the bitmap, a chunk of pixels combined to a single composite character, or
    /// anything else as defined by the renderer.</summary>
    void OpenBox();

    /// <summary>
    /// Invoked by the <see cref="Renderer"/> when processing a "sub-pixel", that is, a raw
    /// pixel in the bitmap being processed that will make up a composite pixel. Invoked at least once for every pixel.
    /// </summary>
    /// <param name="dx">The sub-pixel's x coordinate in the current pixel being processed.</param>
    /// <param name="dy">The sub-pixel's y coordinate in the current pixel being processed.</param>
    /// <param name="pixel">The pixel's color information.</param>
    /// <param name="isSet">Indicates if the pixel is considered to be set by the renderer.</param>
    /// <param name="pixelBrightness">Indicates the arbitrary "brightness" value as determined by the renderer.</param>
    /// <returns>If the renderer should consider the pixel "set" after the colorization process. Null to leave it as is.</returns>
    bool? ProcessSubPixel(int dx, int dy, Color pixel, bool isSet, float pixelBrightness);

    /// <summary>Invoked by the <see cref="Renderer"/> before writing the pixel to
    /// the target <see cref="Canvas"/>.</summary>
    void BeforePixel();

    /// <summary>Invoked by the <see cref="Renderer"/> after writing the pixel to
    /// the target <see cref="Canvas"/>.</summary>
    void AfterPixel();

    /// <summary>Invoked by the <see cref="Renderer"/> when finishing processing
    /// a "pixel".</summary>
    void CloseBox();

    /// <summary>Invoked before processing the body portion of a row.</summary>
    void BeforeRow();
    /// <summary>Invoked after processing the body portion of a row.</summary>
    void AfterRow();
}
