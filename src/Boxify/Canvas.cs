using System.Text;

namespace PodNet.Boxify;

/// <summary>A simple proxy wraps a <see cref="StringBuilder"/> used to build string-rendered box-drawings of bitmaps. 
/// You can inherit from this class if you want to define how the drawn characters are handled.</summary>

public class Canvas
{
    /// <summary>The constructor is protected to only allow instantiation with a <see cref="StringBuilder"/>,
    /// or when inherited from.</summary>
    protected Canvas() { _stringBuilder = null!; }

    /// <summary>Create a new canvas that is used when rendering box-drawings to strings.</summary>
    /// <param name="stringBuilder">The builder to operate on. The reference is kept private to discourage
    /// unintended manipulation of the builder. Use <see cref="GetResult()"/> to obtain the fully rendered string.</param>
    public Canvas(StringBuilder stringBuilder) => _stringBuilder = stringBuilder;

    /// <summary>The original builder supplied to this canvas during construction.</summary>
    private readonly StringBuilder _stringBuilder;

    /// <inheritdoc/>
    public virtual void Append(char value) => _stringBuilder.Append(value);

    /// <inheritdoc/>
    public virtual void Append(string? value) => _stringBuilder.Append(value);

    /// <inheritdoc/>
    public virtual void AppendLine() => _stringBuilder.AppendLine();

    /// <summary>Gets the result of the rendering operation by calling <see cref="StringBuilder.ToString()"/> on the
    /// underlying builder instance.</summary>
    /// <returns>The built string (as is).</returns>
    public virtual string GetResult() => _stringBuilder.ToString();
}