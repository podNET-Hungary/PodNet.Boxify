using System.Drawing;

namespace PodNet.Boxify;

/// <summary>Provides the logic on how to determine if a pixel in a bitmap is considered to be "set", 
/// and the overall brightness of it. You can inherit from this class if you want to provide your own logic. 
/// See <see cref="Alpha"/>, <see cref="AlphaXHueSaturationBrightness"/> and 
/// <see cref="HueSaturationBrightness"/> for the out-of-the-box implementations.</summary>
public class PixelAnalyzer
{
    /// <summary>The constructor is protected to only allow access to the class through the provided default
    /// implementations or by inheriting from it.</summary>
    protected PixelAnalyzer() { }
    private PixelAnalyzer(Func<Color, float>? getBrightnessFunc) => _getBrightnessFunc = getBrightnessFunc;

    private static PixelAnalyzer? _alpha;
    /// <summary>Determines brightness by dividing <see cref="Color.A"/> with 256.</summary>
    public static PixelAnalyzer Alpha => _alpha ??= new(pixel => (float)pixel.A / 256);
    
    private static PixelAnalyzer? _hueSaturationBrightness;
    /// <inheritdoc cref="Color.GetBrightness()"/>
    public static PixelAnalyzer HueSaturationBrightness => _hueSaturationBrightness ??= new(pixel => pixel.GetBrightness());

    private static PixelAnalyzer? _alphaXHueSaturationBrightness;
    /// <summary>Multiplies the results as defined in <see cref="Alpha"/> and <see cref="HueSaturationBrightness"/>.</summary>
    public static PixelAnalyzer AlphaXHueSaturationBrightness => _alphaXHueSaturationBrightness ??= new(pixel => (float)pixel.A / 256 * pixel.GetBrightness());

    /// <summary>Determine if a pixel is considered to be "set" when building monochrome images. Checks if <see cref="GetBrightness(Color)"/> is larger than 0.5f.</summary>
    /// <param name="pixel">The pixel (color) to determine if considered to be "set".</param>
    /// <returns>True is <see cref="GetBrightness(Color)"/> for <paramref name="pixel"/>was larger than 0.5f.</returns>
    public virtual bool IsSet(Color pixel) => GetBrightness(pixel) > 0.5f;

    private readonly Func<Color, float>? _getBrightnessFunc;
    /// <summary>Determine a pixel's brightness. Used by default to determine if a pixel <see cref="IsSet(Color)"/>,
    /// can be used when building grayscale images, or can be used by colorization algorithms for clustering.</summary>
    /// <param name="pixel">The pixel's brightness value to determine.</param>
    /// <returns>The brightness value, expected to be between [0f (black), 1f (white)].</returns>
    public virtual float GetBrightness(Color pixel) => _getBrightnessFunc?.Invoke(pixel) ?? default;
}
