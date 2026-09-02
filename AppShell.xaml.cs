namespace RentManage;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register detail routes so they don't clog up the bottom TabBar
        Routing.RegisterRoute(nameof(UnitsPage), typeof(UnitsPage));
        Routing.RegisterRoute(nameof(TenantsPage), typeof(TenantsPage));
    }
}