using SQLite;

namespace RentManage.Models;

public enum UserRole
{
    Owner,
    Admin,
    Tenant,
    Housekeeper,
    Agent,
    Accountant
}

[Table("UserProfiles")]
public class UserProfile
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Owner;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}