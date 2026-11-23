using System.Collections.ObjectModel;
using RailwayErp.MvvmDemo.Services;

namespace RailwayErp.MvvmDemo.ViewModels;

public class LogViewModel
{
    public LogViewModel(IAppStateService state) => Entries = state.LogEntries;

    public ObservableCollection<string> Entries { get; }
}
