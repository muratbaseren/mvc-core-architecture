using Microsoft.AspNetCore.Identity;

namespace App.Infrastructure.Identity;

/// <summary>
/// Uygulamanın kimlik doğrulama kullanıcısı. İhtiyaç halinde
/// yeni profil alanları buraya eklenebilir.
/// </summary>
public class AppUser : IdentityUser
{
    public string? FullName { get; set; }
}
