using AdRasta2.Enums;

namespace AdRasta2.Models;

public struct ColourBiasResult
{
    public ColourBias Bias { get; set; }
    public double Confidence { get; set; } // 0–100%
}
