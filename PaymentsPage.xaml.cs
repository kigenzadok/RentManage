using RentManage.Models;
using RentManage.Services;
using Microsoft.Maui.ApplicationModel.DataTransfer;

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
            try
            {
                // 1. Generate text receipt content
                string fileName = $"Receipt_{payment.TenantName.Replace(" ", "_")}_{payment.PaymentDate:yyyyMMdd}.txt";
                string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                string receiptText =
    $@"========================================
            RENT PAYMENT RECEIPT         
========================================
Date:           {payment.PaymentDate:dd MMMM yyyy HH:mm}
Receipt No:     REC-{payment.Id:D5}

----------------------------------------
Tenant Name:    {payment.TenantName}
Amount Paid:    ${payment.AmountPaid:F2}
Payment Method: {payment.PaymentMethod}
Status:         COMPLETED
----------------------------------------

Thank you for your payment!
========================================";

                // 2. Write file to device cache directory
                await File.WriteAllTextAsync(filePath, receiptText);

                // 3. Trigger native OS Share dialog (WhatsApp, Email, Drive, Printing, etc.)
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = $"Payment Receipt - {payment.TenantName}",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not share receipt: {ex.Message}", "OK");
            }
        }
    }
    private async void OnMonthSelected(object sender, DateChangedEventArgs e)
    {
        await RefreshMonthlyDataAsync(e.NewDate.Year, e.NewDate.Month);
    }

    private async Task RefreshMonthlyDataAsync(int year, int month)
    {
        var breakdown = await _dbService.GetMonthlyUnitBreakdownAsync(year, month);
        MonthlyBreakdownListView.ItemsSource = breakdown;
    }
    private void OnShowTransactionsClicked(object sender, EventArgs e)
    {
        TransactionsView.IsVisible = true;
        BreakdownView.IsVisible = false;

        BtnShowTransactions.BackgroundColor = Color.FromArgb("#4F46E5");
        BtnShowTransactions.TextColor = Colors.White;

        BtnShowBreakdown.BackgroundColor = Color.FromArgb("#E2E8F0");
        BtnShowBreakdown.TextColor = Color.FromArgb("#475569");
    }

    private void OnShowBreakdownClicked(object sender, EventArgs e)
    {
        TransactionsView.IsVisible = false;
        BreakdownView.IsVisible = true;

        BtnShowBreakdown.BackgroundColor = Color.FromArgb("#4F46E5");
        BtnShowBreakdown.TextColor = Colors.White;

        BtnShowTransactions.BackgroundColor = Color.FromArgb("#E2E8F0");
        BtnShowTransactions.TextColor = Color.FromArgb("#475569");
    }
}