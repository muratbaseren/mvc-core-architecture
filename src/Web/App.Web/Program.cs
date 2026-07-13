using App.Application;
using App.Infrastructure;
using App.SharedKernel.Modules;
using App.Web.Infrastructure;
using App.Web.Infrastructure.GraphQl;
using Serilog;

// Uygulama başlarken oluşabilecek hataları da yakalamak için bootstrap logger.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Uygulama başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog: konfigürasyon appsettings.json'daki "Serilog" bölümünden okunur.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // 1) Modülleri keşfet (servis kayıtlarından ÖNCE — EF Core ve MediatR
    //    modül assembly'lerini ModuleRegistry üzerinden görür).
    ModuleLoader.LoadModules(Log.Logger);

    // 2) Çekirdek katmanlar.
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplicationCore(ModuleRegistry.Assemblies);

    // 3) Modüllerin kendi servis kayıtları.
    foreach (var module in ModuleRegistry.Modules)
        module.ConfigureServices(builder.Services, builder.Configuration);

    // 4) MVC — modül assembly'leri ApplicationPart olarak eklenir,
    //    böylece modüllerdeki controller ve derlenmiş view'lar bulunur.
    var mvcBuilder = builder.Services.AddControllersWithViews();
    foreach (var assembly in ModuleRegistry.Assemblies)
        mvcBuilder.AddApplicationPart(assembly);

    // GraphQL: modül entity'lerinden otomatik şema üretimi.
    builder.Services.AddModularGraphQl();

    // Google ile giriş: yalnızca appsettings'te ClientId tanımlıysa etkinleşir.
    var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
    if (!string.IsNullOrWhiteSpace(googleClientId))
    {
        builder.Services.AddAuthentication().AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        });
    }

    // Identity cookie yolları.
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseStaticFiles();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // GraphQL endpoint'i (+ tarayıcıda Nitro IDE: /graphql).
    app.MapGraphQL();

    // Veritabanı şemasını (modül entity'leri dahil) oluştur.
    await app.Services.InitializeDatabaseAsync();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama başlatılırken ölümcül hata oluştu");
}
finally
{
    Log.CloseAndFlush();
}
