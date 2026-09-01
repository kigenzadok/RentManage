using RentManage.Models;
using RentManage.Services;

namespace RentManage;

public partial class TenantsPage : ContentPage
{
    private readonly LocalDbService _dbService;

    public TenantsPage(LocalDbService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        // Load vacant units into Picker dropdown
        var vacantUnits = await _dbService.GetVacantUnitsAsync();
        VacantUnitsPicker.ItemsSource = vacantUnits;

        // Load existing tenants
        TenantsListView.ItemsSource = await _dbService.GetTenantsAsync();
    }

    private async void OnRegisterTenantClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FullNameEntry.Text) || string.IsNullOrWhiteSpace(PhoneEntry.Text))
        {
            await DisplayAlert("Error", "Please fill in Tenant Name and Phone Number.", "OK");
            return;
        }

        if (VacantUnitsPicker.SelectedItem is not Unit selectedUnit)
        {
            await DisplayAlert("Error", "Please select an available unit for this tenant.", "OK");
            return;
        }

        var tenant = new Tenant
        {
            FullName = FullNameEntry.Text,
            Phone = PhoneEntry.Text,
            Email = EmailEntry.Text
        };

        await _dbService.RegisterTenantAsync(tenant, selectedUnit.Id);

        // Clear Form & Refresh
        FullNameEntry.Text = string.Empty;
        PhoneEntry.Text = string.Empty;
        EmailEntry.Text = string.Empty;

        await DisplayAlert("Success", "Tenant registered and unit set to Occupied!", "OK");
        await LoadDataAsync();
    }
}