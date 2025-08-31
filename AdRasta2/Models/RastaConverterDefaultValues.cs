namespace AdRasta2.Models;

public class RastaConverterDefaultValues
{
    public static int DefaultThreads { get; } = 1;
    public static int DefaultMaxEvaluations { get; } = 0;
    public static string DefaultAutoSavePeriod { get; } = "auto";
    public static decimal DefaultRandomSeed { get; } = 0;
    public static int DefaultHeight { get; } = 240;
    public static string DefaultPalette { get; } = "laoo";
    public static string DefaultDithering { get; } = "none";
    public static int DefaultDitheringStrength { get; } = 1;
    public static int DefaultDitheringRandomness { get; } = 0;
    public static decimal DefaultMaskStrength { get; } = (decimal)1.0;
    public static string DefaultResizeFilter { get; } = "box";
    public static int DefaultBrightness { get; } = 0;
    public static int DefaultContrast { get; } = 0;
    public static decimal DefaultGamma { get; } = (decimal)1.0;
    public static string DefaultInitialState { get; } = "random";
    public static int DefaultSolutionHistoryLength { get; } = 1;
    public static string DefultOptimiser { get; } = "dlas";
    public static string DefaultColourDistance { get; } = "yuv";
    public static string DefaultPreColourDistance { get; } = "ciede";
    public static int DefaultCacheInMB { get; } = 64;
    public static bool DefaultDualFrameMode { get; } = false;
    public static int DefualtFirstDualSteps { get; } = 100000;
    public static string DefaultAfterDualSteps { get; } = "copy";
    public static int DefaultAlternatingDualSteps { get; } = 50000;
    public static string DefaultDualBlending { get; } = "yuv";
    public static decimal DefaultDualLuma { get; } = (decimal)0.2;
    public static decimal DefaultDualChroma { get; } = (decimal)0.1;
    public static string DefaultSourceImagePath = string.Empty;
    public static string DefaultSourceImageMaskPath { get; } = string.Empty;
    public static string DefaultDestinationFilePath { get; } = string.Empty;
    public static string DefaultDistantionFilePrefix { get; } = "output";
    public static string DefaultRegisterOnOffFilePath { get; } = string.Empty;
}