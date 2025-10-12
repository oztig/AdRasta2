using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AdRasta2.Models;
using Avalonia.Media.Imaging;
using MsBox.Avalonia.Enums;
using SkiaSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
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

    public async Task GenerateIconAsync(SKBitmap sourceBitmap, string destinationPath, string overlayText = "β18")
    {
        var info = new SKImageInfo(IconSize, IconSize);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);

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

        using var pngStream = new MemoryStream();
        surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100)?.SaveTo(pngStream);
        pngStream.Seek(0, SeekOrigin.Begin);

        using var image = await Image.LoadAsync<Rgba32>(pngStream);

        // Always generate BMP for SDL
        var bmpPath = Path.Combine(destinationPath, "RastaConverter.bmp");
        await image.SaveAsync(bmpPath, new BmpEncoder());

        // If Windows, also generate ICO using embedded writer
        if (Settings.IsWindows)
        {
            var icoPath = Path.Combine(destinationPath, "RastaConverter.ico");
            WriteIcoFromPng(pngStream.ToArray(), icoPath);
        }
    }

    private void WriteIcoFromPng(byte[] pngBytes, string outputPath)
    {
        using var stream = new FileStream(outputPath, FileMode.Create);

        // ICO header
        stream.WriteByte(0); // reserved
        stream.WriteByte(0);
        stream.WriteByte(1); // image type: icon
        stream.WriteByte(0);
        stream.WriteByte(1); // number of images
        stream.WriteByte(0);

        // Image entry
        stream.WriteByte(IconSize); // width
        stream.WriteByte(IconSize); // height
        stream.WriteByte(0); // colors
        stream.WriteByte(0); // reserved
        stream.WriteByte(1); // color planes
        stream.WriteByte(0);
        stream.WriteByte(32); // bits per pixel
        stream.WriteByte(0);

        int imageSize = pngBytes.Length;
        int imageOffset = 6 + 16; // header + entry

        stream.WriteByte((byte)(imageSize & 0xFF));
        stream.WriteByte((byte)((imageSize >> 8) & 0xFF));
        stream.WriteByte((byte)((imageSize >> 16) & 0xFF));
        stream.WriteByte((byte)((imageSize >> 24) & 0xFF));

        stream.WriteByte((byte)(imageOffset & 0xFF));
        stream.WriteByte((byte)((imageOffset >> 8) & 0xFF));
        stream.WriteByte((byte)((imageOffset >> 16) & 0xFF));
        stream.WriteByte((byte)((imageOffset >> 24) & 0xFF));

        // Image data
        stream.Write(pngBytes, 0, pngBytes.Length);
    }


    // Invocation from bitmap
    // public async Task GenerateIconAsync(SKBitmap sourceBitmap, string destinationPath, string overlayText = "β18")
    // {
    //     var info = new SKImageInfo(IconSize, IconSize);
    //     using var surface = SKSurface.Create(info);
    //     var canvas = surface.Canvas;
    //     canvas.Clear(SKColors.Black);
    //
    //     try
    //     {
    //         var fullIconPath= Path.Combine(destinationPath, "RastaConverter.bmp");
    //         var scaledRect = GetScaledRect(sourceBitmap.Width, sourceBitmap.Height, IconSize, IconSize);
    //         canvas.DrawBitmap(sourceBitmap, scaledRect);
    //
    //         var textPaint = new SKPaint
    //         {
    //             Color = SKColors.White,
    //             TextSize = 10,
    //             IsAntialias = true,
    //             Typeface = SKTypeface.Default
    //         };
    //         canvas.DrawText(overlayText, 2, IconSize - 4, textPaint);
    //
    //         // Encode to PNG in memory
    //         using var pngStream = new MemoryStream();
    //         surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100)?.SaveTo(pngStream);
    //         pngStream.Seek(0, SeekOrigin.Begin);
    //
    //         // Load PNG and save as BMP using ImageSharp
    //         using var image = await Image.LoadAsync<Rgba32>(pngStream);
    //         await image.SaveAsBmpAsync(fullIconPath);
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e);
    //         throw;
    //     }
    // }

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