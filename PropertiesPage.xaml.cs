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

    private async void OnSavePropertyClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PropertyNameEntry.Text))
        {
            await DisplayAlert("Error", "Property Name is required.", "OK");
            return;
        }

        var property = new Property
        {
            Name = PropertyNameEntry.Text,
            Address = PropertyAddressEntry.Text
        };

        await _dbService.SavePropertyAsync(property);

        PropertyNameEntry.Text = string.Empty;
        PropertyAddressEntry.Text = string.Empty;

        await LoadPropertiesAsync();
    }
}