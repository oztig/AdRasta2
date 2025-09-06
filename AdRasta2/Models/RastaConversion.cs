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

    // Input
    private string _sourceImagePath;

    public string SourceImagePath
    {
        get => _sourceImagePath;
        set
        {
            // Manually check if changed, so we can force re-load of the Image.
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
                    _sourceImage = ImageUtils.CreateBlankImage(320, 240, Brushes.WhiteSmoke);
                }
            }
            catch (Exception ex)
            {
                _sourceImage = null;
            }

            return _sourceImage;
        }
    }

    private string _sourceImageMaskPath;

    public string SourceImageMaskPath
    {
        get => _sourceImageMaskPath;
        set
        {
            // Manully check if changed, so we can force re-load of the Image.
            if (_sourceImageMaskPath != value)
            {
                _sourceImageMaskPath = value;
                this.RaisePropertyChanged();
                _sourceImageMask = null;
                this.RaisePropertyChanged(nameof(SourceImageMask));
            }
        }
    }

    private Bitmap? _sourceImageMask;

    public Bitmap? SourceImageMask
    {
        get
        {
            if (_sourceImageMask != null) return _sourceImageMask;

            try
            {
                if (File.Exists(SourceImageMaskPath))
                    _sourceImageMask = new Bitmap(SourceImageMaskPath);
                else
                {
                    _sourceImageMask = ImageUtils.CreateBlankImage(320, 240, Brushes.WhiteSmoke);
                }
            }
            catch (Exception ex)
            {
                _sourceImageMask = null;
            }

            return _sourceImageMask;
        }
    }

    private string _destinationFullFilePath;

    public string DestinationFilePath
    {
        get => _destinationFullFilePath;
        set => this.RaiseAndSetIfChanged(ref _destinationFullFilePath, value);
    }

    private string _imagePreviewPath;

    public string ImagePreviewPath
    {
        get => _imagePreviewPath;
        set
        {
            // Manully check if changed, so we can force re-load of the Image.
            if (_imagePreviewPath != value)
            {
                _imagePreviewPath = value;
                this.RaisePropertyChanged();
                _imagePreview = null;
                this.RaisePropertyChanged(nameof(ImagePreviewPath));
            }
        }
    }

    private Bitmap? _imagePreview;

    public Bitmap? ImagePreview
    {
        get
        {
            if (_imagePreview != null) return _imagePreview;

            try
            {
                if (File.Exists(ImagePreviewPath))
                    _imagePreview = new Bitmap(ImagePreviewPath);
                else
                {
                    _imagePreview = ImageUtils.CreateBlankImage(320, 240, Brushes.WhiteSmoke);
                }
            }
            catch (Exception ex)
            {
                _imagePreview = null;
            }

            return _imagePreview;
        }
    }

    private int? _height = 240;

    public int? Height
    {
        get => _height;
        set
        {
            value ??= 0;
            this.RaiseAndSetIfChanged(ref _height, value);
        }
    }

    private string _resizeFilter;

    public string ResizeFilter
    {
        get => _resizeFilter;
        set => this.RaiseAndSetIfChanged(ref _resizeFilter, value);
    }

    private string _palette;

    public string Palette
    {
        get => _palette;
        set => this.RaiseAndSetIfChanged(ref _palette, value);
    }

    private string _preColourDistance;

    public string PreColourDistance
    {
        get => _preColourDistance;
        set
        {
            this.RaiseAndSetIfChanged(ref _preColourDistance, value);
            // If ciede and knoll - display a warning on Preview button - LATER
            // SetPreviewButtonWarning();
        }
    }

    private string _dithering;

    public string Dithering
    {
        get => _dithering;
        set
        {
            this.RaiseAndSetIfChanged(ref _dithering, value);
            //  SetPreviewButtonWarning();
        }
    }

    private decimal? _ditheringStrength = 1;

    public decimal? DitheringStrength
    {
        get => _ditheringStrength;
        set => this.RaiseAndSetIfChanged(ref _ditheringStrength, value);
    }

    private decimal? _ditheringRandomness = 0;

    public decimal? DitheringRandomness
    {
        get => _ditheringRandomness;
        set => this.RaiseAndSetIfChanged(ref _ditheringRandomness, value);
    }

    private decimal? _brightness = 0;

    public decimal? Brightness
    {
        get => _brightness;
        set
        {
            value ??= 0;
            this.RaiseAndSetIfChanged(ref _brightness, value);
        }
    }

    private decimal? _contrast = 0;

    public decimal? Contrast
    {
        get => _contrast;
        set
        {
            value ??= 0;
            this.RaiseAndSetIfChanged(ref _contrast, value);
        }
    }

    private decimal? _gamma = 1;

    public decimal? Gamma
    {
        get => _gamma;
        set
        {
            value ??= 0;
            this.RaiseAndSetIfChanged(ref _gamma, value);
        }
    }

    private decimal? _maskStrength = (decimal?)1.0;

    public decimal? MaskStrength
    {
        get => _maskStrength;
        set
        {
            value ??= 0;
            this.RaiseAndSetIfChanged(ref _maskStrength, value);
        }
    }

    private string _registerOnOffFilePath = string.Empty;

    public string RegisterOnOffFilePath
    {
        get => _registerOnOffFilePath;
        set
        {
            this.RaiseAndSetIfChanged(ref _registerOnOffFilePath, value);
            // LATER !
            // RegisterOnOffFileBasename = value;
        }
    }

    private string _colourDistance;

    public string ColourDistance
    {
        get => _colourDistance;
        set => this.RaiseAndSetIfChanged(ref _colourDistance, value);
    }

    private string _initialState;

    public string InitialState
    {
        get => _initialState;
        set => this.RaiseAndSetIfChanged(ref _initialState, value);
    }

    private decimal? _solutionHistoryLength = 1;

    public decimal? SolutionHistoryLength
    {
        get => _solutionHistoryLength;
        set
        {
            value ??= 0;
            this.RaiseAndSetIfChanged(ref _solutionHistoryLength, value);
        }
    }

    private string _autoSavePeriod;

    public string AutoSavePeriod
    {
        get => _autoSavePeriod;
        set => this.RaiseAndSetIfChanged(ref _autoSavePeriod, value);
    }

    private int _threads;

    public int Threads
    {
        get => _threads;
        set => this.RaiseAndSetIfChanged(ref _threads, value);
    }

    // New, Previosuly unset Params in previous AdRasta
    private decimal _randomSeed;

    public decimal RandomSeed
    {
        get => _randomSeed;
        set => this.RaiseAndSetIfChanged(ref _randomSeed, value);
    }

    private int _maxEvaluations = 0;

    public int MaxEvaluations
    {
        get => _maxEvaluations;
        set => this.RaiseAndSetIfChanged(ref _maxEvaluations, value);
    }

    private string _optimiser;

    public string Optimiser
    {
        get => _optimiser;
        set => this.RaiseAndSetIfChanged(ref _optimiser, value);
    }

    private int _cacheInMB = 64;

    public int CacheInMB
    {
        get => _cacheInMB;
        set => this.RaiseAndSetIfChanged(ref _cacheInMB, value);
    }

    // Dual Mode specific
    private bool _dualFrameMode;

    public bool DualFrameMode
    {
        get => _dualFrameMode;
        set => this.RaiseAndSetIfChanged(ref _dualFrameMode, value);
    }

    private int _firstDualSteps = 100000;

    public int FirstDualSteps
    {
        get => _firstDualSteps;
        set => this.RaiseAndSetIfChanged(ref _firstDualSteps, value);
    }

    private string _afterDualSteps;

    public string AfterDualSteps
    {
        get => _afterDualSteps;
        set => this.RaiseAndSetIfChanged(ref _afterDualSteps, value);
    }

    private int _alternatingDualSteps = 50000;

    public int AlternatingDualSteps
    {
        get => _alternatingDualSteps;
        set => this.RaiseAndSetIfChanged(ref _alternatingDualSteps, value);
    }

    private string _dualBlending;

    public string DualBlending
    {
        get => _dualBlending;
        set => this.RaiseAndSetIfChanged(ref _dualBlending, value);
    }

    private decimal _dualLuma = (decimal)0.2;

    public decimal DualLuma
    {
        get => _dualLuma;
        set => this.RaiseAndSetIfChanged(ref _dualLuma, value);
    }

    private decimal _dualChroma = (decimal)0.1;

    public decimal DualChroma
    {
        get => _dualChroma;
        set => this.RaiseAndSetIfChanged(ref _dualChroma, value);
    }


    // Current Conversion ?
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public RastaConversion(string title)
    {
        PopulateDefaultValues();
        Title = title;
    }

    public void PopulateDefaultValues()
    {
        Title = "New Conversion";
        Threads = RastaConverterDefaultValues.DefaultThreads;
        MaxEvaluations = RastaConverterDefaultValues.DefaultMaxEvaluations;
        AutoSavePeriod = RastaConverterDefaultValues.DefaultAutoSavePeriod;
        RandomSeed = RastaConverterDefaultValues.DefaultRandomSeed;
        Height = RastaConverterDefaultValues.DefaultHeight;
        Palette = RastaConverterDefaultValues.DefaultPalette;
        Dithering = RastaConverterDefaultValues.DefaultDithering;
        DitheringStrength = RastaConverterDefaultValues.DefaultDitheringStrength;
        DitheringRandomness = RastaConverterDefaultValues.DefaultDitheringRandomness;
        MaskStrength = RastaConverterDefaultValues.DefaultMaskStrength;
        ResizeFilter = RastaConverterDefaultValues.DefaultResizeFilter;
        Brightness = RastaConverterDefaultValues.DefaultBrightness;
        Contrast = RastaConverterDefaultValues.DefaultContrast;
        Gamma = RastaConverterDefaultValues.DefaultGamma;
        InitialState = RastaConverterDefaultValues.DefaultInitialState;
        SolutionHistoryLength = RastaConverterDefaultValues.DefaultSolutionHistoryLength;
        Optimiser = RastaConverterDefaultValues.DefultOptimiser;
        ColourDistance = RastaConverterDefaultValues.DefaultColourDistance;
        PreColourDistance = RastaConverterDefaultValues.DefaultPreColourDistance;
        CacheInMB = RastaConverterDefaultValues.DefaultCacheInMB;
        DualFrameMode = RastaConverterDefaultValues.DefaultDualFrameMode;
        FirstDualSteps = RastaConverterDefaultValues.DefualtFirstDualSteps;
        AfterDualSteps = RastaConverterDefaultValues.DefaultAfterDualSteps;
        AlternatingDualSteps = RastaConverterDefaultValues.DefaultAlternatingDualSteps;
        DualBlending = RastaConverterDefaultValues.DefaultDualBlending;
        DualLuma = RastaConverterDefaultValues.DefaultDualLuma;
        DualChroma = RastaConverterDefaultValues.DefaultDualChroma;
        SourceImagePath = RastaConverterDefaultValues.DefaultSourceImagePath;
        SourceImageMaskPath = RastaConverterDefaultValues.DefaultSourceImageMaskPath;
        DestinationFilePath = RastaConverterDefaultValues.DefaultDestinationFilePath;
        RegisterOnOffFilePath = RastaConverterDefaultValues.DefaultRegisterOnOffFilePath;
        
        // public static string DefaultDestantionFilePrefix;
    }
}