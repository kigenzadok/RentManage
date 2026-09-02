using RentManage.Services;

namespace RentManage;

public partial class MainPage : ContentPage
{
    private readonly LocalDbService _dbService;

    public MainPage(LocalDbService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDashboardMetricsAsync();
    }

    private async Task LoadDashboardMetricsAsync()
    {
        int totalProps = await _dbService.GetTotalPropertiesCountAsync();
        int totalUnits = await _dbService.GetTotalUnitsCountAsync();
        int occupiedUnits = await _dbService.GetOccupiedUnitsCountAsync();
        decimal totalRevenue = await _dbService.GetTotalMonthlyRevenueAsync();

        int vacantUnits = totalUnits - occupiedUnits;
        double occupancyRate = totalUnits > 0 ? ((double)occupiedUnits / totalUnits) * 100 : 0;

        // Update Labels
        TotalPropertiesLabel.Text = totalProps.ToString();
        TotalUnitsLabel.Text = totalUnits.ToString();
        VacantUnitsLabel.Text = vacantUnits.ToString();
        OccupancyRateLabel.Text = $"{occupancyRate:F0}%";
        TotalRevenueLabel.Text = totalRevenue.ToString("C");
    }

    private async void OnNavToTenantsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("TenantsPage");
    }

    private async void OnNavToPropertiesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PropertiesPage");
    }
    private async void OnRegisterTenantClicked(object sender, EventArgs e)
    {
        // Relative pathing pushes TenantsPage on top of Dashboard without affecting tabs
        await Shell.Current.GoToAsync($"{nameof(TenantsPage)}?PropertyId=0");
    }
}