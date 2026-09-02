using RentManage.Models;
using RentManage.Services;

namespace RentManage;

public partial class PropertiesPage : ContentPage
{
    private readonly LocalDbService _dbService;

    public PropertiesPage(LocalDbService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPropertiesAsync();
    }

    private async Task LoadPropertiesAsync()
    {
        PropertiesListView.ItemsSource = await _dbService.GetPropertiesAsync();
    }

    // Opens the unified modal page set specifically to "Property" mode
    private async void OnToggleAddFormClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AddEditItemPage(_dbService, "Property"));
    }

    // Navigates to UnitsPage and passes the selected Property ID & Name
    private async void OnViewUnitsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Property selectedProperty)
        {
            await Shell.Current.GoToAsync($"{nameof(UnitsPage)}?PropertyId={selectedProperty.Id}&PropertyName={Uri.EscapeDataString(selectedProperty.Name)}");
        }
    }

    // Navigates to TenantsPage and passes the selected Property ID & Name
    private async void OnViewTenantsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Property selectedProperty)
        {
            await Shell.Current.GoToAsync($"{nameof(TenantsPage)}?PropertyId={selectedProperty.Id}&PropertyName={Uri.EscapeDataString(selectedProperty.Name)}");
        }
    }
}