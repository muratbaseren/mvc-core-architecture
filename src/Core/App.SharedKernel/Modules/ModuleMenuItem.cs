namespace App.SharedKernel.Modules;

/// <summary>
/// Modülün ana menüye eklemek istediği bağlantı.
/// </summary>
public record ModuleMenuItem(string Title, string Url, int Order = 0);
