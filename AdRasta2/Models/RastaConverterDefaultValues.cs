using System.IO;

namespace AdRasta2.Models;

public class RastaConverterDefaultValues
{
    public static bool LegacyMode = true;
    public static int DefaultThreads = 1;
    public static int DefaultMaxEvaluations = 0;
    public static string DefaultAutoSavePeriod = "auto";
    public static decimal DefaultRandomSeed = 0;
    public static int DefaultHeight = 241;
    public static string DefaultPalette = "laoo";
    public static string DefaultDithering = "none";
    public static int DefaultDitheringStrength = 1;
    public static int DefaultDitheringRandomness = 0;
    public static decimal DefaultMaskStrength = (decimal)1.0;
    public static string DefaultResizeFilter = "box";
    public static int DefaultBrightness = 0;
    public static int DefaultContrast = 0;
    public static decimal DefaultGamma = (decimal)1.0;
    public static string DefaultInitialState = "random";
    public static int DefaultSolutionHistoryLength = 1;
    public static string DefaultOptimiser = "lahc";
    public static string DefaultColourDistance = "yuv";
    public static string DefaultPreColourDistance = "ciede";
    public static int DefaultCacheInMB = 64;
    public static bool DefaultDualFrameMode = false;
    public static int DefualtFirstDualSteps = 100000;
    public static string DefaultAfterDualSteps = "copy";
    public static int DefaultAlternatingDualSteps = 50000;
    public static string DefaultDualBlending = "yuv";
    public static decimal DefaultDualLuma = (decimal)0.2;
    public static decimal DefaultDualChroma = (decimal)0.1;
    public static string DefaultSourceImagePath = string.Empty;
    public static string DefaultSourceImageMaskPath = string.Empty;
    public static string DefaultDestinationFilePath = Directory.GetCurrentDirectory();
    public static string DefaultDestantionFilePrefix = "output";
    public static string DefaultDestintionName = "output.png-dst.png";
    public static string DefaultDualModeDestintionName = "out_dual_blended.png";
    public static string DefaultRegisterOnOffFilePath = string.Empty;
    public static decimal DefaultUnstuckDrift = (decimal)0.00001;
    public static int DefaultUnstuckAfter = 1000000;
}