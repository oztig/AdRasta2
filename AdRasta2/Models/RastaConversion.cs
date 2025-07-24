using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.IO;
using Avalonia.Controls.Converters;
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

    public string ImagePath { get; }
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
            }
            catch
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