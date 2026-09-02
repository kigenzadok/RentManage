using RentManage.Models;
using RentManage.Services;

namespace RentManage;

public partial class AddEditItemPage : ContentPage
{
    private readonly LocalDbService _dbService;
    private string _initialCategory;

    public AddEditItemPage(LocalDbService dbService, string initialCategory = "Property")
    {
        InitializeComponent();
        _dbService = dbService;
        _initialCategory = initialCategory;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ItemTypePicker.SelectedItem = _initialCategory;
        await LoadDropdownDataAsync();
    }

    private async Task LoadDropdownDataAsync()
    {
        PropertyPicker.ItemsSource = await _dbService.GetPropertiesAsync();
        VacantUnitsPicker.ItemsSource = await _dbService.GetVacantUnitsAsync();
        TenantPicker.ItemsSource = await _dbService.GetTenantsAsync();
    }

    private void OnItemTypeChanged(object sender, EventArgs e)
    {
        string selected = ItemTypePicker.SelectedItem?.ToString();

        PropertyFields.IsVisible = selected == "Property";
        UnitFields.IsVisible = selected == "Unit";
        TenantFields.IsVisible = selected == "Tenant";
        PaymentFields.IsVisible = selected == "Payment";

        TitleLabel.Text = $"Add New {selected}";
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        string category = ItemTypePicker.SelectedItem?.ToString();

        try
        {
            switch (category)
            {
                case "Property":
                    if (string.IsNullOrWhiteSpace(PropertyNameEntry.Text)) return;
                    await _dbService.SavePropertyAsync(new Property
                    {
                        Name = PropertyNameEntry.Text,
                        Address = PropertyAddressEntry.Text
                    });
                    break;

                case "Unit":
                    if (PropertyPicker.SelectedItem is not Property selectedProperty || !decimal.TryParse(MonthlyRentEntry.Text, out decimal rent)) return;
                    await _dbService.SaveUnitAsync(new Unit
                    {
                        PropertyId = selectedProperty.Id,
                        UnitNumber = UnitNumberEntry.Text,
                        MonthlyRent = rent, // Changed RentAmount -> MonthlyRent
                        Status = "Vacant"
                    });
                    break;

                case "Tenant":
                    if (VacantUnitsPicker.SelectedItem is not Unit selectedUnit) return;
                    await _dbService.SaveTenantAsync(new Tenant
                    {
                        FullName = TenantNameEntry.Text,
                        Phone = TenantPhoneEntry.Text,
                        Email = TenantEmailEntry.Text,
                        UnitNumber = selectedUnit.UnitNumber
                    });
                    break;

                case "Payment":
                    if (TenantPicker.SelectedItem is not Tenant selectedTenant || !decimal.TryParse(PaymentAmountEntry.Text, out decimal amount)) return;
                    await _dbService.RecordPaymentAsync(new Payment
                    {
                        TenantId = selectedTenant.Id,
                        TenantName = selectedTenant.FullName,
                        UnitNumber = selectedTenant.UnitNumber,
                        AmountPaid = amount,
                        PaymentMethod = MethodPicker.SelectedItem?.ToString() ?? "Cash",
                        PaymentDate = DateTime.Now
                    });
                    break;
            }

            // Close modal after saving
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}