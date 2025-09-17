using System;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AdRasta2.ViewModels;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AdRasta2.Views;

using Avalonia.Threading;

public partial class ConversionLogView : UserControl
{
    public ConversionLogView()
    {
        InitializeComponent();

        this.DataContextChanged += (_, _) =>
        {
            if (DataContext is AdRastaMainViewViewModel vm &&
                vm.SelectedConversion is not null)
            {
                vm.SelectedConversion.ScrollToLatestLogEntry = () =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (LogScrollViewer is { })
                        {
                            LogScrollViewer.Offset = new Avalonia.Vector(
                                LogScrollViewer.Offset.X,
                                LogScrollViewer.Extent.Height
                            );
                        }
                    }, DispatcherPriority.Background);
                };
            }
        };
    }
}

// Console.WriteLine("ScrollToLatestLogEntry invoked");
// Dispatcher.UIThread.Post(() =>
// {
//     Console.WriteLine("Inspecting visual tree...");
//
//     foreach (var descendant in LogListBox.GetVisualDescendants())
//     {
//         Console.WriteLine($"Descendant: {descendant.GetType().Name}");
//
//         if (descendant is ScrollViewer sv)
//         {
//             Console.WriteLine($"ScrollViewer found. Extent: {sv.Extent}, Offset: {sv.Offset}");
//             sv.Offset = new Avalonia.Vector(sv.Offset.X, sv.Extent.Height);
//         }
//     }
// }, DispatcherPriority.Background);
// };