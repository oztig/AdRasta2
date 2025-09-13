using System;
using System.Collections.ObjectModel;

namespace AdRasta2.Models;

public class SourceData
{
    public ObservableCollection<string> AfterDualSteps { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> DualBlending { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> ResizeFilters { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> ColourDistance { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> InitialState { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> AutoSavePeriods { get; } = new ObservableCollection<string>();
    public ObservableCollection<int> TotalThreads { get; } = new ObservableCollection<int>();
    public ObservableCollection<string> Palettes { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> PreColourDistance { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> Dithering { get; } = new ObservableCollection<string>();
    
    public void Populate()
    {
        PopulateAfterDualSteps();
        PopulateDualBlending();
        PopulateResizeFilters();
        PopulateColourDistance();
        PopulateInitialState();
        PopulateAutoSavePeriods();
        PopulateThreads();
        PopulatePalettes();
        PopulatePreColourDistance();
        PopulateDithering();
    }

    private void PopulateAfterDualSteps()
    {
        AfterDualSteps.Clear();
        AfterDualSteps.Add("copy");
        AfterDualSteps.Add("generate");
    }

    private void PopulateDualBlending()
    {
        DualBlending.Clear();
        DualBlending.Add("yuv");
        DualBlending.Add("rgb");
    }
    
    private void PopulateResizeFilters()
    {
        ResizeFilters.Clear();
        ResizeFilters.Add("bicubic");
        ResizeFilters.Add("bilinear");
        ResizeFilters.Add("box");
        ResizeFilters.Add("bspline");
        ResizeFilters.Add("catmullrom");
        ResizeFilters.Add("lanczos");
    }
    
    private void PopulateColourDistance()
    {
        ColourDistance.Clear();
        ColourDistance.Add("cie94");
        ColourDistance.Add("ciede");
        ColourDistance.Add("euclid");
        ColourDistance.Add("yuv");
    }
    
    private void PopulateInitialState()
    {
        InitialState.Clear();
        InitialState.Add("empty");
        InitialState.Add("less");
        InitialState.Add("random");
        InitialState.Add("smart");
    }
    
    private void PopulateAutoSavePeriods()
    {
        AutoSavePeriods.Clear();
        AutoSavePeriods.Add("auto");
        AutoSavePeriods.Add("0");
        AutoSavePeriods.Add("1000");
        AutoSavePeriods.Add("10000");
        AutoSavePeriods.Add("50000");
        AutoSavePeriods.Add("100000");
        AutoSavePeriods.Add("500000");
        AutoSavePeriods.Add("1000000");
        AutoSavePeriods.Add("5000000");
        AutoSavePeriods.Add("10000000");
    }
    
    private void PopulateThreads()
    {
        TotalThreads.Clear();
        for (int i = 1; i <= Environment.ProcessorCount; i++)
        {
            TotalThreads.Add(i);
        }
    }
    
    private void PopulatePalettes()
    {
        Palettes.Clear();
        Palettes.Add("altirra");
        Palettes.Add("altirra_old");
        Palettes.Add("g2f");
        Palettes.Add("jakub");
        Palettes.Add("laoo");
        Palettes.Add("ntsc");
        Palettes.Add("oliverp");
        Palettes.Add("real");
    }
    
    private void PopulatePreColourDistance()
    {
        PreColourDistance.Clear();
        PreColourDistance.Add("ciede");
        PreColourDistance.Add("cie94");
        PreColourDistance.Add("euclid");
        PreColourDistance.Add("yuv");
    }
    
    private void PopulateDithering()
    {
        Dithering.Clear();
        Dithering.Add("2d");
        Dithering.Add("chess");
        Dithering.Add("floyd");
        Dithering.Add("jarvis");
        Dithering.Add("knoll");
        Dithering.Add("line");
        Dithering.Add("line2");
        Dithering.Add("none");
    }

    public SourceData()
    {
        Populate();
    }
}