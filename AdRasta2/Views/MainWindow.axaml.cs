using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AdRasta2.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Code for restricing resize etc - Do we need !?
        // this.GetObservable(Window.WindowStateProperty).Subscribe(state =>
        // {
        //     this.GetObservable(Window.ClientSizeProperty)
        //         .Subscribe(size =>
        //         {
        //             this.Title = $"AdRasta2 {size.Width}x{size.Height}";
        //             //         var maxWidth = 1920;
        //             //         var maxHeight = 1080;
        //             //
        //             //         if (size.Width > maxWidth || size.Height > maxHeight)
        //             //         {
        //             //             this.Width = Math.Min(size.Width, maxWidth);
        //             //             this.Height = Math.Min(size.Height, maxHeight);
        //             //         }
        //         });

        // this.GetObservable(Window.WindowStateProperty).Subscribe(state =>
        // {
        //     if (state == WindowState.Maximized)
        //     {
        //         Dispatcher.UIThread.Post(() =>
        //         {
        //             if (Bounds.Width > MaxWidth || Bounds.Height > MaxHeight)
        //             {
        //                 WindowState = WindowState.Normal;
        //                 Width = MaxWidth;
        //                 Height = MaxHeight;
        //             }
        //         }, DispatcherPriority.Background);
        //     }
        // });

        //
        // if (state == WindowState.Maximized)
        // {
        //     WindowState = WindowState.Normal;
        //     Width = Math.Min(this.ClientSize.Width, MaxWidth);
        //     Height = Math.Min(this.ClientSize.Height, MaxHeight);
        // }
        //  });
    }
}