using System.Runtime.CompilerServices;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MemeGen.ImageProcessor.Services;

public static class ImageTextDrawer
{
    private static FontFamily _mainFontFamily;

    private const float Padding = 28f;
    private const float PaddingX2 = Padding * 2;
    private const float MinFontSize = 1f;
    private const float MaxFontSize = 65f;

    private static readonly Dictionary<int, float> FontSizeCache = new();

    public static void Init()
    {
        var fonts = new FontCollection();
        _mainFontFamily = fonts.Add("data\\Roboto-Regular.ttf");
    }

    public static async Task DrawTextOnImage(string text, string path)
    {
        using Image input = Image.Load<Rgba32>(path);

        var width = input.Width;

        var fsMin = MinFontSize; // min FontSize
        var fsMax = MaxFontSize; // start max FontSize 
        var fontSize = fsMax;
        var measured = FontRectangle.Empty;

        if (FontSizeCache.TryGetValue(text.Length, out var value))
        {
            fontSize = value;
            measured = FindSize(text, fontSize, _mainFontFamily);
        }
        else
        {
            while (fsMin <= fsMax)
            {
                var mid = (fsMin + fsMax) / 2f;
                measured = FindSize(text, mid, _mainFontFamily);

                if (measured.Width + PaddingX2 > width)
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

            FontSizeCache[text.Length] = fontSize;
        }

        var font = _mainFontFamily.CreateFont(fontSize, FontStyle.Regular);

        var backgroundRectangle = new RectangleF(0, 0, width, measured.Height + PaddingX2);
        var textLocation = new PointF(backgroundRectangle.Width / 2 - measured.Width / 2,
            backgroundRectangle.Height / 2 - measured.Height / 2);

        input.Mutate(ctx =>
        {
            ctx.Fill(new Rgba32(0, 0, 0, 120), backgroundRectangle);
            ctx.DrawText(text, font, Color.White, textLocation);
        });

        await input.SaveAsPngAsync(path);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static FontRectangle FindSize(string text, float fontSize, FontFamily family)
    {
        var font = family.CreateFont(fontSize, FontStyle.Regular);
        var opts = new TextOptions(font);
        return TextMeasurer.MeasureSize(text, opts);
    }
}