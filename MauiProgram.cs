using Microsoft.Extensions.Logging;
using RentManage.Services;
using RentManage;

namespace RentManage;

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

        // Register Database Service
        builder.Services.AddSingleton<LocalDbService>();

        // Register Pages
        builder.Services.AddTransient<MainPage>();

        builder.Services.AddSingleton<LocalDbService>();

        // Register Pages
        builder.Services.AddTransient<TenantsPage>();
        // (Add PropertiesPage and UnitsPage as you create their XAML files)
        builder.Services.AddSingleton<LocalDbService>();

        // Register Pages
        builder.Services.AddTransient<PropertiesPage>();
        builder.Services.AddTransient<UnitsPage>();
        builder.Services.AddTransient<TenantsPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<PaymentsPage>();
        builder.Services.AddSingleton<ReceiptService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}