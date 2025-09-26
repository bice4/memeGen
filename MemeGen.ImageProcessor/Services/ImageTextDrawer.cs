using System.Runtime.CompilerServices;
using MemeGen.Contracts.Messaging.V1.Requests;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MemeGen.ImageProcessor.Services;

/// <summary>
/// Draws text on an image with configurable options.
/// </summary>
public static class ImageTextDrawer
{
    private static FontFamily _mainFontFamily;

    private const int MaxTextPadding = 50;
    private const int MinTextPadding = 10;

    private const int MaxBackgroundOpacity = 210;
    private const int MinBackgroundOpacity = 80;

    private const int DefaultTextPadding = 28;
    private const byte DefaultBackgroundOpacity = 120;

    private const float MinFontSize = 1f;
    private const float MaxFontSize = 65f;

    // Cache for fonts to improve performance
    private static readonly Dictionary<float, Font?> FontsCache = new();

    public static void Init()
    {
        var fonts = new FontCollection();
        // Load the main font family from the specified path. Hardcoded for simplicity.
        _mainFontFamily = fonts.Add("data\\Roboto-Regular.ttf");
    }

    public static async Task DrawTextOnImage(string text, string path, ImageProcessingConfig config)
    {
        // Load the image
        using Image input = Image.Load<Rgba32>(path);

        var width = input.Width;

        // If configured to use upper text, convert it
        if (config.UseUpperText)
        {
            text = text.ToUpper();
        }

        // Binary search to find the maximum font size that fits within the image width with padding
        var fsMin = MinFontSize; // min FontSize
        var fsMax = MaxFontSize; // start max FontSize 
        var fontSize = fsMax;
        var measured = FontRectangle.Empty;

        var textPadding = config.TextPadding is <= MaxTextPadding and >= MinTextPadding
            ? config.TextPadding
            : DefaultTextPadding;
        var textPaddingX2 = textPadding * 2;

        while (fsMin <= fsMax)
        {
            var mid = (fsMin + fsMax) / 2f;
            measured = FindSize(text, mid);

            if (measured.Width + textPaddingX2 > width)
            {
                // text weight too high decrease font size
                fsMax = mid - 1f;
            }
            else
            {
                // text fit can increase font size
                fsMin = mid + 1f;
                fontSize = mid;
            }
        }

        // Get the font with the determined size
        var font = GetOrCreateFont(fontSize, FontStyle.Regular);

        RectangleF backgroundRectangle;
        PointF textLocation;

        // Determine background rectangle and text location based on configuration for top or bottom text
        // Also center the text horizontally
        if (config.TextAtTop)
        {
            backgroundRectangle = new RectangleF(
                0,
                0,
                width,
                measured.Height + textPaddingX2);

            var x = backgroundRectangle.Width / 2 - measured.Width / 2;
            var y = backgroundRectangle.Height / 2 - measured.Height / 2;
            textLocation = new PointF(x, y);
        }
        else
        {
            var rectHeight = measured.Height + textPaddingX2;

            backgroundRectangle = new RectangleF(
                0, input.Height - rectHeight, width, rectHeight);

            var x = backgroundRectangle.X + backgroundRectangle.Width / 2 - measured.Width / 2;
            var y = backgroundRectangle.Y + (backgroundRectangle.Height / 2 - measured.Height / 2);
            textLocation = new PointF(x, y);
        }

        // Determine background opacity
        var backgroundOpacity =
            config.BackgroundOpacity is <= MaxBackgroundOpacity and >= MinBackgroundOpacity
                ? (byte)config.BackgroundOpacity
                : DefaultBackgroundOpacity;

        // Draw the background rectangle and text onto the image
        input.Mutate(ctx =>
        {
            ctx.Fill(new Rgba32(0, 0, 0, backgroundOpacity), backgroundRectangle);
            ctx.DrawText(text, font, Color.White, textLocation);
        });

        // Save the modified image
        await input.SaveAsJpegAsync(path);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static FontRectangle FindSize(string text, float fontSize)
    {
        // Get or create the font for the given size
        var font = GetOrCreateFont(fontSize, FontStyle.Regular);
        var opts = new TextOptions(font);
        return TextMeasurer.MeasureSize(text, opts);
    }

    private static Font GetOrCreateFont(float fontSize, FontStyle fontStyle)
    {
        // Check cache first
        if (FontsCache.TryGetValue(fontSize, out var font))
        {
            return font ?? _mainFontFamily.CreateFont(fontSize, fontStyle);
        }

        // Create and cache the font
        font = _mainFontFamily.CreateFont(fontSize, fontStyle);
        FontsCache.Add(fontSize, font);
        return font;
    }
}