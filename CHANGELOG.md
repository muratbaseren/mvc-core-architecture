# Changelog

Bu projede yapılan tüm önemli değişiklikler bu dosyada belgelenir.
En yeni sürüm her zaman en üsttedir.

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
