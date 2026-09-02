using SQLite;

namespace RentManage.Models;

[Table("Tenants")]
public class Tenant
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UnitId { get; set; }

    [NotNull]
    public string FullName { get; set; } = string.Empty;

    [NotNull]
    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string UnitNumber { get; set; } = string.Empty;

    public decimal MonthlyRent { get; set; }

    public DateTime LeaseStartDate { get; set; } = DateTime.Now;

    // --- Computed Properties for UI Reminders ---
    [Ignore]
    public bool IsRentPaidThisMonth { get; set; }

    [Ignore]
    public string PaymentStatusText => IsRentPaidThisMonth ? "Paid" : "Rent Overdue";

    [Ignore]
    public Color PaymentStatusColor => IsRentPaidThisMonth ? Color.FromArgb("#059669") : Color.FromArgb("#DC2626");

    [Ignore]
    public Color PaymentStatusBgColor => IsRentPaidThisMonth ? Color.FromArgb("#ECFDF5") : Color.FromArgb("#FEF2F2");
}