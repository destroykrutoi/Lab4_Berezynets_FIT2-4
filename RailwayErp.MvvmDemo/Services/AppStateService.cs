using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RailwayErp.MvvmDemo.Services;

public interface IAppStateService
{
    DateTime SessionStartedAt { get; }
    bool IsInBackground { get; }
    string CurrentPage { get; set; }
    int ResumeCount { get; }
    ObservableCollection<string> LogEntries { get; }
    string LogFilePath { get; }

    event EventHandler? AppStopped;
    event EventHandler? AppResumed;

    void Log(string message);
    void NotifyStopped();
    void NotifyResumed();
}

// Сервіс стану сесії: реєструється як Singleton, тому живе весь час роботи застосунку
public class AppStateService : IAppStateService
{
    private readonly object _sync = new();

    public AppStateService()
    {
        SessionStartedAt = DateTime.Now;
        LogFilePath = Path.Combine(FileSystem.AppDataDirectory, "lifecycle.log");
    }

    public DateTime SessionStartedAt { get; }
    public bool IsInBackground { get; private set; }
    public string CurrentPage { get; set; } = string.Empty;
    public int ResumeCount { get; private set; }
    public ObservableCollection<string> LogEntries { get; } = new();
    public string LogFilePath { get; }

    public event EventHandler? AppStopped;
    public event EventHandler? AppResumed;

    public void Log(string message)
    {
        var line = $"{DateTime.Now:HH:mm:ss.fff}  {message}";
        Debug.WriteLine($"[Lifecycle] {line}");
        lock (_sync)
        {
            File.AppendAllText(LogFilePath, line + Environment.NewLine);
        }
        MainThread.BeginInvokeOnMainThread(() => LogEntries.Insert(0, line));
    }

    public void NotifyStopped()
    {
        IsInBackground = true;
        AppStopped?.Invoke(this, EventArgs.Empty);
    }

    public void NotifyResumed()
    {
        IsInBackground = false;
        ResumeCount++;
        AppResumed?.Invoke(this, EventArgs.Empty);
    }
}
