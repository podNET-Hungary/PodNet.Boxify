namespace PodNet.Boxify;

/// <summary>
/// Reference implementation that uses sub-pixels or shades to represent a higher fidelity image.
/// See the static properties for predefined palettes. For best compatibility, use modern fonts
/// like <b>Cascadia Code</b> which supports all predefined palettes, even <see cref="Octants"/>.
/// For lower compatibility fonts (Courier, Lucida etc.) use <see cref="Boolean"/>, 
/// <see cref="Halves"/> or <see cref="Quadrants"/>. Always use fixed-width fonts.
/// </summary>
/// <remarks>You can inherit from this class to provide your own custom palette logic.</remarks>
public class PixelPalette
{
    // These constants are named as such so that they fit well with other one-character length strings in
    // a matrix/table structure, eg. "▒", which result in three characters total.

    /// <summary>Generally used for the "empty" box character: U+00A0 Non-Breaking Space. Unfortunately, 
    /// there's no box-width empty space character.</summary>
    private const string ETY = "\u00a0";
    /// <summary>Generally used for the "full" box character: U+2588 Full Block.</summary>
    private const string FUL = "█";

    /// <summary>The number of characters in this palette.</summary>
    public virtual int Length { get; }

    /// <summary>The "width" of the pixels in this palette. For a 2x4 octant palette, this would be 2.</summary>
    public virtual int PixelWidth { get; }

    /// <summary>The "height" of the pixels in this palette. For a 2x4 octant palette, this would be 4.</summary>
    public virtual int PixelHeight { get; }

    /// <summary>The number of shades stored for each pixel.</summary>
    public virtual int ShadesPerPixel { get; }

    /// <summary>
    /// The approximate aspect ratio of each character in the palette, provided the consumer expects characters 
    /// with an aspect ratio of 1:2 (that is, box characters with a height double of width). For example, a 2x4
    /// or 1x2 resolution box character can be considered to have a character-pixel A/R of 1f (square pixels). 
    /// A reference implementation might use <c>((float)<see cref="PixelWidth"/>) / ((float)<see cref="PixelHeight"/> / 2f)</c>.
    /// Use this property to render/resize the source image to the correct aspect ratio.
    /// </summary>
    public virtual float CharacterAspectRatio { get; }

    private readonly string[] _characters;

    /// <param name="pixelWidth">The "width" of the pixels in this palette. For a 2x4 octant palette, this would be 2.</param>
    /// <param name="pixelHeight">The "height" of the pixels in this palette. For a 2x4 octant palette, this would be 4.</param>
    /// <param name="shadesPerPixel">The number of shades stored for each pixel.</param>
    /// <param name="characters">The character map <b>ordered</b> from "empty" to "full", going from left-to-right, then 
    /// top-to-bottom (row by row). Contents are only validated to be conformant in size to 2^(<paramref name="pixelWidth"/> 
    /// * <paramref name="pixelHeight"/>) OR <paramref name="shadesPerPixel"/>. The elements will be copied.</param>
    public PixelPalette(int pixelWidth, int pixelHeight, int shadesPerPixel, string[] characters)
    {
        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
        ShadesPerPixel = shadesPerPixel;
        Length = characters.Length;
        CharacterAspectRatio = pixelWidth / (pixelHeight / 2f);

        var correctLength = (pixelWidth, pixelHeight, shadesPerPixel) switch
        {
            ( >= 1, >= 1, > 1) => Math.Pow(2, pixelWidth * pixelHeight - 1) * shadesPerPixel,
            ( >= 1, >= 1, 1) => Math.Pow(2, pixelWidth * pixelHeight),
            _ => throw new InvalidOperationException("Unrecognized set of parameters for character array.")
        };
        if (characters.Length != correctLength)
            throw new ArgumentException($"The {nameof(characters)} array must contain 2^({nameof(pixelWidth)} * {nameof(pixelHeight)}) * {nameof(shadesPerPixel)} = {correctLength} elements to represent all possible combinations");
        _characters = [.. characters];
    }

    /// <summary>
    /// Gets the character represented at the index of the given <paramref name="pixel"/>.
    /// </summary>
    /// <param name="pixel">The 0-based pixel index to get the relevant character for.</param>
    /// <returns>The character at <paramref name="pixel"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public virtual string this[int pixel] => pixel < 0 || pixel >= _characters.Length
        ? throw new ArgumentOutOfRangeException(nameof(pixel), $"The provided pixel index has to be between 0 and {_characters.Length - 1}. The value was: {pixel}. The palette has {PixelWidth}x{PixelHeight} pixels in {ShadesPerPixel} shades.")
        : _characters[pixel];

    private static PixelPalette? _boolean;
    /// <summary>
    /// Predefined palette using only the empty and full characters. Very high compatibility.
    /// <code>
    ///  █
    /// </code>
    /// </summary>
    public static PixelPalette Boolean => _boolean ??= new(1, 1, 1, [ETY, FUL]);

    private static PixelPalette? _halves;
    /// <summary>
    /// Predefined palette using 1/2 half characters. High compatibility (Consolas, Courier, Courier New, Cascadia Code, FixedSys, Lucida Console, Terminal etc.).
    /// <code>
    ///  ▀▄█
    /// </code>
    /// </summary>
    public static PixelPalette Halves => _halves ??= new(1, 2, 1, [ETY, "▀", "▄", FUL]);

    private static PixelPalette? _thirds;
    /// <summary>
    /// Predefined palette using 1/3 third characters. Low frontend and font compatibility (eg. Cascadia Code).
    /// <code>
    ///  🬂🬋🬎🬭🬰🬹█
    /// </code>
    /// </summary>
    public static PixelPalette Thirds => _thirds ??= new(1, 3, 1, [ETY, "🬂", "🬋", "🬎", "🬭", "🬰", "🬹", FUL]);

    private static PixelPalette? _quarters;

    /// <summary>
    /// Predefined palette using 1/4 quarter characters. Low frontend and font compatibility (eg. Cascadia Code).
    /// <code>
    ///  🮂𜴆▀𜴧𜴪𜴳🮅▂𜶮𜶷𜶺▄𜷝▆█
    /// </code>
    /// </summary>
    public static PixelPalette Quarters => _quarters ??= new(1, 4, 1, [
        ETY, "🮂", "𜴆", "▀", "𜴧", "𜴪", "𜴳", "🮅",
        "▂", "𜶮", "𜶷", "𜶺", "▄", "𜷝", "▆", FUL]);

    private static PixelPalette? _quadrants;

    /// <summary>
    /// Predefined palette using 2/2 quadrant characters. High frontend and font compatibility (Consolas, Cascadia Code, Courier, Courier New, FixedSys, Lucida Console).
    /// <code>
    ///  ▘▝▀▖▌▞▛
    /// ▗▚▐▜▄▙▟█
    /// </code>
    /// </summary>
    public static PixelPalette Quadrants => _quadrants ??= new(2, 2, 1, [
        ETY, "▘", "▝", "▀", "▖", "▌", "▞", "▛",
        "▗", "▚", "▐", "▜", "▄", "▙", "▟", FUL]);

    private static PixelPalette? _sextants;
    /// <summary>
    /// Predefined palette using 2/3 sextant characters. Low frontend and font compatibility (eg. Cascadia Code).
    /// <code>
    ///  🬀🬁🬂🬃🬄🬅🬆🬇🬈🬉🬊🬋🬌🬍🬎
    /// 🬏🬐🬑🬒🬓▌🬔🬕🬖🬗🬘🬙🬚🬛🬜🬝
    /// 🬞🬟🬠🬡🬢🬣🬤🬥🬦🬧▐🬨🬩🬪🬫🬬
    /// 🬭🬮🬯🬰🬱🬲🬳🬴🬵🬶🬷🬸🬹🬺🬻█
    /// </code>
    /// </summary>
    public static PixelPalette Sextants => _sextants ??= new(2, 3, 1, [
        ETY, "🬀", "🬁", "🬂", "🬃", "🬄", "🬅", "🬆", "🬇", "🬈", "🬉", "🬊", "🬋", "🬌", "🬍", "🬎",
        "🬏", "🬐", "🬑", "🬒", "🬓", "▌", "🬔", "🬕", "🬖", "🬗", "🬘", "🬙", "🬚", "🬛", "🬜", "🬝",
        "🬞", "🬟", "🬠", "🬡", "🬢", "🬣", "🬤", "🬥", "🬦", "🬧", "▐", "🬨", "🬩", "🬪", "🬫", "🬬",
        "🬭", "🬮", "🬯", "🬰", "🬱", "🬲", "🬳", "🬴", "🬵", "🬶", "🬷", "🬸", "🬹", "🬺", "🬻", FUL]);

    private static PixelPalette? _octants;
    /// <summary>
    /// Predefined palette using 2/4 octant characters. Low frontend and font compatibility (eg. Cascadia Code).
    /// <code>
    ///  𜺨𜺫🮂𜴀▘𜴁𜴂𜴃𜴄▝𜴅𜴆𜴇𜴈▀
    /// 𜴉𜴊𜴋𜴌🯦𜴍𜴎𜴏𜴐𜴑𜴒𜴓𜴔𜴕𜴖𜴗
    /// 𜴘𜴙𜴚𜴛𜴜𜴝𜴞𜴟🯧𜴠𜴡𜴢𜴣𜴤𜴥𜴦
    /// 𜴧𜴨𜴩𜴪𜴫𜴬𜴭𜴮𜴯𜴰𜴱𜴲𜴳𜴴𜴵🮅
    /// 𜺣𜴶𜴷𜴸𜴹𜴺𜴻𜴼𜴽𜴾𜴿𜵀𜵁𜵂𜵃𜵄
    /// ▖𜵅𜵆𜵇𜵈▌𜵉𜵊𜵋𜵌▞𜵍𜵎𜵏𜵐▛
    /// 𜵑𜵒𜵓𜵔𜵕𜵖𜵗𜵘𜵙𜵚𜵛𜵜𜵝𜵞𜵟𜵠
    /// 𜵡𜵢𜵣𜵤𜵥𜵦𜵧𜵨𜵩𜵪𜵫𜵬𜵭𜵮𜵯𜵰
    /// 𜺠𜵱𜵲𜵳𜵴𜵵𜵶𜵷𜵸𜵹𜵺𜵻𜵼𜵽𜵾𜵿
    /// 𜶀𜶁𜶂𜶃𜶄𜶅𜶆𜶇𜶈𜶉𜶊𜶋𜶌𜶍𜶎𜶏
    /// ▗𜶐𜶑𜶒𜶓▚𜶔𜶕𜶖𜶗▐𜶘𜶙𜶚𜶛▜
    /// 𜶜𜶝𜶞𜶟𜶠𜶡𜶢𜶣𜶤𜶥𜶦𜶧𜶨𜶩𜶪𜶫
    /// ▂𜶬𜶭𜶮𜶯𜶰𜶱𜶲𜶳𜶴𜶵𜶶𜶷𜶸𜶹𜶺
    /// 𜶻𜶼𜶽𜶾𜶿𜷀𜷁𜷂𜷃𜷄𜷅𜷆𜷇𜷈𜷉𜷊
    /// 𜷋𜷌𜷍𜷎𜷏𜷐𜷑𜷒𜷓𜷔𜷕𜷖𜷗𜷘𜷙𜷚
    /// ▄𜷛𜷜𜷝𜷞▙𜷟𜷠𜷡𜷢▟𜷣▆𜷤𜷥█
    /// </code>
    /// </summary>
    public static PixelPalette Octants => _octants ??= new(2, 4, 1, [
        ETY, "𜺨", "𜺫", "🮂", "𜴀", "▘", "𜴁", "𜴂", "𜴃", "𜴄", "▝", "𜴅", "𜴆", "𜴇", "𜴈", "▀",
        "𜴉", "𜴊", "𜴋", "𜴌", "🯦", "𜴍", "𜴎", "𜴏", "𜴐", "𜴑", "𜴒", "𜴓", "𜴔", "𜴕", "𜴖", "𜴗",
        "𜴘", "𜴙", "𜴚", "𜴛", "𜴜", "𜴝", "𜴞", "𜴟", "🯧", "𜴠", "𜴡", "𜴢", "𜴣", "𜴤", "𜴥", "𜴦",
        "𜴧", "𜴨", "𜴩", "𜴪", "𜴫", "𜴬", "𜴭", "𜴮", "𜴯", "𜴰", "𜴱", "𜴲", "𜴳", "𜴴", "𜴵", "🮅",
        "𜺣", "𜴶", "𜴷", "𜴸", "𜴹", "𜴺", "𜴻", "𜴼", "𜴽", "𜴾", "𜴿", "𜵀", "𜵁", "𜵂", "𜵃", "𜵄",
        "▖", "𜵅", "𜵆", "𜵇", "𜵈", "▌", "𜵉", "𜵊", "𜵋", "𜵌", "▞", "𜵍", "𜵎", "𜵏", "𜵐", "▛",
        "𜵑", "𜵒", "𜵓", "𜵔", "𜵕", "𜵖", "𜵗", "𜵘", "𜵙", "𜵚", "𜵛", "𜵜", "𜵝", "𜵞", "𜵟", "𜵠",
        "𜵡", "𜵢", "𜵣", "𜵤", "𜵥", "𜵦", "𜵧", "𜵨", "𜵩", "𜵪", "𜵫", "𜵬", "𜵭", "𜵮", "𜵯", "𜵰",
        "𜺠", "𜵱", "𜵲", "𜵳", "𜵴", "𜵵", "𜵶", "𜵷", "𜵸", "𜵹", "𜵺", "𜵻", "𜵼", "𜵽", "𜵾", "𜵿",
        "𜶀", "𜶁", "𜶂", "𜶃", "𜶄", "𜶅", "𜶆", "𜶇", "𜶈", "𜶉", "𜶊", "𜶋", "𜶌", "𜶍", "𜶎", "𜶏",
        "▗", "𜶐", "𜶑", "𜶒", "𜶓", "▚", "𜶔", "𜶕", "𜶖", "𜶗", "▐", "𜶘", "𜶙", "𜶚", "𜶛", "▜",
        "𜶜", "𜶝", "𜶞", "𜶟", "𜶠", "𜶡", "𜶢", "𜶣", "𜶤", "𜶥", "𜶦", "𜶧", "𜶨", "𜶩", "𜶪", "𜶫",
        "▂", "𜶬", "𜶭", "𜶮", "𜶯", "𜶰", "𜶱", "𜶲", "𜶳", "𜶴", "𜶵", "𜶶", "𜶷", "𜶸", "𜶹", "𜶺",
        "𜶻", "𜶼", "𜶽", "𜶾", "𜶿", "𜷀", "𜷁", "𜷂", "𜷃", "𜷄", "𜷅", "𜷆", "𜷇", "𜷈", "𜷉", "𜷊",
        "𜷋", "𜷌", "𜷍", "𜷎", "𜷏", "𜷐", "𜷑", "𜷒", "𜷓", "𜷔", "𜷕", "𜷖", "𜷗", "𜷘", "𜷙", "𜷚",
        "▄", "𜷛", "𜷜", "𜷝", "𜷞", "▙", "𜷟", "𜷠", "𜷡", "𜷢", "▟", "𜷣", "▆", "𜷤", "𜷥", FUL]);

    private static PixelPalette? _shades;
    /// <summary>
    /// Predefined palette using shading characters. Very high compatibility.
    /// <code>
    ///  ░▒▓█
    /// </code>
    /// </summary>
    public static PixelPalette Shades { get; } = _shades ??= new(1, 1, 5, [ETY, "░", "▒", "▓", FUL]);
}
