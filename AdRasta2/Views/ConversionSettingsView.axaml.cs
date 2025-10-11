using System.Reactive.Disposables;
using System.Reactive.Linq;
using AdRasta2.Extensions;
using AdRasta2.Models;
using AdRasta2.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace AdRasta2.Views;

public partial class ConversionSettingsView : UserControl
{
    public ConversionSettingsView()
    {
        InitializeComponent();
        ApplyDefaults();
    }

    private void ApplyDefaults()
    {
        NumSolutions.DefaultIfNull = RastaConverterDefaultValues.DefaultSolutionHistoryLength;
        MaxEvals.DefaultIfNull = RastaConverterDefaultValues.DefaultMaxEvaluations;
        UnstuckAfter.DefaultIfNull = RastaConverterDefaultValues.DefaultUnstuckAfter;
        UnstuckDrift.DefaultIfNull = RastaConverterDefaultValues.DefaultUnstuckDrift;
    }
}