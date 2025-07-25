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

    private string _sourceImagePath;

    public string SourceImagePath
    {
        get => _sourceImagePath;
        set
        {
            // Manully check if changed, so we can force re-load of the Image.
            if (_sourceImagePath != value)
            {
                _sourceImagePath = value;
                this.RaisePropertyChanged();
                _sourceImage = null;
                this.RaisePropertyChanged(nameof(SourceImage));
            }
        }
    }

    private Bitmap? _sourceImage;
    public Bitmap? SourceImage
    {
        get
        {
            if (_sourceImage != null) return _sourceImage;

            try
            {
                if (File.Exists(SourceImagePath))
                    _sourceImage = new Bitmap(SourceImagePath);
                else
                {
                    _sourceImage = ImageUtils.CreateBlankImage(320, 240);
                }
            }
            catch(Exception ex)
            {
                _sourceImage = null;
            }

            return _sourceImage;
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
        SourceImagePath = imagePath;
    }
}