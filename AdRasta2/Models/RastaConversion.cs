using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.IO;
using System.Linq.Expressions;
using Avalonia.Controls.Converters;
using Avalonia.Media;
using ReactiveUI;

namespace AdRasta2.Models;

public class RastaConversion : ReactiveObject
{

    private string _title;
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private string _imagePath;

    public string ImagePath
    {
        get => _imagePath;
        set
        {
            // Manully check if changed, so we can force re-load of the Image.
            if (_imagePath != value)
            {
                _imagePath = value;
                this.RaisePropertyChanged(nameof(ImagePath));
                _image = null;
                this.RaisePropertyChanged(nameof(Image));
            }
        }
    }

    private Bitmap? _image;
    public Bitmap? Image
    {
        get
        {
            if (_image != null) return _image;

            try
            {
                if (File.Exists(ImagePath))
                    _image = new Bitmap(ImagePath);
                else
                {
                    _image = CreateBlankImage(320, 240);
                }
            }
            catch(Exception ex)
            {
                // optional: log or fallback
                _image = null;
            }

            return _image;
        }
    }
    
    public static Bitmap CreateBlankImage(int width, int height)
    {
        var renderTarget = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));

        using (var ctx = renderTarget.CreateDrawingContext(true))
        {
            // Optionally fill with transparent or solid color
            ctx.FillRectangle(Brushes.WhiteSmoke, new Rect(0, 0, width, height));
            
            // Prepare watermark text
            var text = new FormattedText(
                "No Image",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI",FontStyle.Italic,FontWeight.Medium),
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

    private bool _isSelected;
    
    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public RastaConversion(string title, string imagePath)
    {
        Title = title;
        ImagePath = imagePath;
    }

    public RastaConversion()
    {
        
    }
}