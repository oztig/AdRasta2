using System;
using System.Collections.ObjectModel;
using System.Reactive;
using AdRasta2.Models;
using Avalonia;
using Avalonia.Platform;
using Microsoft.VisualBasic;
using ReactiveUI;

namespace AdRasta2.ViewModels;

public class AdRastaMainViewViewModel : ReactiveObject
{
    public string HeadingText { get; set; } = "Ad Rasta v2 - Alpha";
    public ObservableCollection<RastaConversion> RastaConversions { get; }

    private RastaConversion? _selectedConversion;

    public RastaConversion? SelectedConversion
    {
        get => _selectedConversion;
        set => this.RaiseAndSetIfChanged(ref _selectedConversion, value);
    }

    public ReactiveCommand<RastaConversion, Unit> PanelClickedCommand { get; }
    public ReactiveCommand<Unit, Unit> MyClickCommand { get; }
    
    public ReactiveCommand<Unit, Unit> NewConversionCommand { get; }
    private int _panelCounter = 1;

    public AdRastaMainViewViewModel()
    {
        PanelClickedCommand = ReactiveCommand.Create<RastaConversion>(conversion => { ChangeSelected(conversion); });
        NewConversionCommand = ReactiveCommand.Create(AddNewConversion);

        // DEBUG - this wil be done via creatinga new conversion (Button or similar)
        RastaConversions = new ObservableCollection<RastaConversion>
        {
            new RastaConversion("3 Sheep", @"/home/nickp/Pictures/3sheep.jpeg"),
            new RastaConversion("2001 Intro - Ape ", @"/home/nickp/Pictures/2001-ape.jpg"),
            new RastaConversion("2001 Monolith ", @"/home/nickp/Pictures/2001_monolith.jpg")
        };

        MyClickCommand = ReactiveCommand.Create(() =>
        {
            // Handle the click here
        });

        // Optional: set default selection
        SelectedConversion = RastaConversions[0];
        SetIsSelected(0);
    }

    private void ChangeSelected(RastaConversion conversion)
    {
        var index = RastaConversions.IndexOf(conversion);
        SelectedConversion = conversion;
        SetIsSelected(index);

        Console.WriteLine($"Clicked item '{conversion.Title}' at index {index}");
    }

    private void SetIsSelected(int selectedIndex)
    {
        for (int i = 0; i < RastaConversions.Count; i++)
            RastaConversions[i].IsSelected = selectedIndex == i;
    }
    
    public void AddNewConversion()
    {
        var title = $"Panel {_panelCounter++}";
        var imagePath = "/home/nickp/Pictures/Glasses.jpg";
        RastaConversions.Add(new RastaConversion(title, imagePath));
    }
}