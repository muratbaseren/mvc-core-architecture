using System.Security.Claims;
using App.Application.Abstractions;
using App.Infrastructure.Identity;
using App.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.Controllers;

/// <summary>
/// Kimlik doğrulama akışları: kayıt, giriş, çıkış, şifremi unuttum,
/// şifre sıfırlama ve Google ile harici giriş.
/// </summary>
public class AccountController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IEmailService emailService,
    ILogger<AccountController> logger) : Controller
{
    // ---------- Kayıt ----------

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            logger.LogInformation("Yeni kullanıcı kaydı: {Email}", model.Email);
            await signInManager.SignInAsync(user, isPersistent: false);
            TempData["Success"] = "Hesabınız oluşturuldu, hoş geldiniz!";
            return LocalRedirect(returnUrl ?? "/");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    // ---------- Giriş / Çıkış ----------

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var result = await signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("Kullanıcı giriş yaptı: {Email}", model.Email);
            return LocalRedirect(returnUrl ?? "/");
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("Hesap kilitlendi: {Email}", model.Email);
            ModelState.AddModelError(string.Empty,
                "Çok fazla başarısız deneme. Hesabınız geçici olarak kilitlendi.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // ---------- Google ile Giriş ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError is not null)
        {
            TempData["Error"] = $"Harici giriş sağlayıcı hatası: {remoteError}";
            return RedirectToAction(nameof(Login));
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            TempData["Error"] = "Harici giriş bilgileri alınamadı.";
            return RedirectToAction(nameof(Login));
        }

        // Bu harici hesap daha önce bağlanmışsa doğrudan giriş yap.
        var signInResult = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (signInResult.Succeeded)
            return LocalRedirect(returnUrl ?? "/");

        // İlk kez geliyorsa: e-posta ile kullanıcı oluştur/eşleştir ve bağla.
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (email is null)
        {
            TempData["Error"] = "Google hesabınızdan e-posta bilgisi alınamadı.";
            return RedirectToAction(nameof(Login));
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = info.Principal.FindFirstValue(ClaimTypes.Name),
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                TempData["Error"] = "Hesap oluşturulamadı: " +
                    string.Join(", ", createResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Login));
            }
        }

        await userManager.AddLoginAsync(user, info);
        await signInManager.SignInAsync(user, isPersistent: false);
        logger.LogInformation("Google ile giriş: {Email}", email);
        return LocalRedirect(returnUrl ?? "/");
    }

    // ---------- Şifremi Unuttum / Şifre Sıfırlama ----------

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is not null)
        {
            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(
                nameof(ResetPassword), "Account",
                new { email = model.Email, code },
                protocol: Request.Scheme)!;

            await emailService.SendAsync(
                model.Email,
                "Şifre Sıfırlama",
                $"Şifrenizi sıfırlamak için <a href='{callbackUrl}'>buraya tıklayın</a>.");
        }

        // Hesabın var olup olmadığını sızdırmamak için her durumda aynı sayfa.
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation() => View();

    [HttpGet]
    public IActionResult ResetPassword(string? email = null, string? code = null)
    {
        if (email is null || code is null)
        {
            TempData["Error"] = "Geçersiz şifre sıfırlama bağlantısı.";
            return RedirectToAction(nameof(Login));
        }

        return View(new ResetPasswordViewModel { Email = email, Code = code });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null)
            return RedirectToAction(nameof(ResetPasswordConfirmation));

        var result = await userManager.ResetPasswordAsync(user, model.Code, model.Password);
        if (result.Succeeded)
        {
            logger.LogInformation("Şifre sıfırlandı: {Email}", model.Email);
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation() => View();

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
