using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AdRasta2.Enums;
using AdRasta2.Models;
using CliWrap;

namespace AdRasta2.Utils;

public class ImageUtils
{
    public static Bitmap CreateBlankImage(int width, int height, IBrush brush)
    {
        var renderTarget = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));

        using (var ctx = renderTarget.CreateDrawingContext(true))
        {
            // fill with transparent or solid color
            ctx.FillRectangle(brush, new Rect(0, 0, width, height));

            // Prepare watermark text
            var text = new FormattedText(
                "No Image",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI", FontStyle.Italic, FontWeight.Medium),
                30,
                new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)) // 50% transparent black
            );

            // Center the text
            var y = (height - text.Height) / 2;
            var x = (width - text.Width) / 2;

            ctx.DrawText(text, new Point(x, y));
        }

        return renderTarget;
    }

    public static Dictionary<int, int>? CountUniqeColoursPerLine(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            return null;

        using var bitmap = SKBitmap.Decode(imagePath);
        if (bitmap == null || bitmap.Width == 0 || bitmap.Height == 0)
            return null;

        return CountUniqueColoursPerLine(bitmap);
    }
    
    public static Dictionary<int, int> CountUniqueColoursPerLine(SKBitmap bitmap)
    {
        var lineColorCounts = new Dictionary<int, int>();

        for (int y = 0; y < bitmap.Height; y++)
        {
            var uniqueColors = new HashSet<SKColor>();

            for (int x = 0; x < bitmap.Width; x++)
            {
                uniqueColors.Add(bitmap.GetPixel(x, y));
            }

            lineColorCounts[y] = uniqueColors.Count;
        }

        return lineColorCounts;
    }

    public static int CountUniqueColors(string imagePath)
    {
        using var bitmap = SKBitmap.Decode(imagePath);
        return CountUniqueColors(bitmap);
    }

    public static int CountUniqueColors(SKBitmap bitmap)
    {
        var uniqueColors = new HashSet<SKColor>();

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                uniqueColors.Add(bitmap.GetPixel(x, y));
            }
        }

        return uniqueColors.Count;
    }

    public static int CountUniqueColors(Bitmap bitmap)
    {
        var skBitmap = ConvertAvaloniaBitmapToSKBitmap(bitmap);
        return CountUniqueColors(skBitmap);
    }

    public static SKBitmap ConvertAvaloniaBitmapToSKBitmap(Bitmap avaloniaBitmap)
    {
        using var stream = new MemoryStream();
        avaloniaBitmap.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return SKBitmap.Decode(stream);
    }

    public static async Task<bool> ViewImage(string fileName)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        var ret = false;

        try
        {
            if (File.Exists(fileName))
            {
                var result = await Cli.Wrap(Settings.Current.DefaultExecuteCommand)
                    .WithArguments(SafeCommand.QuoteIfNeeded(fileName))
                    .WithValidation(CommandResultValidation.None)
                    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                    .ExecuteAsync();

                var stdOut = stdOutBuffer.ToString();
                var stdErr = stdErrBuffer.ToString();

                ret = true;
            }
            else
            {
                ret = false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ret = false;
        }

        return ret;
    }

    /// <summary>
    /// Horizontally flips an image in-place using SkiaSharp.
    /// Logs success or failure to ConversionLog without throwing exceptions.
    /// </summary>
    /// <param name="imagePath">The path to the image file to flip.</param>
    public static bool FlipImageHorizontally(RastaConversion conversion, string imagePath)
    {
        var ret = true;

        try
        {
            if (!File.Exists(imagePath))
            {
                ConversionLogger.LogIfDebug(conversion, ConversionStatus.FileNotFound,
                    $"[Flip] File not found: {imagePath}");
                return false;
            }

            using var input = File.OpenRead(imagePath);
            using var bitmap = SKBitmap.Decode(input);

            if (bitmap == null)
            {
                ConversionLogger.LogIfDebug(conversion, ConversionStatus.Error,
                    $"[Flip] Failed to decode image: {imagePath}");
                return false;
            }

            using var surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height));
            var canvas = surface.Canvas;

            canvas.Scale(-1, 1);
            canvas.Translate(-bitmap.Width, 0);
            canvas.DrawBitmap(bitmap, 0, 0);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            using var output = File.OpenWrite(imagePath);
            data.SaveTo(output);

            ConversionLogger.LogIfDebug(conversion, ConversionStatus.Success,
                $"Image flipped successfully: {imagePath}");

            return true;
        }
        catch (Exception ex)
        {
            ConversionLogger.LogIfDebug(conversion, ConversionStatus.Error, $"Error flipping image: {imagePath} — {ex.Message}");
            return false;
        }
    }
    
    public static ColourBiasResult GetColourBias(RastaConversion conversion, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
        {
            ConversionLogger.LogIfDebug(conversion,ConversionStatus.Error, $"File not found: {fileName}");
            return new ColourBiasResult { Bias = ColourBias.Unknown, Confidence = 0 };
        }

        using var input = File.OpenRead(fileName);
        using var bitmap = SKBitmap.Decode(input);

        if (bitmap == null)
        {
            ConversionLogger.LogIfDebug(conversion,ConversionStatus.Error, $"Failed to decode image: {fileName}");
            return new ColourBiasResult { Bias = ColourBias.Unknown, Confidence = 0 };
        }

        int width = bitmap.Width;
        int height = bitmap.Height;

        var leftSet = new HashSet<SKColor>();
        var rightSet = new HashSet<SKColor>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width / 2; x++)
            {
                leftSet.Add(bitmap.GetPixel(x, y));
                rightSet.Add(bitmap.GetPixel(width - 1 - x, y));
            }
        }

        int leftCount = leftSet.Count;
        int rightCount = rightSet.Count;
        int total = leftCount + rightCount;

        var bias = leftCount == rightCount
            ? ColourBias.Unknown
            : (rightCount > leftCount ? ColourBias.Right : ColourBias.Left);

        var confidence = total == 0 ? 0 : 100.0 * Math.Max(leftCount, rightCount) / total;

        ConversionLogger.LogIfDebug(conversion,ConversionStatus.Success,$"{bias} edge dominant ({confidence:F1}%) in {Path.GetFileName(fileName)}");
        
        return new ColourBiasResult { Bias = bias, Confidence = confidence };
        
        // Calculation for 'Is Colourful'
        // int leftScore = 0;
        // int rightScore = 0;
        //
        // for (int y = 0; y < height; y++)
        // {
        //     for (int x = 0; x < width / 2; x++)
        //     {
        //         var leftPixel = bitmap.GetPixel(x, y);
        //         var rightPixel = bitmap.GetPixel(width - 1 - x, y);
        //
        //         if (IsColourful(leftPixel)) leftScore++;
        //         if (IsColourful(rightPixel)) rightScore++;
        //     }
        // }
        //
        // var bias = leftScore == rightScore
        //     ? ColourBias.Equal
        //     : (rightScore > leftScore ? ColourBias.Right : ColourBias.Left);
        //
        // var totalScore = leftScore + rightScore;
        // var confidence = totalScore == 0 ? 0 : 100.0 * Math.Max(leftScore, rightScore) / totalScore;
        //

    }
    
    private static bool IsColourful(SKColor color)
    {
        int range = Math.Abs(color.Red - color.Green) + Math.Abs(color.Green - color.Blue);
        return range > 50; // Threshold for colourfulness
    }

}