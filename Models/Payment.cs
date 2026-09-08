using SQLite;

namespace RentManage.Models;

[Table("Payments")]
public class Payment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int TenantId { get; set; }

    [NotNull]
    public string TenantName { get; set; } = string.Empty;

    public string UnitNumber { get; set; } = string.Empty;

    [NotNull]
    public decimal AmountPaid { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.Now;

    public string PaymentMethod { get; set; } = "Cash"; // Cash, M-Pesa, Bank Transfer, Card

    public string Notes { get; set; } = string.Empty;
    [PrimaryKey, AutoIncrement]

    public int UnitId { get; set; }
    // Advance / Target billing period properties
    public int TargetMonth { get; set; }

    public int TargetYear { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;
}