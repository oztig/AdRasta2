using System.Collections.ObjectModel;

namespace AdRasta2.Models;

public class BoundedLogCollection<T> : ObservableCollection<T>
{
    private readonly int _maxEntries;

    public BoundedLogCollection(int maxEntries)
    {
        _maxEntries = maxEntries;
    }

    public void AddEntry(T item)
    {
        if (Count >= _maxEntries)
        {
            RemoveAt(0); // Remove oldest
        }
        Add(item);
    }
}