using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdRasta2.Models;

public class RastaConverter
{
    public async Task<IReadOnlyList<string>> GenerateRastaArguments(RastaConversion rastaConversion)
    {
        var args = new List<string>();

        // if (!AutoHeight)
        //     args.Add($"/h={Height}");
        //
        // if (SelectedResizeFilter != defaultValues.defaultSelectedResizeFilter)
        //     args.Add($"/filter={SelectedResizeFilter}");
        //
        // if (_selectedPreColourDistance != defaultValues.defaultSelectedPreColourDistance)
        //     args.Add($"/predistance={SelectedPreColourDistance}");
        //
        // if (_selectedDithering != defaultValues.defaultSelectedDithering)
        //     args.Add($"/dither={SelectedDithering}");
        //
        // if (SelectedDithering != "none")
        // {
        //     if (DitheringStrength != defaultValues.defaultDitheringStrength)
        //         args.Add($"/dither_val={DitheringStrength}");
        //
        //     if (DitheringRandomness != defaultValues.defaultDitheringRandomness)
        //         args.Add($"/dither_rand={DitheringRandomness}");
        // }
        //
        // if (Brightness != defaultValues.defaultBrightness)
        //     args.Add($"/brightness={Brightness}");
        //
        // if (Contrast != defaultValues.defaultContrast)
        //     args.Add($"/contrast={Contrast}");
        //
        // if (Gamma != defaultValues.defaultGamma)
        //     args.Add($"/gamma={Gamma}");
        //
        // if (!string.IsNullOrWhiteSpace(MaskFilePath))
        // {
        //     args.Add($"/details={MaskFilePath}");
        //
        //     if (MaskStrength != defaultValues.defaultMaskStrength)
        //         args.Add($"/details_val={MaskStrength}");
        // }
        //
        // if (!string.IsNullOrWhiteSpace(RegisterOnOffFilePath))
        //     args.Add($"/onoff={RegisterOnOffFilePath}");
        //
        // if (SelectedColourDistance != defaultValues.defaultSelectedColourDistance)
        //     args.Add($"/distance={SelectedColourDistance}");
        //
        // if (SelectedInitialState != defaultValues.defaultSelectedInitialState)
        //     args.Add($"/init={SelectedInitialState}");
        //
        // if (NumberOfSolutions != defaultValues.defaultNumberOfSolutions)
        //     args.Add($"/s={NumberOfSolutions}");
        //
        // if (SelectedAutoSavePeriod != defaultValues.defaultSelectedAutoSavePeriod)
        //     args.Add($"/save={SelectedAutoSavePeriod}");
        //
        // args.Add($"/threads={SelectedThread}");
        //
        // if (isPreview)
        //     args.Add("/preprocess");
        //
        // if (isContinue)
        //     args.Add("/continue");
        //
        // args.Add($"/i={SourceFilePath}");
        // args.Add($"/o={FullDestinationFileName}");
        // args.Add($"/pal={Path.Combine(_settings.PaletteDirectory, SelectedPalette.Trim() + ".act")}");
        //
        // RastConverterFullCommandLine = await RastaConverter.GenerateFullCommandLineString(_settings, args);

        return args;
    }
}