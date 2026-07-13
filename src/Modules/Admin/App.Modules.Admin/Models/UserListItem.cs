namespace App.Modules.Admin.Models;

public class UserListItem
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsLockedOut { get; set; }
}
