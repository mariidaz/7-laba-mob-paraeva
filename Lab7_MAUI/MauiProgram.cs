using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Lab7_MAUI.Data;
using Lab7_MAUI.Services;
using Lab7_MAUI.ViewModels;
using Lab7_MAUI.Views;

namespace Lab7_MAUI;

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

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Configure EF Core SQLite Database
        var appDataDir = FileSystem.AppDataDirectory;
        if (!Directory.Exists(appDataDir))
        {
            Directory.CreateDirectory(appDataDir);
        }
        var dbPath = Path.Combine(appDataDir, "komunalni_poslugi.db");
        builder.Services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Register Business Services
        builder.Services.AddSingleton<IUtilityDataService, UtilityDataService>();

        // Register ViewModels
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<TenantsViewModel>();
        builder.Services.AddTransient<ApartmentsViewModel>();
        builder.Services.AddTransient<ServicesViewModel>();
        builder.Services.AddTransient<BillsViewModel>();

        // Register Views
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<TenantsPage>();
        builder.Services.AddTransient<ApartmentsPage>();
        builder.Services.AddTransient<ServicesPage>();
        builder.Services.AddTransient<BillsPage>();

        return builder.Build();
    }
}
