# Лаб4

SQLite (sqlite-net-pcl), репозиторій, SyncQueue, синхронізація з RailwayErp.StubApi, AES-шифрування поля.

Запуск (Windows 10/11, .NET 8 SDK з робочим навантаженням maui-windows):

```
cd RailwayErp.MvvmDemo
dotnet build -c Debug
dotnet run -f net8.0-windows10.0.19041.0
```

Спочатку запустити заглушковий сервер синхронізації:

```
dotnet run --project RailwayErp.StubApi
```
Сервер слухає http://localhost:5085, застосунок відправляє туди чергу змін.
