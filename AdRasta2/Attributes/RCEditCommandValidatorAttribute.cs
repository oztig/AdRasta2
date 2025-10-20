using System.ComponentModel.DataAnnotations;
using System.IO;
using AdRasta2.Models;
using AdRasta2.ViewModels;

namespace AdRasta2.Attributes;

public class RCEditCommandValidatorAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var viewModel = validationContext.ObjectInstance as SettingsEditorViewModel;
        if (viewModel == null) return ValidationResult.Success;

        if (!Settings.IsWindows || !viewModel.SetConversionIcon)
            return ValidationResult.Success;

        var path = value as string;
        if (string.IsNullOrWhiteSpace(path))
            return new ValidationResult("RCEDIT file location is required.");

        if (!File.Exists(path))
            return new ValidationResult("RCEDIT executable must exist.");

        return ValidationResult.Success;
    }
}
