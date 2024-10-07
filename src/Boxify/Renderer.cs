using PodNet.Boxify.Colorization;
using System.Drawing;
using System.Text;

namespace PodNet.Boxify;

/// <summary>A renderer used to render box art. You can inherit from this class to fine-tune the rendering process. This 
/// class is stateless and thread-safe (state is passed around using <see cref="RenderContext"/>), so you can only access
/// an instance through the singleton accessor <see cref="Default"/>.</summary>
public class Renderer
{
    /// <summary>The default renderer instance.</summary>
    public static Renderer Default { get; } = new();

    /// <summary>The constructor is </summary>
    protected Renderer() { }

    /// <summary>
    /// Renders the provided <paramref name="boxBitmapSource"/> to the <paramref name="canvas"/> using <paramref name="pixelPalette"/>.
    /// </summary>
    /// <param name="boxBitmapSource">The source that provides the pixels to render. Take care that the size of the bitmap
    /// determines the size of the rendered output's dimensions.</param>
    /// <param name="canvas">The canvas to use for compiling the box drawing. Supplied externally to allow reuse
    /// and lower memory footprint. If using in a loop, consider reusing and clearing an instance instead of allocating
    /// one for each pass. Will be used as-is and the drawing will be appended to the end of it. Won't be cleared before
    /// rendering.</param>
    /// <param name="pixelPalette">The palette to use when looking up a range of pixels.</param>
    /// <param name="boxFrame">An optional embellishment frame to surround the box art with.</param>
    /// <param name="colorizerFactory">An optional colorizer factory, the produced <see cref="IColorizer"/> handles
    /// per-pixel colorization.</param>
    /// <param name="renderOptions">Options to further refine the rendering process.</param>
    /// <remarks>
    /// The bitmap will be chunked to boxes as per per the dimensions defined in <paramref name="pixelPalette"/>,
    /// and each box will be rendered as a final pixel (with appropriate sub-pixels for each rendered pixel) with appropriate
    /// shading (if sub-pixels or shades are provided by the palette).<br/>
    /// When re-implementing this method, you should:
    /// <list type="bullet">
    /// <item>create the <see cref="RenderContext"/> and supply the contextual parameters to it,</item>
    /// <item>create the colorizer by calling <paramref name="colorizerFactory"/>, and give reference to and 
    /// from <see cref="RenderContext"/>,</item>
    /// <item>invoke <see cref="RenderInternal(RenderContext)"/>.</item>
    /// </list>
    /// </remarks>
    public virtual void Render(
        IBoxBitmapSource boxBitmapSource,
        Canvas canvas,
        PixelPalette pixelPalette,
        BoxFrame? boxFrame = null,
        Func<RenderContext, IColorizer>? colorizerFactory = null,
        RenderOptions? renderOptions = null)
    {
        var context = new RenderContext(this, canvas, boxBitmapSource, pixelPalette, boxFrame, renderOptions);
        context.Colorizer = colorizerFactory?.Invoke(context);
        RenderInternal(context);
    }

    /// <summary>Convenience method for rendering the provided <paramref name="boxBitmapSource"/> to a string using the provided
    /// <paramref name="pixelPalette"/>. This method instantiates a <see cref="Canvas"/> and a <see cref="StringBuilder"/> manually. Consider
    /// using <see cref="Render(IBoxBitmapSource, Canvas, PixelPalette, BoxFrame?, Func{RenderContext, IColorizer}?, RenderOptions?)"/> if
    /// performance is crucial or you need to supply your own <see cref="Canvas"/> instance.</summary>
    /// <returns>The string result built by appending the pixel box characters to a <see cref="StringBuilder"/>.</returns>
    /// <inheritdoc cref="Render(IBoxBitmapSource, Canvas, PixelPalette, BoxFrame?, Func{RenderContext, IColorizer}?, RenderOptions?)" />
    public virtual string Render(
        IBoxBitmapSource boxBitmapSource,
        PixelPalette pixelPalette,
        BoxFrame? boxFrame = null,
        Func<RenderContext, IColorizer>? colorizerFactory = null,
        RenderOptions? renderOptions = null)
    {
        var sb = new StringBuilder();
        var canvas = new Canvas(sb);
        Render(boxBitmapSource, canvas, pixelPalette, boxFrame, colorizerFactory, renderOptions);
        return canvas.GetResult();
    }

    /// <summary>Encapsulates the overall rendering process. The default implementation calls <see cref="RenderTopFrame(RenderContext)"/>, 
    /// <see cref="RenderBody(RenderContext)"/> and <see cref="RenderBottomFrame(RenderContext)"/>.</summary>
    /// <param name="context">The rendering context.</param>
    protected virtual void RenderInternal(RenderContext context)
    {
        RenderTopFrame(context);
        RenderBody(context);
        RenderBottomFrame(context);
    }

    /// <summary>Renders the top embellishment frame of the box art, if a <see cref="BoxFrame"/> was provided.</summary>
    /// <param name="context">The rendering context.</param>
    protected virtual void RenderTopFrame(RenderContext context)
        => context.BoxFrame?.RenderTop(context.Canvas, context.FrameWidth);

    /// <summary>Iterates through the <see cref="IBoxBitmapSource"/>'s rows by <see cref="PixelPalette.PixelHeight"/> units,
    /// and calls <see cref="RenderRow(RenderContext)"/> for each row.</summary>
    /// <param name="context">The rendering context.</param>
    protected virtual void RenderBody(RenderContext context)
    {
        for (context.CurrentY = 0; context.CurrentY < context.BoxBitmapSource.Height; context.CurrentY += context.PixelPalette.PixelHeight)
            RenderRow(context);
    }

    /// <summary>Renders a row of pixels by rendering the left and right of the <see cref="BoxFrame"/> (if provided),
    /// iterates through the row's chunks by <see cref="PixelPalette.PixelWidth"/> units, and appending a line 
    /// to the canvas.</summary>
    /// <param name="context">The rendering context.</param>
    protected virtual void RenderRow(RenderContext context)
    {
        context.BoxFrame?.RenderLeft(context.Canvas);
        context.Colorizer?.BeforeRow();
        for (context.CurrentX = 0; context.CurrentX < context.BoxBitmapSource.Width; context.CurrentX += context.PixelPalette.PixelWidth)
            RenderBox(context);
        context.Colorizer?.AfterRow();
        context.BoxFrame?.RenderRight(context.Canvas);
        context.Canvas.AppendLine();
    }

    /// <summary>Renders one box (full pixel) in the context. This includes handling the <see cref="IColorizer"/>'s state, 
    /// calculating the index by calling <see cref="CalculateBoxIndex(RenderContext)"/>, looking up the correct box using
    /// <see cref="GetBoxCharacter(RenderContext, int)"/>, calling the colorizer's before and after hooks and appending the
    /// box art character to the <see cref="Canvas"/>.</summary>
    /// <param name="context">The rendering context.</param>
    protected virtual void RenderBox(RenderContext context)
    {
        context.Colorizer?.OpenBox();
        var index = CalculateBoxIndex(context);
        var box = GetBoxCharacter(context, index);
        context.Colorizer?.BeforePixel();
        context.Canvas.Append(box);
        context.Colorizer?.AfterPixel();
        context.Colorizer?.CloseBox();
    }

    /// <summary>Calculates the index of the box to look up in <see cref="GetBoxCharacter(RenderContext, int)"/> by iterating
    /// through all sub-pixels (if any) in the <see cref="IBoxBitmapSource"/> in the correct order and determining which ones are 
    /// set, as well as their brightnesses as decided by <see cref="RenderContext.PixelAnalyzer"/>.
    /// Also calls the colorizer's <see cref="IColorizer.ProcessSubPixel(int, int, Color, bool, float)"/> hook.</summary>
    /// <param name="context">The rendering context.</param>
    /// <returns>The index of the correct box that represents the current chunk with its sub-pixels set in the <see cref="PixelPalette"/>.</returns>
    protected virtual int CalculateBoxIndex(RenderContext context)
    {
        var index = 0;
        var totalShade = 0f;
        for (var dy = 0; dy < context.PixelPalette.PixelHeight && context.CurrentY + dy < context.BoxBitmapSource.Height; dy++)
        {
            for (var dx = 0; dx < context.PixelPalette.PixelWidth && context.CurrentX + dx < context.BoxBitmapSource.Width; dx++)
            {
                var pixel = context.BoxBitmapSource.GetPixel(context.CurrentX + dx, context.CurrentY + dy);
                var isSet = context.PixelAnalyzer.IsSet(pixel);
                var pixelBrightness = Math.Min(1f, Math.Max(0f, context.PixelAnalyzer.GetBrightness(pixel)));
                if (context.Colorizer?.ProcessSubPixel(dx, dy, pixel, isSet, pixelBrightness) is { } colorizerIsSet)
                    isSet = colorizerIsSet;
                if (isSet)
                    index |= 1 << (context.PixelPalette.PixelWidth * dy) + dx;
                totalShade += pixelBrightness;
            }
        }
        totalShade /= context.PixelPalette.PixelWidth * context.PixelPalette.PixelHeight;
        return (index, context.PixelPalette.ShadesPerPixel) switch
        {
            (_, 1) => index,
            (0, _) => 0,
            ( >= 1, > 1) => ((index - 1) * context.PixelPalette.ShadesPerPixel) + ((int)(totalShade * context.PixelPalette.ShadesPerPixel)),
            _ => throw new InvalidOperationException($"{nameof(index)} ({index}) and {nameof(context.PixelPalette.ShadesPerPixel)} ({context.PixelPalette.ShadesPerPixel}) were out of range for handled cases")
        };
    }

    /// <summary>Gets the character at <paramref name="index"/> of the <see cref="RenderContext.PixelPalette"/>.
    /// Empty and Full characters can be special cased through the <see cref="RenderOptions"/>.</summary>
    /// <param name="context">The rendering context.</param>
    /// <param name="index">The index of the box art to find. The index is determined in 
    /// the <see cref="RenderContext.PixelPalette"/> from top-left to bottom-right as if they were binary numbers,
    /// eg.: {0b00000:empty},▘,▝,▀,▖,▌,▞,▛,▗,▚,▐,▜,▄,▙,▟,{0b11111:full}.</param>
    /// <returns>The character (normally a UTF-32 code point as a UTF-16 string) as determined by <paramref name="index"/> in 
    /// the <see cref="RenderContext.PixelPalette"/>.</returns>
    protected virtual string GetBoxCharacter(RenderContext context, int index)
        => index == 0 ? context.EmptyChar : index == context.PixelPalette.Length - 1 ? context.FullChar : context.PixelPalette[index];

    /// <summary>Renders the bottom embellishment frame of the box art, if a <see cref="BoxFrame"/> was provided.</summary>
    /// <param name="context">The rendering context.</param>
    protected virtual void RenderBottomFrame(RenderContext context)
        => context.BoxFrame?.RenderBottom(context.Canvas, context.FrameWidth);
}
