using AdRasta2.Enums;

namespace AdRasta2.Models;

public class ProcessRunResult
{
    public RastaConversion Conversion { get; init; }
    public AdRastaStatus Status { get; init; }
    public string? StandardOutput { get; init; }
    public string? StandardError { get; init; }
    public int ExitCode { get; init; }

    public bool IsSuccess => Status == AdRastaStatus.Success && ExitCode == 0;

    public string GetSummary()
    {
        return $"ExitCode: {ExitCode}\nStatus: {Status}\nStdOut:\n{StandardOutput}\nStdErr:\n{StandardError}";
    }
}
