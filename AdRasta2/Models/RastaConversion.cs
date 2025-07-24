using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.IO;
using System.Linq.Expressions;
using AdRasta2.Utils;
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
                    _image = ImageUtils.CreateBlankImage(320, 240);
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