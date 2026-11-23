using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RailwayErp.MvvmDemo.Models;
using RailwayErp.MvvmDemo.Services;

namespace RailwayErp.MvvmDemo.ViewModels;

public partial class TrainRouteListViewModel : ObservableObject
{
    private readonly ITrainRouteRepository _repository;
    private readonly ISyncQueue _queue;
    private readonly ISyncService _sync;
    private readonly INetworkService _network;
    private readonly IAppStateService _state;

    public TrainRouteListViewModel(ITrainRouteRepository repository, ISyncQueue queue,
        ISyncService sync, INetworkService network, IAppStateService state)
    {
        _repository = repository;
        _queue = queue;
        _sync = sync;
        _network = network;
        _state = state;

        _isOffline = network.SimulateOffline;
        _queue.QueueChanged += async (_, _) => await MainThread.InvokeOnMainThreadAsync(UpdateQueueAsync);
        _network.StatusChanged += (_, _) => MainThread.BeginInvokeOnMainThread(UpdateNetworkText);
        UpdateNetworkText();
    }

    public ObservableCollection<TrainRoute> Routes { get; } = new();

    [ObservableProperty] private bool _isOffline;
    [ObservableProperty] private string _networkText = string.Empty;
    [ObservableProperty] private int _queueCount;
    [ObservableProperty] private string _syncMessage = string.Empty;
    [ObservableProperty] private bool _isEmpty;

    partial void OnIsOfflineChanged(bool value)
    {
        _network.SimulateOffline = value;
        UpdateNetworkText();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        Routes.Clear();
        foreach (var route in await _repository.GetAllAsync())
            Routes.Add(route);
        IsEmpty = Routes.Count == 0;
        await UpdateQueueAsync();
    }

    [RelayCommand]
    private Task AddAsync() => NavigateAsync("routeedit");

    [RelayCommand]
    private Task EditAsync(TrainRoute? route) =>
        route is null ? Task.CompletedTask : NavigateAsync($"routeedit?id={route.Id}");

    private async Task NavigateAsync(string route)
    {
        try
        {
            _state.Log($"Навігація: {route}");
            await Shell.Current.GoToAsync(route);
            _state.Log($"Навігація завершена: {Shell.Current.CurrentState.Location}");
        }
        catch (Exception ex)
        {
            _state.Log($"Помилка навігації: {ex.GetType().Name}: {ex.Message} {ex.InnerException?.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(TrainRoute? route)
    {
        if (route is null) return;
        await _repository.DeleteAsync(route);
        Routes.Remove(route);
        _state.Log($"Видалено маршрут {route.Name}");
        if (_network.IsOnline)
            SyncMessage = (await _sync.SyncAsync()).Message;
    }

    [RelayCommand]
    private async Task SyncAsync()
    {
        var result = await _sync.SyncAsync();
        SyncMessage = result.Message;
        await UpdateQueueAsync();
    }

    private async Task UpdateQueueAsync() => QueueCount = (await _queue.GetPendingAsync()).Count;

    private void UpdateNetworkText() =>
        NetworkText = _network.IsOnline ? "Мережа: онлайн" : "Мережа: офлайн";
}
