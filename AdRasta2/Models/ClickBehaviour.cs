using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;


namespace AdRasta2.Models;

public class ClickBehavior : Behavior<Border>
{
    private DateTime _lastClickTime = DateTime.MinValue;
    private CancellationTokenSource _clickCts;
    private const int DoubleClickThresholdMs = 300;
    private int _clickCount = 0;
    
    public static readonly AvaloniaProperty SingleClickCommandProperty =
        AvaloniaProperty.Register<ClickBehavior, ICommand>(nameof(SingleClickCommand));

    public static readonly AvaloniaProperty DoubleClickCommandProperty =
        AvaloniaProperty.Register<ClickBehavior, ICommand>(nameof(DoubleClickCommand));

    public ICommand SingleClickCommand
    {
        get => (ICommand)GetValue(SingleClickCommandProperty);
        set => SetValue(SingleClickCommandProperty, value);
    }

    public ICommand DoubleClickCommand
    {
        get => (ICommand)GetValue(DoubleClickCommandProperty);
        set => SetValue(DoubleClickCommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PointerPressed += OnPointerPressed;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PointerPressed -= OnPointerPressed;
    }

    private async void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        // Only respond to left-button presses
        var point = e.GetCurrentPoint(AssociatedObject);
        if (!point.Properties.IsLeftButtonPressed)
            return;
        
        _clickCount++;
        _clickCts?.Cancel();

        if (_clickCount == 1)
        {
            _clickCts = new CancellationTokenSource();
            var token = _clickCts.Token;

            try
            {
                await Task.Delay(DoubleClickThresholdMs, token);
                if (!token.IsCancellationRequested)
                {
                    SingleClickCommand?.Execute(null);
                    _clickCount = 0;
                }
            }
            catch (TaskCanceledException)
            {
                // Gesture was upgraded
            }
        }
        else if (_clickCount == 2)
        {
            DoubleClickCommand?.Execute(null);
            _clickCount = 0;
        }
    }
}