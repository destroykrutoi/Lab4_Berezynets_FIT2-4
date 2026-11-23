using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RailwayErp.MvvmDemo.Models;
using RailwayErp.MvvmDemo.Services;

namespace RailwayErp.MvvmDemo.ViewModels;

// Додавання та редагування маршруту
public partial class RouteEditViewModel : ObservableObject, IQueryAttributable
{
    private readonly ITrainRouteRepository _repository;
    private readonly ISyncService _sync;
    private readonly INetworkService _network;
    private TrainRoute _route = new();

    public RouteEditViewModel(ITrainRouteRepository repository, ISyncService sync, INetworkService network)
    {
        _repository = repository;
        _sync = sync;
        _network = network;
    }

    [ObservableProperty] private string _header = "Новий маршрут";
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _origin = string.Empty;
    [ObservableProperty] private string _destination = string.Empty;
    [ObservableProperty] private string _departure = string.Empty;
    [ObservableProperty] private string _chiefDocumentNumber = string.Empty;
    [ObservableProperty] private string _error = string.Empty;

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value) && int.TryParse(value?.ToString(), out var id))
        {
            var route = await _repository.GetByIdAsync(id);
            if (route is null)
            {
                Error = $"Маршрут з Id = {id} не знайдено";
                return;
            }
            _route = route;
            Header = $"Редагування маршруту (Id = {id})";
            Name = route.Name;
            Origin = route.Origin;
            Destination = route.Destination;
            Departure = route.DepartureTime.ToString("HH:mm");
            ChiefDocumentNumber = route.ChiefDocumentNumber ?? string.Empty;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || !TimeSpan.TryParse(Departure, out var time))
        {
            Error = "Вкажіть назву поїзда та час відправлення у форматі ГГ:ХХ";
            return;
        }

        _route.Name = Name.Trim();
        _route.Origin = Origin.Trim();
        _route.Destination = Destination.Trim();
        _route.DepartureTime = DateTime.Today.Add(time);
        _route.ChiefDocumentNumber = ChiefDocumentNumber.Trim();

        await _repository.SaveAsync(_route);
        if (_network.IsOnline)
            await _sync.SyncAsync();

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private Task CancelAsync() => Shell.Current.GoToAsync("..");
}
