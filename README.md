# ASP.NET Core MVC — Modüler Clean Architecture (.NET 10)

ASP.NET Core MVC üzerinde **Clean Architecture** ve **modüler (feature-folder) mimari** ile
kurgulanmış başlangıç projesi. Her özellik (feature) kendi klasöründe yaşar; bir modülü
kaldırmak için **klasörünü silmek yeterlidir** — uygulama o modülsüz çalışmaya devam eder.

## Özellikler

- ✅ **Clean Architecture** katmanları (SharedKernel → Application → Infrastructure → Web)
- ✅ **Modüler yapı**: modül = klasör; wildcard proje referansı + çalışma zamanında otomatik keşif
- ✅ **MediatR** (CQRS) + pipeline behavior'ları (FluentValidation doğrulama, loglama)
- ✅ **EF Core + SQLite**, generic **Repository** ve **UnitOfWork** pattern'leri
- ✅ **GraphQL API** (HotChocolate): her yeni entity için **otomatik** sorgu alanları
- ✅ **ASP.NET Core Identity**: Kayıt, Giriş, Şifremi Unuttum, Şifre Sıfırlama ekranları
- ✅ **Google ile giriş** (yapılandırma ile açılır)
- ✅ **Serilog**: günlük dönen log dosyaları + yalnızca hatalar için ayrı günlük `txt`
- ✅ **TailwindCSS** tabanlı hazır ekranlar

## Hızlı Başlangıç

```bash
dotnet run --project src/Web/App.Web
```

Uygulama açıldığında:

| Adres | Açıklama |
|---|---|
| `/` | Yüklü modülleri gösteren dashboard |
| `/Products` | Örnek Ürünler modülü (CRUD) |
| `/Account/Login`, `/Account/Register` | Kimlik doğrulama ekranları |
| `/graphql` | GraphQL endpoint'i + tarayıcıda Nitro IDE |

Veritabanı (SQLite `app.db`) ilk çalıştırmada modül entity'leri dahil otomatik oluşturulur.
Loglar `logs/log-YYYYMMDD.txt`, hatalar ayrıca `logs/errors-YYYYMMDD.txt` dosyasına yazılır.

## Mimari

```
src/
├── Core/
│   ├── App.SharedKernel/          # En içteki halka: her katmanın görebileceği çekirdek
│   │   ├── Domain/                #   BaseEntity, IAggregateRoot
│   │   ├── Modules/               #   IModule, ModuleRegistry, ModuleMenuItem
│   │   └── Common/                #   Result, Result<T>
│   └── App.Application/           # Uygulama katmanı (iş kuralları için altyapı)
│       ├── Abstractions/          #   IRepository<T>, IUnitOfWork, IEmailService
│       ├── Behaviors/             #   ValidationBehavior, LoggingBehavior (MediatR pipeline)
│       └── DependencyInjection.cs #   AddApplicationCore(moduleAssemblies)
├── Infrastructure/
│   └── App.Infrastructure/        # Dış dünya: veritabanı, kimlik, e-posta
│       ├── Persistence/           #   AppDbContext, EfRepository<T>, UnitOfWork
│       ├── Identity/              #   AppUser
│       └── Services/              #   LogEmailService (dev)
├── Web/
│   └── App.Web/                   # MVC host: modül yükleyici, GraphQL, auth ekranları
│       └── Infrastructure/        #   ModuleLoader, GraphQL kurulumu
└── Modules/                       # ★ ÖZELLİK MODÜLLERİ — her klasör bir modül
    └── Products/
        └── App.Modules.Products/
            ├── ProductsModule.cs  #   IModule implementasyonu (ad, versiyon, menü)
            ├── Domain/            #   Product entity'si
            ├── Persistence/       #   ProductConfiguration (EF)
            ├── Application/       #   MediatR command/query + validator'lar
            ├── Controllers/       #   ProductsController
            ├── Views/             #   Tailwind view'ları (assembly'ye derlenir)
            └── GraphQl/           #   Modüle özel mutation'lar (opsiyonel)
```

### Modülerlik nasıl çalışıyor?

1. **Derleme zamanı** — `App.Web.csproj` modülleri wildcard ile referans alır:
   ```xml
   <ProjectReference Include="..\..\Modules\**\*.csproj" />
   ```
   Yeni modül klasörü otomatik derlenir; silinen klasörün referansı kendiliğinden kalkar.

2. **Çalışma zamanı** — `ModuleLoader`, çıktı klasöründeki `App.Modules.*.dll` dosyalarını
   tarar, `IModule` implementasyonlarını `ModuleRegistry`'ye kaydeder.

3. **Otomatik entegrasyonlar** (modül eklerken hiçbir merkezi dosyaya dokunulmaz):
   - **EF Core**: `AppDbContext`, modül assembly'lerindeki `IEntityTypeConfiguration<T>`
     sınıflarını ve `IAggregateRoot` entity'lerini modele otomatik dahil eder.
   - **MediatR + FluentValidation**: handler ve validator'lar modül assembly'lerinden kaydedilir.
   - **MVC**: modül assembly'leri `ApplicationPart` olarak eklenir — controller'lar ve
     derlenmiş view'lar bulunur.
   - **GraphQL**: her aggregate root için liste (`products`) ve tekil (`productById`) sorgu
     alanları üretilir; modüldeki `ObjectTypeExtension` sınıfları (mutation'lar) şemaya eklenir.
   - **Menü**: `IModule.MenuItems` ile modül, üst menüye bağlantı ekler.

### Modül kaldırma

```bash
rm -rf src/Modules/Products          # 1) Modül klasörünü sil
dotnet sln remove src/Modules/Products/App.Modules.Products  # 2) Solution'dan çıkar
rm -rf src/Web/App.Web/bin           # 3) Eski dll kalmasın diye temiz derleme
dotnet run --project src/Web/App.Web
```

> **Not:** 2. adım yalnızca solution dosyası üzerinden derliyorsanız gereklidir;
> `dotnet build src/Web/App.Web` klasör silindikten sonra doğrudan çalışır.
> 3. adım önemlidir: `bin/` içinde önceki derlemeden kalan modül dll'i, modül
> yükleyici tarafından bulunmaya devam eder.

## Yeni Modül Geliştirme (Mini Rehber)

Örnek: **Notes** (Notlar) modülü. `src/Modules/Notes/App.Modules.Notes/` klasörünü oluşturun.

### 1. Proje dosyası — `App.Modules.Notes.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="10.0.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\Core\App.Application\App.Application.csproj" />
  </ItemGroup>
</Project>
```

Solution'a ekleyin (IDE'de görünmesi için): `dotnet sln add src/Modules/Notes/App.Modules.Notes`

### 2. Modül tanımı — `NotesModule.cs`

```csharp
public class NotesModule : IModule
{
    public string Name => "Notlar";
    public string Version => "1.0.0";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Modüle özel servisler (örn. services.AddScoped<INoteExporter, PdfNoteExporter>();)
    }

    public IEnumerable<ModuleMenuItem> MenuItems => [ new("Notlar", "/Notes", Order: 20) ];
}
```

### 3. Entity — `Domain/Note.cs`

```csharp
public class Note : BaseEntity, IAggregateRoot   // ← otomatik EF + GraphQL kaydı
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
}
```

Bu kadar! Uygulama açıldığında `Notes` tablosu oluşur ve GraphQL şemasında
`notes` / `noteById` sorguları belirir. İnce ayar isterseniz:

```csharp
public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");
        builder.Property(n => n.Title).IsRequired().HasMaxLength(150);
    }
}
```

### 4. İş mantığı — MediatR command örneği

```csharp
public record CreateNoteCommand(string Title, string? Content) : IRequest<Result<Guid>>;

public class CreateNoteValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteValidator() =>
        RuleFor(x => x.Title).NotEmpty().WithMessage("Başlık zorunludur.");
}

public class CreateNoteHandler(IRepository<Note> repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateNoteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateNoteCommand request, CancellationToken ct)
    {
        var note = new Note { Title = request.Title, Content = request.Content };
        await repository.AddAsync(note, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(note.Id);
    }
}
```

Validator, MediatR pipeline'ındaki `ValidationBehavior` tarafından otomatik çalıştırılır;
handler'a hatalı istek ulaşmaz. Her istek `LoggingBehavior` ile süresiyle loglanır.

### 5. Controller + View

```csharp
public class NotesController(ISender sender) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(await sender.Send(new GetNotesQuery(), ct));
}
```

View'ları `Views/Notes/Index.cshtml` altına koyun; `Views/_ViewStart.cshtml` dosyasına
`Layout = "_Layout";` yazarsanız ana şablonu kullanır. View'lar modül dll'ine derlenir.

### 6. (Opsiyonel) Modüle özel GraphQL mutation

`HotChocolate.Types` paketini ekleyip bir `ObjectTypeExtension` yazın — şemaya otomatik eklenir
(örnek: `Products` modülündeki `ProductMutations`).

## Var Olan Yapıların Kullanımı

### Repository + UnitOfWork

```csharp
public class ArchiveNotesHandler(IRepository<Note> repo, IUnitOfWork uow) : ...
{
    // Sorgulama
    var all    = await repo.ListAsync(ct);
    var single = await repo.GetByIdAsync(id, ct);
    var some   = await repo.ListAsync(n => n.Title.Contains("rapor"), ct);
    var query  = repo.Query().OrderBy(n => n.CreatedAt);   // IQueryable — ileri senaryolar

    // Yazma: değişiklikler UnitOfWork.SaveChangesAsync ile tek seferde kalıcı olur
    await repo.AddAsync(note, ct);
    repo.Update(note);
    repo.Remove(note);
    await uow.SaveChangesAsync(ct);   // CreatedAt/UpdatedAt otomatik doldurulur
}
```

### MediatR gönderimi (controller'dan)

```csharp
var result = await sender.Send(new CreateNoteCommand("Başlık", null), ct);
if (result.IsSuccess) { /* result.Value => Guid */ }
```

### GraphQL sorgu örnekleri

```graphql
{ products(first: 10, where: { price: { gt: 100 } }, order: { name: ASC }) {
    nodes { id name price stock } pageInfo { hasNextPage } } }

{ productById(id: "...") { name price } }

mutation { createProduct(name: "Klavye", price: 899.90, stock: 10) }
```

### E-posta gönderimi

`IEmailService` enjekte edin. Geliştirmede `LogEmailService` e-postayı log'a yazar
(şifre sıfırlama bağlantısı log dosyasında görünür). Üretim için SMTP/SendGrid
implementasyonu yazıp `AddInfrastructure` içindeki kaydı değiştirin.

### Loglama

Serilog yapılandırması `appsettings.json` → `Serilog` bölümündedir.
Sınıflarda `ILogger<T>` enjekte ederek loglayın; `Error` ve üstü seviyeler ayrıca
`logs/errors-YYYYMMDD.txt` dosyasına düşer.

## Kimlik Doğrulama

- Ekranlar: `/Account/Register`, `/Account/Login`, `/Account/ForgotPassword`, `/Account/ResetPassword`
- Bir sayfayı korumak için controller/action üzerine `[Authorize]` ekleyin.
- **Google ile giriş** için `appsettings.json` (veya user-secrets) içine:
  ```json
  "Authentication": { "Google": { "ClientId": "xxx", "ClientSecret": "yyy" } }
  ```
  Değerler doluysa giriş/kayıt ekranlarında Google butonu otomatik görünür.
  (Google Cloud Console'da OAuth istemcisi oluşturup yönlendirme adresi olarak
  `https://localhost:xxxx/signin-google` tanımlayın.)

## Notlar / Tasarım Kararları

- **Veritabanı şeması** `EnsureCreated` ile oluşturulur (modüler yapı için pratik).
  Üretimde EF Migrations'a geçmek isterseniz `InitializeDatabaseAsync` içeriğini
  `Database.MigrateAsync()` ile değiştirin.
- **TailwindCSS** hızlı geliştirme için CDN üzerinden yüklüdür. Üretimde
  [Tailwind CLI](https://tailwindcss.com/docs/installation) ile statik CSS üretip
  `_Layout.cshtml` içindeki CDN script'ini kendi CSS dosyanızla değiştirin.
- **MediatR 12** (Apache-2.0 lisanslı sürüm) kullanılmıştır.
- Tüm sürüm geçmişi için [CHANGELOG.md](CHANGELOG.md) dosyasına bakın.
