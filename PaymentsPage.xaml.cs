using RentManage.Models;
using RentManage.Services;

namespace RentManage;

public partial class PaymentsPage : ContentPage
{
    private readonly LocalDbService _dbService;

    public PaymentsPage(LocalDbService dbService)
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
        var payments = await _dbService.GetPaymentsAsync();
        PaymentsListView.ItemsSource = payments;

        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        // Using AmountPaid and PaymentDate matching Payment model
        decimal monthlyTotal = payments
            .Where(p => p.PaymentDate.Month == currentMonth && p.PaymentDate.Year == currentYear)
            .Sum(p => p.AmountPaid);

        decimal grandTotal = payments.Sum(p => p.AmountPaid);

        MonthlyTotalLabel.Text = $"${monthlyTotal:F2}";
        TotalRevenueLabel.Text = $"${grandTotal:F2}";

        var tenants = await _dbService.GetTenantsAsync();
        TenantPicker.ItemsSource = tenants;
    }

    public void OnToggleAddFormClicked(object sender, EventArgs e)
    {
        AddFormCard.IsVisible = !AddFormCard.IsVisible;
    }

    public async void OnSavePaymentClicked(object sender, EventArgs e)
    {
        if (TenantPicker.SelectedItem is not Tenant selectedTenant)
        {
            await DisplayAlert("Validation Error", "Please select a tenant.", "OK");
            return;
        }

        if (!decimal.TryParse(AmountEntry.Text, out decimal amount) || amount <= 0)
        {
            await DisplayAlert("Validation Error", "Please enter a valid amount.", "OK");
            return;
        }

        string method = MethodPicker.SelectedItem?.ToString() ?? "Cash";

        // Using selectedTenant.FullName / AmountPaid / PaymentMethod
        var newPayment = new Payment
        {
            TenantId = selectedTenant.Id,
            TenantName = selectedTenant.FullName,
            AmountPaid = amount,
            PaymentMethod = method,
            PaymentDate = DateTime.Now
        };

        // Call database service method
        await _dbService.AddPaymentAsync(newPayment);

        AmountEntry.Text = string.Empty;
        TenantPicker.SelectedItem = null;
        MethodPicker.SelectedItem = null;
        AddFormCard.IsVisible = false;

        await LoadDataAsync();
    }

    private async void OnShareReceiptClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Payment payment)
        {
            await DisplayAlert("Receipt", $"Generating PDF receipt for {payment.TenantName}...", "OK");
        }
    }
}