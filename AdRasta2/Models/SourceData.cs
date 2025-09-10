using System.Collections.ObjectModel;

namespace AdRasta2.Models;

public class SourceData
{
    public ObservableCollection<string> AfterDualSteps { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> DualBlending { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> ResizeFilters { get; } = new ObservableCollection<string>();
    

    public void Populate()
    {
        PopulateAfterDualSteps();
        PopulateDualBlending();
        PopulateResizeFilters();
    }

    private void PopulateAfterDualSteps()
    {
        AfterDualSteps.Clear();
        AfterDualSteps.Add("copy");
        AfterDualSteps.Add("generate");
    }

    private void PopulateDualBlending()
    {
        DualBlending.Clear();
        DualBlending.Add("yuv");
        DualBlending.Add("rgb");
    }
    
    private void PopulateResizeFilters()
    {
        ResizeFilters.Clear();
        ResizeFilters.Add("bicubic");
        ResizeFilters.Add("bilinear");
        ResizeFilters.Add("box");
        ResizeFilters.Add("bspline");
        ResizeFilters.Add("catmullrom");
        ResizeFilters.Add("lanczos");
    }

    public SourceData()
    {
        Populate();
    }
}