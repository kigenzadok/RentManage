namespace RentManage.Models;

public class UnitMonthlyStatus
{
    public int UnitId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string MonthYearDisplay { get; set; } = string.Empty; // e.g. "September 2026"

    public decimal TargetRent { get; set; }
    public decimal AmountPaid { get; set; }

    public string TenantName { get; set; } = "—";
    public string PaymentMethod { get; set; } = "—";
    public DateTime? PaymentDate { get; set; }

    public bool IsVacant { get; set; }
    public string StatusBadge => IsVacant ? "Vacant" : (AmountPaid >= TargetRent ? "Paid" : "Partial/Unpaid");
    public string StatusColor => IsVacant ? "#EF4444" : (AmountPaid >= TargetRent ? "#16A34A" : "#F59E0B");
}