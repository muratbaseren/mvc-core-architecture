# Changelog

Bu projede yapılan tüm önemli değişiklikler bu dosyada belgelenir.
En yeni sürüm her zaman en üsttedir.

## [0.5.0] - 2026-07-13

### Eklendi
- **GraphQL API** (HotChocolate 16):
  - `/graphql` endpoint'i (tarayıcıdan açıldığında Nitro IDE).
  - `EntityQueryExtension<T>`: Her aggregate root entity için **otomatik** query alanları —
    çoğul liste alanı (cursor sayfalama + filtreleme + sıralama) ve `xxxById(id)` tekil alanı.
    Yeni entity eklendiğinde şema kendiliğinden genişler.
  - Modül assembly'lerindeki `ObjectTypeExtension` sınıfları (modüle özel mutation/query'ler)
    otomatik keşfedilip şemaya eklenir.
  - GraphQL resolver'ları için `IDbContextFactory` tabanlı güvenli DbContext yönetimi.

### Değişti
- `AddInfrastructure`: `AddDbContext` yerine `AddDbContextFactory` + factory'den scoped kayıt
  (MVC/Identity aynı şekilde çalışır, GraphQL paralel resolver'ları için güvenli).

## [0.4.0] - 2026-07-13

### Eklendi
- **Kimlik doğrulama ekranları** (`AccountController` + Tailwind view'ları):
  - Kayıt Ol (`/Account/Register`) — kayıt sonrası otomatik giriş.
  - Giriş (`/Account/Login`) — kilitleme (lockout) destekli.
  - Şifremi Unuttum (`/Account/ForgotPassword`) — sıfırlama bağlantısı `IEmailService`
    üzerinden gönderilir (dev ortamında log dosyasına yazılır).
  - Şifre Sıfırlama (`/Account/ResetPassword`) — token doğrulamalı.
  - Erişim Engellendi ve onay sayfaları.
- **Google ile giriş**: `Authentication:Google:ClientId/ClientSecret` appsettings'te
  tanımlandığında otomatik etkinleşir; giriş/kayıt sayfalarında Google butonu görünür.
  Harici hesap ilk girişte e-posta üzerinden yerel hesapla eşleştirilir/oluşturulur.
- Layout'a giriş durumu bölümü (`_LoginPartial`) eklendi.

### Doğrulandı
- Kayıt → otomatik giriş → şifremi unuttum → sıfırlama bağlantısı → yeni şifre ile
  giriş akışının tamamı uçtan uca test edildi.

## [0.3.0] - 2026-07-13

### Eklendi
- **App.Web** host uygulaması:
  - `ModuleLoader`: `App.Modules.*.dll` assembly'lerini keşfedip `ModuleRegistry`'ye kaydeder.
    Modül klasörü silindiğinde modül uygulamadan tamamen kalkar.
  - Wildcard `ProjectReference` (`src/Modules/**/*.csproj`): yeni modül eklemek/kaldırmak
    için csproj'a dokunmak gerekmez.
  - Modül assembly'leri `ApplicationPart` olarak eklenir (controller + derlenmiş view keşfi).
  - **Serilog** entegrasyonu: konsol + günlük dönen dosya (`logs/log-YYYYMMDD.txt`) ve
    yalnızca hataları içeren günlük dosya (`logs/errors-YYYYMMDD.txt`).
  - **TailwindCSS** tabanlı layout, modül menülerini otomatik listeleyen navigasyon.
  - Ana sayfa: yüklü modülleri ve mimari katmanları gösteren dashboard.
  - `IModule.MenuItems`: modüllerin ana menüye otomatik bağlantı eklemesi.
- Bootstrap/jQuery şablon dosyaları kaldırıldı.
- `app.db` ve `logs/` `.gitignore`'a eklendi.

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
