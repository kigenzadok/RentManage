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

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (!string.IsNullOrEmpty(PropertyName))
        {
            Title = $"Tenants - {PropertyName}";
        }

        // Load data after navigation parameters (PropertyId) are guaranteed to be bound
        await LoadUnitsAsync();
        await LoadFilteredTenantsAsync();
    }

    private async Task LoadUnitsAsync()
    {
        var allUnits = await _dbService.GetUnitsAsync();

        List<Unit> availableUnits;

        if (PropertyId > 0)
        {
            // Case-insensitive status check and filter by PropertyId
            availableUnits = allUnits
                .Where(u => u.PropertyId == PropertyId &&
                           (string.IsNullOrWhiteSpace(u.Status) ||
                            u.Status.Equals("Vacant", StringComparison.OrdinalIgnoreCase) ||
                            u.Status.Equals("Available", StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
        else
        {
            availableUnits = allUnits
                .Where(u => string.IsNullOrWhiteSpace(u.Status) ||
                            u.Status.Equals("Vacant", StringComparison.OrdinalIgnoreCase) ||
                            u.Status.Equals("Available", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Assign to picker
        UnitPicker.ItemsSource = availableUnits;
    }

    private async Task LoadFilteredTenantsAsync()
    {
        var allTenants = await _dbService.GetTenantsAsync();

        if (PropertyId > 0)
        {
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
        if (UnitPicker.SelectedItem is not Unit selectedUnit)
        {
            await DisplayAlert("Validation Error", "Please select a unit to assign.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
        {
            await DisplayAlert("Validation Error", "Please enter tenant full name.", "OK");
            return;
        }

        var newTenant = new Tenant
        {
            FullName = FullNameEntry.Text.Trim(),
            Phone = PhoneEntry.Text?.Trim(),
            Email = EmailEntry.Text?.Trim(),
            UnitNumber = selectedUnit.UnitNumber
        };

        // Update unit status to occupied
        selectedUnit.Status = "Occupied";
        await _dbService.SaveUnitAsync(selectedUnit);

        // Save tenant using SaveTenantAsync (or AddTenantAsync depending on your LocalDbService)
        await _dbService.SaveTenantAsync(newTenant);

        // Reset form inputs
        FullNameEntry.Text = string.Empty;
        PhoneEntry.Text = string.Empty;
        EmailEntry.Text = string.Empty;
        UnitPicker.SelectedItem = null;
        AddFormCard.IsVisible = false;

        await DisplayAlert("Success", "Tenant registered successfully!", "OK");

        // Refresh lists
        await LoadUnitsAsync();
        await LoadFilteredTenantsAsync();
    }

    public void OnToggleAddFormClicked(object sender, EventArgs e)
    {
        AddFormCard.IsVisible = !AddFormCard.IsVisible;
    }
}