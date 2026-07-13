namespace App.SharedKernel.Domain;

/// <summary>
/// Aggregate root işaretleyici arayüzü. Bu arayüzü uygulayan entity'ler
/// repository üzerinden erişilebilir ve GraphQL şemasına otomatik eklenir.
/// </summary>
public interface IAggregateRoot;
