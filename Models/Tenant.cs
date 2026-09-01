using SQLite;

namespace RentManage.Models;

[Table("Tenants")]
public class Tenant
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UnitId { get; set; } // Foreign key linking to Unit

    [NotNull]
    public string FullName { get; set; } = string.Empty;

    [NotNull]
    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    // Direct property fields needed by MainPage UI
    public string UnitNumber { get; set; } = string.Empty;

    public decimal MonthlyRent { get; set; }

    public DateTime LeaseStartDate { get; set; } = DateTime.Now;
}