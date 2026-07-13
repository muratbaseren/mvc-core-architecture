namespace App.Application.Abstractions;

/// <summary>
/// Değişikliklerin tek bir transaction içinde kalıcı hale getirilmesini sağlar.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
