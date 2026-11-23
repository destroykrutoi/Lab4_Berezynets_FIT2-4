using System.Text.Json;
using RailwayErp.MvvmDemo.Data;
using RailwayErp.MvvmDemo.Models;

namespace RailwayErp.MvvmDemo.Services;

public interface ITrainRouteRepository
{
    Task<List<TrainRoute>> GetAllAsync();
    Task<TrainRoute?> GetByIdAsync(int id);
    Task<int> SaveAsync(TrainRoute route);
    Task<int> DeleteAsync(TrainRoute route);
}

// Репозиторій: ViewModel не працює з SQLiteAsyncConnection напряму
public class TrainRouteRepository : ITrainRouteRepository
{
    private readonly AppDatabase _database;
    private readonly ICryptoService _crypto;
    private readonly ISyncQueue _queue;

    public TrainRouteRepository(AppDatabase database, ICryptoService crypto, ISyncQueue queue)
    {
        _database = database;
        _crypto = crypto;
        _queue = queue;
    }

    public async Task<List<TrainRoute>> GetAllAsync()
    {
        var db = await _database.GetConnectionAsync();
        var list = await db.Table<TrainRoute>().OrderBy(r => r.DepartureTime).ToListAsync();
        foreach (var r in list)
            r.ChiefDocumentNumber = await _crypto.DecryptAsync(r.ChiefDocumentNumber);
        return list;
    }

    public async Task<TrainRoute?> GetByIdAsync(int id)
    {
        var db = await _database.GetConnectionAsync();
        var route = await db.FindAsync<TrainRoute>(id);
        if (route is not null)
            route.ChiefDocumentNumber = await _crypto.DecryptAsync(route.ChiefDocumentNumber);
        return route;
    }

    public async Task<int> SaveAsync(TrainRoute route)
    {
        var db = await _database.GetConnectionAsync();
        var isNew = route.Id == 0;

        // запис у БД виконується з копії, де чутливе поле зашифроване
        var stored = Clone(route);
        stored.UpdatedAt = DateTime.UtcNow;
        stored.ChiefDocumentNumber = await _crypto.EncryptAsync(route.ChiefDocumentNumber);

        var result = isNew ? await db.InsertAsync(stored) : await db.UpdateAsync(stored);
        route.Id = stored.Id;
        route.UpdatedAt = stored.UpdatedAt;

        await _queue.EnqueueAsync(isNew ? "Insert" : "Update", stored);
        return result;
    }

    public async Task<int> DeleteAsync(TrainRoute route)
    {
        var db = await _database.GetConnectionAsync();
        var result = await db.DeleteAsync<TrainRoute>(route.Id);
        await _queue.EnqueueAsync("Delete", new TrainRoute { Id = route.Id, UpdatedAt = DateTime.UtcNow });
        return result;
    }

    private static TrainRoute Clone(TrainRoute r) =>
        JsonSerializer.Deserialize<TrainRoute>(JsonSerializer.Serialize(r))!;
}
