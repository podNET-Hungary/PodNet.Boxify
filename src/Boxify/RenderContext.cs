using PodNet.Boxify.Colorization;

namespace PodNet.Boxify;

/// <summary>A context object that defines the state and dependencies of a box-art rendering process.</summary>
/// <param name="Renderer">The renderer object that initiated the rendering.</param>
/// <param name="Canvas">The canvas to write the produced box-art to.</param>
/// <param name="BoxBitmapSource">The bitmap to process the pixels of to produce the box-art.</param>
/// <param name="PixelPalette">The <see cref="Boxify.PixelPalette"/> implementation. Also determines the lookup
/// logic for the pixels.</param>
/// <param name="BoxFrame">An optional embellishment frame to surround the resulting box-art with.</param>
/// <param name="RenderOptions">The options wrapped by this context.</param>
public record RenderContext(
    Renderer Renderer,
    Canvas Canvas,
    IBoxBitmapSource BoxBitmapSource,
    PixelPalette PixelPalette,
    IBoxFrame? BoxFrame,
    RenderOptions? RenderOptions)
{
    /// <summary name="Colorizer">An optional colorizer component that handles colorization of the pixels. Can manipulate
    /// the output itself, or produce color information through another mechanism.</summary>
    /// <remarks>This is late-bound to allow it (and the factory) to reference the currently created context.</remarks>
    public IColorizer? Colorizer { get; set; }

    /// <summary>The calculated effective empty character, either from <see name="RenderOptions"/> or <see name="PixelPalette"/>.</summary>
    public string EmptyChar { get; } = RenderOptions?.EmptyCharacter ?? PixelPalette[0];
    /// <summary>The calculated effective full character, either from <see name="RenderOptions"/> or <see name="PixelPalette"/>.</summary>
    public string FullChar { get; } = RenderOptions?.FullCharacter ?? PixelPalette[^1];

    /// <summary>The width of the frame to be produced. Only accounts for the <b>content's pixels</b>.
    /// Passed to <see name="BoxFrame"/> to set the width of the top of the frame.</summary>
    public int FrameWidth { get; } = (int)Math.Ceiling((float)BoxBitmapSource.Width / PixelPalette.PixelWidth);

    /// <summary>The iterator variable used to move through the columns of the current row of pixels in the bitmap.</summary>
    public int CurrentX { get; set; }
    /// <summary>The iterator variable used to move through the rows of pixels in the bitmap.</summary>
    public int CurrentY { get; set; }

    /// <summary>The <see cref="Boxify.PixelAnalyzer"/> used to determine if a pixel is set and its brightness. 
    /// Provided via <see cref="RenderOptions"/>, defaults to <see cref="PixelAnalyzer.AlphaXHueSaturationBrightness"/>.</summary>
    public PixelAnalyzer PixelAnalyzer { get; } = RenderOptions?.PixelAnalyzer ?? PixelAnalyzer.AlphaXHueSaturationBrightness;
}
