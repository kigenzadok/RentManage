using SQLite;

namespace RentManage.Models;

[Table("Units")]
public class Unit
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int PropertyId { get; set; } // Foreign key linking to Property

    [NotNull]
    public string UnitNumber { get; set; } = string.Empty; // e.g., Apt 4B

    public decimal MonthlyRent { get; set; }

    public string Status { get; set; } = "Vacant"; // "Vacant" or "Occupied"
}