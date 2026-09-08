namespace RentManage.Models;

public class UnitBreakdownItem
{
    public int UnitId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string TenantName { get; set; } = "Vacant";
    public decimal MonthlyRent { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Balance => MonthlyRent - AmountPaid;

    public string PaymentStatus
    {
        get
        {
            if (AmountPaid >= MonthlyRent && MonthlyRent > 0) return "Paid";
            if (AmountPaid > 0) return "Partial";
            return "Unpaid";
        }
    }

    public Color StatusBackgroundColor => PaymentStatus switch
    {
        "Paid" => Color.FromArgb("#DCFCE7"),
        "Partial" => Color.FromArgb("#FEF9C3"),
        _ => Color.FromArgb("#FEE2E2")
    };

    public Color StatusTextColor => PaymentStatus switch
    {
        "Paid" => Color.FromArgb("#15803D"),
        "Partial" => Color.FromArgb("#A16207"),
        _ => Color.FromArgb("#B91C1C")
    };
}