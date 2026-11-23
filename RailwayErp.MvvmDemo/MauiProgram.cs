using Microsoft.Extensions.Logging;
using RailwayErp.MvvmDemo.Data;
using RailwayErp.MvvmDemo.Services;
using RailwayErp.MvvmDemo.ViewModels;
using RailwayErp.MvvmDemo.Views;

namespace RailwayErp.MvvmDemo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // стан застосунку, тема, deep link (з лабораторної роботи № 3)
        builder.Services.AddSingleton<IAppStateService, AppStateService>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddSingleton<IDeepLinkService, DeepLinkService>();

        // локальна БД, репозиторій, шифрування, черга та синхронізація
        builder.Services.AddSingleton<AppDatabase>();
        builder.Services.AddSingleton<ICryptoService, AesCryptoService>();
        builder.Services.AddSingleton<ISyncQueue, SyncQueue>();
        builder.Services.AddSingleton<INetworkService, NetworkService>();
        builder.Services.AddSingleton<ISyncService, SyncService>();
        builder.Services.AddSingleton<ITrainRouteRepository, TrainRouteRepository>();

        builder.Services.AddTransient<TrainRouteListViewModel>();
        builder.Services.AddTransient<RouteEditViewModel>();
        builder.Services.AddTransient<DatabaseViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<LogViewModel>();

        builder.Services.AddTransient<TrainRouteListPage>();
        builder.Services.AddTransient<RouteEditPage>();
        builder.Services.AddTransient<DatabasePage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<LogPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
