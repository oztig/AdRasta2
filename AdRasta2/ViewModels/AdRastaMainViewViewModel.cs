using System;
using System.Collections.ObjectModel;
using System.Reactive;
using AdRasta2.Models;
using AdRasta2.Services;
using Avalonia.Controls;
using ReactiveUI;

namespace AdRasta2.ViewModels;

public class AdRastaMainViewViewModel : ReactiveObject
{
    private Window? _window;
    public string HeadingText { get; set; } = "Ad Rasta v2 - Alpha";

    private string _currentConversionTitle = "" ;
    public string CurrentConversionTitle
    {
        get => SelectedConversion?.Title;
        set => this.RaiseAndSetIfChanged(ref _currentConversionTitle, value);
    }

    public ObservableCollection<int> LeftSprockets { get; } = new();
    
    
    public ObservableCollection<RastaConversion> RastaConversions { get; }

    private RastaConversion? _selectedConversion;

    public RastaConversion? SelectedConversion
    {
        get => _selectedConversion;
        set => this.RaiseAndSetIfChanged(ref _selectedConversion, value);
    }

    public ReactiveCommand<RastaConversion, Unit> PanelClickedCommand { get; }

    public ReactiveCommand<Unit, Unit> NewConversionCommand;
    private int _panelCounter = 4;

    public AdRastaMainViewViewModel(Window window)
    {
        _window = window;
        PopulateSprockets();
        PanelClickedCommand = ReactiveCommand.Create<RastaConversion>(conversion => { ChangeSelected(conversion); });
        NewConversionCommand = ReactiveCommand.Create(AddNewConversion);

        // DEBUG - this wil be done via creatinga new conversion (Button or similar)
        RastaConversions = new ObservableCollection<RastaConversion>
        {
            new RastaConversion("Yellow Submarine", @"/home/nickp/Pictures/RC conversions/Yellow-submarine-seofholes.webp",@"/home/nickp/Pictures/Yellow-submarine-seofholes-mask.png"),
            new RastaConversion("2001 Intro - Ape ", @"/home/nickp/Pictures/2001-ape.jpg",null),
            new RastaConversion("2001 Monolith ", @"/home/nickp/Pictures/2001_monolith.jpg",null)
        };

        // Optional: set default selection
        ChangeSelected(RastaConversions[0]);
        // SelectedConversion = RastaConversions[0];
        // SetIsSelected(0);
    }
    
    public void SetWindow(Window window)
    {
        _window = window;
    }

    private void PopulateSprockets()
    {
        const int sprocketCount = 7;
        for (int i = 0; i < sprocketCount; i++)
        {
            // TopSprockets.Add(i);
            // BottomSprockets.Add(i);
            LeftSprockets.Add(i);
            // RightSprockets.Add(i);
        }
    }

    private void ChangeSelected(RastaConversion conversion)
    {
        var index = RastaConversions.IndexOf(conversion);
        SelectedConversion = conversion;
        CurrentConversionTitle = conversion.Title;
        
        SetIsSelected(index);

        // DEBUG
        Console.WriteLine($"Clicked item '{conversion.Title}' at index {index}");
    }

    private void SetIsSelected(int selectedIndex)
    {
        for (int i = 0; i < RastaConversions.Count; i++)
            RastaConversions[i].IsSelected = selectedIndex == i;
    }
    
    public async void AddNewConversion()
    {
        var userInput = await DialogService.ShowInputDialogAsync("New Conversion",
          "Name:",  "", "New Conversion Name", _window);
        if (!(userInput.confirmed ?? false))
            return;

        RastaConversions.Add(new RastaConversion(userInput.value.Trim(), "",""));
        
        ChangeSelected(RastaConversions[^1]);
    }
}