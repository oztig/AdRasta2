using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;

namespace AdRasta2;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            var logPath = Path.Combine(AppContext.BaseDirectory, "AdRasta2_crashlog.txt");
            try
            {
                File.WriteAllText(logPath, $"Startup exception logged at {DateTime.Now}:\n{ex}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .With(new Win32PlatformOptions
                { DpiAwareness = Win32DpiAwareness.PerMonitorDpiAware })
            .UseReactiveUI();
}