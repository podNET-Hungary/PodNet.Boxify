namespace PodNet.Boxify;

/// <summary>Describes an embellishment box frame that can be used to wrap box art.
/// See <see cref="BoxFrame"/> for a simple reference implementation.</summary>
public interface IBoxFrame
{
    /// <summary>Renders the top part of the box frame to the canvas.</summary>
    /// <param name="canvas">The builder to append the frame to.</param>
    /// <param name="width">The width of the frame's content. This determines how 
    /// many top characters to draw.</param>
    void RenderBottom(Canvas canvas, int width);

    /// <summary>Renders the left part of the box frame to the canvas.</summary>
    /// <param name="canvas">The builder to append the frame to.</param>
    void RenderLeft(Canvas canvas);

    /// <summary>Renders the right part of the box frame to the canvas.</summary>
    /// <param name="canvas">The builder to append the frame to.</param>
    void RenderRight(Canvas canvas);

    /// <summary>Renders the bottom part of the box frame to the canvas.</summary>
    /// <param name="canvas">The builder to append the frame to.</param>
    /// <param name="width">The width of the frame's content. This determines how 
    /// many bottom characters to draw.</param>
    void RenderTop(Canvas canvas, int width);
}

/// <summary>
/// An embellishment frame descriptor to use when rendering box art. See <see cref="Default"/> for a simple prebuilt variant.
/// Uses strings to allow defining UTF32 characters (eg. surrogate pairs, extended Unicode outside UTF-16).
/// </summary>
/// <remarks>You can inherit this record to customize the frame rendering.</remarks>
/// <param name="TopLeft">The character for the named part of the embellishment box.</param>
/// <param name="Top">The character for the named part of the embellishment box.</param>
/// <param name="TopRight">The character for the named part of the embellishment box.</param>
/// <param name="Left">The character for the named part of the embellishment box.</param>
/// <param name="Right">The character for the named part of the embellishment box.</param>
/// <param name="BottomLeft">The character for the named part of the embellishment box.</param>
/// <param name="Bottom">The character for the named part of the embellishment box.</param>
/// <param name="BottomRight">The character for the named part of the embellishment box.</param>
public record BoxFrame(string? TopLeft, string? Top, string? TopRight, string? Left, string? Right, string? BottomLeft, string? Bottom, string? BottomRight) : IBoxFrame
{
    private static BoxFrame? _default;
    /// <summary>
    /// A simple prebuilt box that uses double lines to frame the content.
    /// <code>
    /// ╔════╗
    /// ║    ║
    /// ╚════╝
    /// </code>
    /// </summary>
    public static BoxFrame Default { get; } = _default ??= new("╔", "═", "╗", "║", "║", "╚", "═", "╝");

    /// <summary>A prefix that will be inserted before each line (top, left and bottom).</summary>
    public string? LinePrefix { get; init; }

    /// <summary>A suffix that will be inserted after each line (top, right and bottom).</summary>
    public string? LineSuffix { get; init; }

    /// <inheritdoc/>
    public virtual void RenderTop(Canvas canvas, int width)
    {
        canvas.Append(LinePrefix);
        canvas.Append(TopLeft);
        for (var i = 0; i < width; i++)
            canvas.Append(Top);
        canvas.Append(TopRight);
        canvas.Append(LineSuffix);
        canvas.AppendLine();
    }

    /// <inheritdoc/>
    public virtual void RenderLeft(Canvas canvas)
    {
        canvas.Append(LinePrefix);
        canvas.Append(Left);
    }

    /// <inheritdoc/>
    public virtual void RenderRight(Canvas canvas)
    {
        canvas.Append(Right);
        canvas.Append(LineSuffix);
    }

    /// <inheritdoc/>
    public virtual void RenderBottom(Canvas canvas, int width)
    {
        canvas.Append(LinePrefix);
        canvas.Append(BottomLeft);
        for (var i = 0; i < width; i++)
            canvas.Append(Bottom);
        canvas.Append(BottomRight);
        canvas.Append(LineSuffix);
        canvas.AppendLine();
    }
}