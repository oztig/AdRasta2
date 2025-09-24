using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using AdRasta2.Enums;
using AdRasta2.Utils;
using Avalonia.Controls.Converters;
using Avalonia.Media;
using ReactiveUI;

namespace AdRasta2.Models;

public class RastaConversion : ReactiveObject
{
    public Action? ScrollToLatestLogEntry { get; set; }

    public Guid UniqueID { get; set; } = Guid.NewGuid();
    public int ProcessID { get; set; } // Use for any CLI process - used to update status from any CLI await

    private bool _isPreProcessExpanded = true;

    public bool IsPreProcessExpanded
    {
        get => _isPreProcessExpanded;
        set => this.RaiseAndSetIfChanged(ref _isPreProcessExpanded, value);
    }

    private bool _isConversionExpanded = true;

    public bool IsConversionExpanded
    {
        get => _isConversionExpanded;
        set => this.RaiseAndSetIfChanged(ref _isConversionExpanded, value);
    }

    private bool _isDualModeExpanded = true;

    public bool IsDualModeExpanded
    {
        get => _isDualModeExpanded && _dualFrameMode;
        set => this.RaiseAndSetIfChanged(ref _isDualModeExpanded, value);
    }

    private bool _isConfigExpanded = false;

    public bool IsConfigExpanded
    {
        get => _isConfigExpanded;
        set => this.RaiseAndSetIfChanged(ref _isConfigExpanded, value);
    }

    private bool _isCommandLineExpanded = false;

    public bool IsCommandLineExpanded
    {
        get => _isCommandLineExpanded;
        set => this.RaiseAndSetIfChanged(ref _isCommandLineExpanded, value);
    }

    private string _commandLineText = string.Empty;

    public string CommandLineText
    {
        get => _commandLineText;
        set => this.RaiseAndSetIfChanged(ref _commandLineText, value);
    }

    public bool CanProcess => !string.IsNullOrEmpty(SourceImagePath);

    public bool CanContinue => CanProcess && !String.IsNullOrEmpty(ImagePreviewPath);
    
    public bool CanPreview => !DualFrameMode && !string.IsNullOrEmpty(SourceImagePath);

    public bool CanGenerateMCH => CanPreview;

    private string _title;
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private string _executableFileName = string.Empty;

    public string ExecutableFileName
    {
        get => _executableFileName;
        set => this.RaiseAndSetIfChanged(ref _executableFileName, value);
    }

    private BoundedLogCollection<StatusEntry> _statuses;

    public BoundedLogCollection<StatusEntry> Statuses
    {
        get => _statuses;
        set
        {
            if (_statuses == value)
                return;

            if (_statuses != null)
                _statuses.CollectionChanged -= Statuses_CollectionChanged;

            _statuses = value;

            if (_statuses != null)
                _statuses.CollectionChanged += Statuses_CollectionChanged;

            this.RaisePropertyChanged(nameof(Statuses));
        }
    }

    private void Statuses_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
            ScrollToLatestLogEntry?.Invoke();

        UpdateUniqueLatestStatuses();
    }

    private IReadOnlyList<StatusEntry> _uniqueLatestStatuses = new List<StatusEntry>();

    public IReadOnlyList<StatusEntry> UniqueLatestStatuses
    {
        get => _uniqueLatestStatuses;
        private set => this.RaiseAndSetIfChanged(ref _uniqueLatestStatuses, value);
    }

    private void UpdateUniqueLatestStatuses()
    {
        UniqueLatestStatuses = Statuses?
            .Where(s => s.ShowOnImageStatusLine)
            .GroupBy(s => s.Status)
            .Select(g => g.OrderByDescending(s => s.Timestamp).First())
            .OrderBy(s => s.Timestamp)
            .ToList() ?? new List<StatusEntry>();
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
                ImagePreviewPath = null;
                this.RaisePropertyChanged(nameof(SourceImage));
                this.RaisePropertyChanged(nameof(CanProcess));
                this.RaisePropertyChanged(nameof(CanPreview));
                this.RaisePropertyChanged(nameof(CanContinue));
                this.RaisePropertyChanged(nameof(CanGenerateMCH));
                this.RaisePropertyChanged(nameof(SourceImageBaseName));

                CopySourceImageToDestination();
            }
        }
    }

    public string SourceImageDirectory =>
        string.IsNullOrEmpty(SourceImagePath) ? string.Empty : Path.GetDirectoryName(SourceImagePath);


    public string SourceImageBaseName => string.IsNullOrEmpty(SourceImagePath)
        ? string.Empty
        : Path.GetFileName(SourceImagePath);


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
                this.RaisePropertyChanged(nameof(SourceImageMaskBaseName));

                Statuses?.AddEntry(
                    DateTime.Now,
                    string.IsNullOrEmpty(_sourceImageMaskPath)
                        ? ConversionStatus.MaskCleared
                        : ConversionStatus.MaskAdded,
                    _sourceImageMaskPath ?? "");
            }
        }
    }
    
    public string SourceImageMaskDirectory =>
        string.IsNullOrEmpty(SourceImageMaskPath) ? string.Empty : Path.GetDirectoryName(SourceImageMaskPath);

    public string SourceImageMaskBaseName => string.IsNullOrEmpty(SourceImageMaskPath)
        ? string.Empty
        : Path.GetFileName(SourceImageMaskPath);

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

    // Converted Image
    private string _convertedImagePath;

    public string ConvertedImagePath
    {
        get => _convertedImagePath;
        set
        {
            // Manually check if changed, so we can force re-load of the Image.
            if (_convertedImagePath != value)
            {
                _convertedImagePath = value;
                this.RaisePropertyChanged();
                _convertedImage = null;
                this.RaisePropertyChanged(nameof(ConvertedImage));
            }
        }
    }

    private Bitmap? _convertedImage;

    public Bitmap? ConvertedImage
    {
        get
        {
            if (_convertedImage != null) return _convertedImage;

            try
            {
                if (File.Exists(ConvertedImagePath))
                    _convertedImage = new Bitmap(ConvertedImagePath);
                else
                {
                    _convertedImage = ImageUtils.CreateBlankImage(320, 240, Brushes.WhiteSmoke);
                }
            }
            catch (Exception ex)
            {
                _convertedImage = null;
            }

            this.RaisePropertyChanged(nameof(ConvertedImageExists));
            return _convertedImage;
        }
    }

    public bool ConvertedImageExists => ConvertedImage != null;

    private string _destinationFilePath;

    public string DestinationFilePath
    {
        get => _destinationFilePath;
        set
        {
            this.RaiseAndSetIfChanged(ref _destinationFilePath, value);

            Statuses?.AddEntry(
                DateTime.Now,
                string.IsNullOrEmpty(_destinationFilePath)
                    ? ConversionStatus.DestinationCleared
                    : ConversionStatus.DestinationSet,
                _destinationFilePath ?? ""
            );
        }
    }

    public string DestinationDirectory => string.IsNullOrEmpty(DestinationFilePath)
        ? string.Empty
        : DestinationFilePath;

    public string DestinationImageFileName => Path.Combine(DestinationDirectory, SourceImageBaseName);

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
                this.RaisePropertyChanged(nameof(ImagePreview));
                this.RaisePropertyChanged(nameof(CanContinue));
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
                {
                    _imagePreview = new Bitmap(ImagePreviewPath);
                    PreviewImageTotalColours = ImageUtils.CountUniqueColors(_imagePreview);
                }
                else
                {
                    _imagePreview = ImageUtils.CreateBlankImage(320, 240, Brushes.WhiteSmoke);
                    PreviewImageTotalColours = 0;
                }
            }
            catch (Exception ex)
            {
                _imagePreview = null;
            }

            return _imagePreview;
        }
    }

    private int _PreviewImageTotalColours;

    public int PreviewImageTotalColours
    {
        get => _PreviewImageTotalColours;
        set
        {
            this.RaiseAndSetIfChanged(ref _PreviewImageTotalColours, value);
            this.RaisePropertyChanged(nameof(PreviewImageColoursText));
        }
    }

    public string PreviewImageColoursText => "Total Colours: " + PreviewImageTotalColours;

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

    public bool AutoHeight => Height == 241;

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
            RegisterOnOffFileBasename = value;
        }
    }

    private string _registerOnOffFileBasename = string.Empty;

    public string RegisterOnOffFileBasename
    {
        get => _registerOnOffFileBasename == string.Empty ? "Select a file" : _registerOnOffFileBasename;
        set
        {
            SetCanEditRegisterFile(value);
            value = Path.GetFileName(value);
            this.RaiseAndSetIfChanged(ref _registerOnOffFileBasename, value);
        }
    }

    private bool _canEditRegisterFile;

    public bool CanEditRegisterFile
    {
        get => _canEditRegisterFile;
        set => this.RaiseAndSetIfChanged(ref _canEditRegisterFile, value);
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

    private int _numberOfThreads = 1;

    public int NumberOfThreads
    {
        get => _numberOfThreads;
        set => this.RaiseAndSetIfChanged(ref _numberOfThreads, value);
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
    public bool IsSingleFrameMode => !DualFrameMode;

    private bool _dualFrameMode;

    public bool DualFrameMode
    {
        get => _dualFrameMode;
        set
        {
            if (DualFrameMode != value)
            {
                _dualFrameMode = value;
                SourceImageMaskPath = null;
                ImagePreviewPath = null;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(IsSingleFrameMode));
                this.RaisePropertyChanged(nameof(IsDualModeExpanded));
                this.RaisePropertyChanged(nameof(CanPreview));
                this.RaisePropertyChanged(nameof(CanGenerateMCH));
            }
        }
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

    private decimal _unstuckDrift;

    public decimal UnstuckDrift
    {
        get => _unstuckDrift;
        set => this.RaiseAndSetIfChanged(ref _unstuckDrift, value);
    }

    private int _unstuckAfter;

    public int UnstuckAfter
    {
        get => _unstuckAfter;
        set => this.RaiseAndSetIfChanged(ref _unstuckAfter, value);
    }

    private string _PreviewHeaderTitle = "Preview";

    public string PreviewHeaderTitle
    {
        get => _PreviewHeaderTitle;
        set => this.RaiseAndSetIfChanged(ref _PreviewHeaderTitle, value);
    }


    // Current Conversion ?
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    private void SetCanEditRegisterFile(string value)
    {
        CanEditRegisterFile = value != string.Empty;
    }

    private bool CopySourceImageToDestination()
    {
        bool ret = SourceImageDirectory != DestinationDirectory &&
                   FileUtils.CopyFile(SourceImagePath, DestinationImageFileName);


        // Old log entries not relevant to new image, so dont show on new image!
        SetLogEntriesAsDontShowOnImage();
            
        Statuses?.AddEntry(
            DateTime.Now,
            string.IsNullOrEmpty(_sourceImagePath)
                ? ConversionStatus.SourceCleared
                : ConversionStatus.SourceAdded,
            _sourceImagePath ?? ""
        );

        return ret;
    }

    private void SetLogEntriesAsDontShowOnImage()
    {
        if (Statuses != null)
        {
            foreach (var entry in Statuses)
            {
                entry.ShowOnImageStatusLine = false;
            }            
        }
    }

    public RastaConversion(string title)
    {
        PopulateDefaultValues();
        Title = title;

        Statuses = new BoundedLogCollection<StatusEntry>(100);
    }

    public void PopulateDefaultValues()
    {
        Title = "New Conversion";
        NumberOfThreads = RastaConverterDefaultValues.DefaultThreads;
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
        Optimiser = RastaConverterDefaultValues.DefaultOptimiser;
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
        ImagePreviewPath = String.Empty;
        DestinationFilePath = RastaConverterDefaultValues.DefaultDestinationFilePath;
        RegisterOnOffFilePath = RastaConverterDefaultValues.DefaultRegisterOnOffFilePath;
        UnstuckAfter = RastaConverterDefaultValues.DefaultUnstuckAfter;
        UnstuckDrift = RastaConverterDefaultValues.DefaultUnstuckDrift;
        Statuses?.Clear();
    }
}