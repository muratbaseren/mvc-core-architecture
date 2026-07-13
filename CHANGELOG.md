# Changelog

Bu projede yapılan tüm önemli değişiklikler bu dosyada belgelenir.
En yeni sürüm her zaman en üsttedir.

## [0.2.0] - 2026-07-13

### Eklendi
- **App.Infrastructure** katmanı:
  - `AppDbContext`: SQLite üzerinde çalışan, Identity tablolarını içeren tek DbContext.
    Modül assembly'lerindeki `IEntityTypeConfiguration<T>` sınıflarını ve aggregate
    root entity'leri otomatik keşfeder — yeni modülde DbContext'e dokunmak gerekmez.
  - Audit alanlarının (`CreatedAt`, `UpdatedAt`) `SaveChangesAsync` içinde otomatik doldurulması.
  - `EfRepository<T>`: Generic repository implementasyonu.
  - `UnitOfWork`: Transaction/save yönetimi.
  - `AppUser`: ASP.NET Core Identity kullanıcısı.
  - `LogEmailService`: Geliştirme ortamı için e-postaları log'a yazan `IEmailService` implementasyonu.
  - `AddInfrastructure()` DI uzantısı ve `InitializeDatabaseAsync()` (EnsureCreated) başlangıç metodu.

## [0.1.0] - 2026-07-13

### Eklendi
- Solution iskeleti oluşturuldu (`MvcCoreArchitecture.sln`, `Directory.Build.props`).
- **App.SharedKernel** katmanı:
  - `BaseEntity`: Id ve audit alanları içeren temel entity sınıfı.
  - `IAggregateRoot`: Repository ve GraphQL otomasyonu için işaretleyici arayüz.
  - `IModule`: Modül sözleşmesi (ad, versiyon, servis kaydı).
  - `ModuleRegistry`: Keşfedilen modüllerin ve assembly'lerin merkezi kaydı.
  - `Result` / `Result<T>`: İşlem sonuç tipi.
- **App.Application** katmanı:
  - `IRepository<T>` ve `IUnitOfWork` soyutlamaları.
  - MediatR entegrasyonu ve pipeline behavior'ları (`ValidationBehavior`, `LoggingBehavior`).
  - FluentValidation entegrasyonu (modül assembly'lerinden otomatik validator kaydı).
