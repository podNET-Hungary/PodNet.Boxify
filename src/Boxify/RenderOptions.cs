namespace PodNet.Boxify;

/// <summary>Additional settings to refine the box-art rendering process.</summary>
public class RenderOptions
{
    /// <summary>Override the empty character provided by the palette.</summary>
    public string? EmptyCharacter { get; init; }
    /// <summary>Override the full character provided by the palette.</summary>
    public string? FullCharacter { get; init; }

    /// <summary>The <see cref="Boxify.PixelAnalyzer"/> used to determine if a 
    /// pixel is set and the pixel's brightness values.</summary>
    public PixelAnalyzer? PixelAnalyzer { get; init; }
}