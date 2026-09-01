using RentManage.Models;
using RentManage.Services;

namespace RentManage;

public partial class UnitsPage : ContentPage
{
    private readonly LocalDbService _dbService;

    public UnitsPage(LocalDbService dbService)
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
        PropertyPicker.ItemsSource = await _dbService.GetPropertiesAsync();

        // Load vacant and occupied units
        var vacant = await _dbService.GetVacantUnitsAsync();
        UnitsListView.ItemsSource = vacant;
    }

    private async void OnSaveUnitClicked(object sender, EventArgs e)
    {
        if (PropertyPicker.SelectedItem is not Property selectedProperty)
        {
            await DisplayAlert("Error", "Please select a property.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(UnitNumberEntry.Text))
        {
            await DisplayAlert("Error", "Unit Number is required.", "OK");
            return;
        }

        decimal.TryParse(MonthlyRentEntry.Text, out decimal rent);

        var unit = new Unit
        {
            PropertyId = selectedProperty.Id,
            UnitNumber = UnitNumberEntry.Text,
            MonthlyRent = rent,
            Status = "Vacant"
        };

        await _dbService.SaveUnitAsync(unit);

        UnitNumberEntry.Text = string.Empty;
        MonthlyRentEntry.Text = string.Empty;

        await LoadDataAsync();
    }
}