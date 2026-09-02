using RentManage.Models;
using RentManage.Services;

namespace RentManage;

[QueryProperty(nameof(PropertyId), "PropertyId")]
[QueryProperty(nameof(PropertyName), "PropertyName")]
public partial class TenantsPage : ContentPage
{
    private readonly LocalDbService _dbService;

    public int PropertyId { get; set; }

    private string _propertyName;
    public string PropertyName
    {
        get => _propertyName;
        set
        {
            _propertyName = Uri.UnescapeDataString(value ?? string.Empty);
            OnPropertyChanged();
        }
    }

    public TenantsPage(LocalDbService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!string.IsNullOrEmpty(PropertyName))
        {
            Title = $"Tenants - {PropertyName}";
        }

        await LoadFilteredTenantsAsync();
    }

    private async Task LoadFilteredTenantsAsync()
    {
        var allTenants = await _dbService.GetTenantsAsync();

        if (PropertyId > 0)
        {
            // Filter tenants assigned to units of this specific property
            var propertyUnits = (await _dbService.GetUnitsAsync())
                .Where(u => u.PropertyId == PropertyId)
                .Select(u => u.UnitNumber)
                .ToHashSet();

            TenantsListView.ItemsSource = allTenants
                .Where(t => propertyUnits.Contains(t.UnitNumber))
                .ToList();
        }
        else
        {
            TenantsListView.ItemsSource = allTenants;
        }
    }
    public async void OnRegisterTenantClicked(object sender, EventArgs e)
    {
        // Add your save/register tenant logic here
        await DisplayAlert("Success", "Tenant registered successfully.", "OK");
        AddFormCard.IsVisible = false;
    }
    private async void OnFabClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AddEditItemPage(_dbService, "Tenant"));
    }
    public void OnToggleAddFormClicked(object sender, EventArgs e)
    {
        AddFormCard.IsVisible = !AddFormCard.IsVisible;
    }
}