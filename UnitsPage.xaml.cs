using RentManage.Models;
using RentManage.Services;

namespace RentManage;

[QueryProperty(nameof(PropertyId), "PropertyId")]
[QueryProperty(nameof(PropertyName), "PropertyName")]
public partial class UnitsPage : ContentPage
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

    public UnitsPage(LocalDbService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Update page title dynamically
        if (!string.IsNullOrEmpty(PropertyName))
        {
            Title = $"Units - {PropertyName}";
        }

        // 1. Load properties first so picker has items
        await LoadPropertiesAsync();

        // 2. Load units filtered by PropertyId
        await LoadFilteredUnitsAsync();
    }

    private async Task LoadPropertiesAsync()
    {
        // Fetch properties from SQLite
        var properties = await _dbService.GetPropertiesAsync();

        // Bind to picker
        PropertyPicker.ItemsSource = properties;

        // Auto-select the property matching the passed PropertyId parameter
        if (PropertyId > 0 && properties != null)
        {
            var currentProperty = properties.FirstOrDefault(p => p.Id == PropertyId);
            if (currentProperty != null)
            {
                PropertyPicker.SelectedItem = currentProperty;
            }
        }
    }

    private async Task LoadFilteredUnitsAsync()
    {
        if (PropertyId > 0)
        {
            // Fetch units specific to this property
            var allUnits = await _dbService.GetUnitsAsync();
            var filteredUnits = allUnits.Where(u => u.PropertyId == PropertyId).ToList();
            UnitsListView.ItemsSource = filteredUnits;
        }
        else
        {
            // Fallback: show all units if no PropertyId passed
            UnitsListView.ItemsSource = await _dbService.GetUnitsAsync();
        }
    }

    public void OnToggleAddFormClicked(object sender, EventArgs e)
    {
        AddFormCard.IsVisible = !AddFormCard.IsVisible;
    }

    public async void OnSaveUnitClicked(object sender, EventArgs e)
    {
        if (PropertyPicker.SelectedItem is not Property selectedProperty)
        {
            await DisplayAlert("Validation Error", "Please select a property.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(UnitNumberEntry.Text))
        {
            await DisplayAlert("Validation Error", "Please enter a unit number.", "OK");
            return;
        }

        if (!decimal.TryParse(MonthlyRentEntry.Text, out decimal rentAmount))
        {
            await DisplayAlert("Validation Error", "Please enter a valid rent amount.", "OK");
            return;
        }

        var newUnit = new Unit
        {
            PropertyId = selectedProperty.Id,
            UnitNumber = UnitNumberEntry.Text.Trim(),
            MonthlyRent = rentAmount,
            Status = "Vacant"
        };

        await _dbService.SaveUnitAsync(newUnit);

        // Reset entries and hide form
        UnitNumberEntry.Text = string.Empty;
        MonthlyRentEntry.Text = string.Empty;
        AddFormCard.IsVisible = false;

        // Refresh list using existing method name
        await LoadFilteredUnitsAsync();
    }
}