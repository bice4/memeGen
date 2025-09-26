using System.Diagnostics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MemeGen.Playground;

/// <summary>
/// Playground program for testing image processing and text rendering.
/// </summary>
internal abstract class Program
{
    private static readonly string[] TestQuotes =
    [
        "test",
        "This is a test",
        "This is a longer test quote",
        "This is a much longer test quote that should wrap around to multiple lines and be quite",
        " long indeed"
    ];

    private static void Main(string[] args)
    {
        Directory.CreateDirectory("output");
        Directory.CreateDirectory("input");

        var images = Directory.GetFiles("input");

        foreach (var image in images)
        {
            Console.WriteLine("Processing " + image);
            foreach (var quote in TestQuotes)
            {
                Generate(quote, image);
            }
        }
    }

    private static void Generate(string quote, string imagePath)
    {
        var st = Stopwatch.StartNew();
        using Image input = Image.Load<Rgba32>(imagePath);
        const float padding = 28f;

        var width = input.Width;
        var height = input.Height;

        var fonts = new FontCollection();
        var mainFont = fonts.Add("Roboto-Regular.ttf");

        var fsMin = 1f; // min FontSize
        var fsMax = 65f; // start max FontSize 
        var fontSize = fsMax;
        var measured = FontRectangle.Empty;


        while (fsMin <= fsMax)
        {
            var mid = (fsMin + fsMax) / 2f;
            measured = FindSize(quote, mid, mainFont);

            if (measured.Width + padding * 2 > width)
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


        var font = mainFont.CreateFont(fontSize, FontStyle.Regular);

        bool atTop = false;

        RectangleF backgroundRectangle;
        PointF textLocation;

        if (atTop)
        {
            backgroundRectangle = new RectangleF(
                0,
                0,
                width,
                measured.Height + padding * 2f);

            var x = backgroundRectangle.Width / 2 - measured.Width / 2;
            var y = backgroundRectangle.Height / 2 - measured.Height / 2;
            textLocation = new PointF(x, y);
        }
        else
        {
            var rectHeight = measured.Height + padding * 2f;

            backgroundRectangle = new RectangleF(
                0, height - rectHeight, width, rectHeight);

            var x = backgroundRectangle.X + backgroundRectangle.Width / 2 - measured.Width / 2;
            var y = backgroundRectangle.Y + (backgroundRectangle.Height / 2 - measured.Height / 2);
            textLocation = new PointF(x, y);
        }


        Console.WriteLine(
            $"Drawing text: width: {backgroundRectangle.Width}, height: {backgroundRectangle.Height} font: {measured.Width}, fontSize: {fontSize}");
        input.Mutate(ctx =>
        {
            ctx.Fill(new Rgba32(0, 0, 0, 120), backgroundRectangle);
            ctx.DrawText(quote, font, Color.White, textLocation);
        });

        input.SaveAsJpeg($"output/{Path.GetFileNameWithoutExtension(imagePath)}.{quote}.jpeg");
        st.Stop();
        Console.WriteLine(
            $"Generated meme for {Path.GetFileNameWithoutExtension(imagePath)} in {st.ElapsedMilliseconds}ms");
    }

    private static FontRectangle FindSize(string text, float prev, FontFamily family)
    {
        var font = family.CreateFont(prev, FontStyle.Regular);
        var opts = new TextOptions(font);
        return TextMeasurer.MeasureSize(text, opts);
    }
}