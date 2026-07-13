using App.SharedKernel.Domain;

namespace App.Modules.AuditLogging.Domain;

/// <summary>
/// Bir entity üzerindeki değişikliğin denetim kaydı.
/// IAggregateRoot sayesinde GraphQL'de `auditLogs` olarak da sorgulanabilir.
/// </summary>
public class AuditLog : BaseEntity, IAggregateRoot
{
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;

    /// <summary>Added / Modified / Deleted</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Değişen alanların JSON gösterimi (eski/yeni değerler).</summary>
    public string? Changes { get; set; }

    public string? UserName { get; set; }
}
