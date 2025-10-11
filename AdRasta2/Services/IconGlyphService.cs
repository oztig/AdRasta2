using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AdRasta2.Services;

public class IconGlyphService
{
    private const int IconSize = 64;

    // Invocation from path
    public async Task GenerateIconAsync(string sourceImagePath, string destinationPath, string overlayText = "β18")
    {
        if (string.IsNullOrWhiteSpace(sourceImagePath) || !File.Exists(sourceImagePath)) return;

        using var inputStream = File.OpenRead(sourceImagePath);
        var bitmap = SKBitmap.Decode(inputStream);
        if (bitmap == null) return;

        await GenerateIconAsync(bitmap, destinationPath, overlayText);
    }

    // Invocation from bitmap
    public async Task GenerateIconAsync(SKBitmap sourceBitmap, string destinationPath, string overlayText = "β18")
    {
        var info = new SKImageInfo(IconSize, IconSize);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);

        try
        {
            var fullIconPath= Path.Combine(destinationPath, "RastaConverter.bmp");
            var scaledRect = GetScaledRect(sourceBitmap.Width, sourceBitmap.Height, IconSize, IconSize);
            canvas.DrawBitmap(sourceBitmap, scaledRect);

            var textPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 10,
                IsAntialias = true,
                Typeface = SKTypeface.Default
            };
            canvas.DrawText(overlayText, 2, IconSize - 4, textPaint);

            // Encode to PNG in memory
            using var pngStream = new MemoryStream();
            surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100)?.SaveTo(pngStream);
            pngStream.Seek(0, SeekOrigin.Begin);

            // Load PNG and save as BMP using ImageSharp
            using var image = await Image.LoadAsync<Rgba32>(pngStream);
            await image.SaveAsBmpAsync(fullIconPath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private SKRect GetScaledRect(int srcWidth, int srcHeight, int targetWidth, int targetHeight)
    {
        float scale = Math.Min((float)targetWidth / srcWidth, (float)targetHeight / srcHeight);
        float w = srcWidth * scale;
        float h = srcHeight * scale;
        float x = (targetWidth - w) / 2;
        float y = (targetHeight - h) / 2;
        return new SKRect(x, y, x + w, y + h);
    }
}