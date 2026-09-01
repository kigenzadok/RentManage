using SQLite;

namespace RentManage.Models;

[Table("Properties")]
public class Property
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty; // e.g., Sunset Apartments

    public string Address { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}