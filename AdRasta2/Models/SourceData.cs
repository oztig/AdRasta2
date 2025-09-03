using System.Collections.ObjectModel;

namespace AdRasta2.Models;

public class SourceData
{
    public ObservableCollection<string> AfterDualSteps { get; } = new ObservableCollection<string>();

    public void Populate()
    {
        PopulateAfterDualSteps();
    }

    private void PopulateAfterDualSteps()
    {
        AfterDualSteps.Clear();
        AfterDualSteps.Add("copy");
        AfterDualSteps.Add("generate");
    }

    public SourceData()
    {
        Populate();
    }
}