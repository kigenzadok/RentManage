using RentManage.Models;
using RentManage.Services;

namespace RentManage;

public partial class MainPage : ContentPage
{
    private readonly LocalDbService _dbService;
    private Tenant? _selectedTenantForEdit = null;

    public MainPage(LocalDbService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTenantsAsync();
    }

    private async Task LoadTenantsAsync(string searchQuery = "")
    {
        var tenants = string.IsNullOrWhiteSpace(searchQuery)
            ? await _dbService.GetTenantsAsync()
            : await _dbService.SearchTenantsAsync(searchQuery);

        TenantsListView.ItemsSource = tenants;
    }

    private async void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        await LoadTenantsAsync(e.NewTextValue);
    }

    private async void OnSaveTenantClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) || string.IsNullOrWhiteSpace(PhoneEntry.Text))
        {
            await DisplayAlert("Error", "Please fill in Name and Phone Number", "OK");
            return;
        }

        decimal.TryParse(RentEntry.Text, out decimal rent);

        var tenantToSave = _selectedTenantForEdit ?? new Tenant();
        tenantToSave.FullName = NameEntry.Text;
        tenantToSave.Phone = PhoneEntry.Text;
        tenantToSave.UnitNumber = UnitEntry.Text;
        tenantToSave.MonthlyRent = rent;

        await _dbService.SaveTenantAsync(tenantToSave);

        ClearForm();
        await LoadTenantsAsync(TenantSearchBar.Text);
    }

    private void OnEditTenantInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem { CommandParameter: Tenant tenant })
        {
            _selectedTenantForEdit = tenant;
            NameEntry.Text = tenant.FullName;
            PhoneEntry.Text = tenant.Phone;
            UnitEntry.Text = tenant.UnitNumber;
            RentEntry.Text = tenant.MonthlyRent.ToString();

            FormHeaderLabel.Text = "Edit Tenant";
            SaveButton.Text = "Update Tenant";
            CancelButton.IsVisible = true;
        }
    }

    private async void OnDeleteTenantInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem { CommandParameter: Tenant tenant })
        {
            bool confirm = await DisplayAlert("Confirm Delete", $"Delete tenant '{tenant.FullName}'?", "Yes", "No");
            if (confirm)
            {
                await _dbService.DeleteTenantAsync(tenant);
                await LoadTenantsAsync(TenantSearchBar.Text);
            }
        }
    }

    private void OnCancelEditClicked(object sender, EventArgs e)
    {
        ClearForm();
    }

    private void ClearForm()
    {
        _selectedTenantForEdit = null;
        NameEntry.Text = string.Empty;
        PhoneEntry.Text = string.Empty;
        UnitEntry.Text = string.Empty;
        RentEntry.Text = string.Empty;

        FormHeaderLabel.Text = "Add New Tenant";
        SaveButton.Text = "Save Tenant";
        CancelButton.IsVisible = false;
    }
}