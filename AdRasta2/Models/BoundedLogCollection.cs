using System;
using System.Collections.ObjectModel;
using AdRasta2.Enums;

namespace AdRasta2.Models;

public class BoundedLogCollection<T> : ObservableCollection<T>
{
    private readonly int _maxEntries;

    public BoundedLogCollection()
    {
        _maxEntries = Settings.MaxLogEntries;
    }

    public void AddEntry(DateTime timeStamp, ConversionStatus status, string details)
    {
        StatusEntry newEntry = new StatusEntry()
        {
            Timestamp = timeStamp,
            Status = status,
            Details = details,
            ShowOnImageStatusLine = StatusFilter.ShouldIncludeOnImageDotLine(status)
        };
        AddEntry((T)(object)newEntry);
    }

    public void AddEntry(T item)
    {
        if (item is StatusEntry statusEntry)
            statusEntry.ShowOnImageStatusLine = StatusFilter.ShouldIncludeOnImageDotLine(statusEntry.Status);

        if (Count >= Settings.MaxLogEntries)
            RemoveAt(0); // Remove oldest

        Add(item);
    }
}