using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Test;
using Microsoft.Extensions.DependencyInjection;
using ids.TestData;
using Serilog.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Microsoft.EntityFrameworkCore;
using ids.Data;
using ids.Database;
using Microsoft.AspNetCore.Identity;

Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Debug()
             .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
             .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
             .MinimumLevel.Override("System", LogEventLevel.Warning)
             .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
             .Enrich.FromLogContext()
             .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
             .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
             .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString(name: "DefaultConnection");
var migrationsAssembly = typeof(Config).Assembly.GetName().Name;

builder.Logging.AddSerilog();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(connectionString, sqliteOptions => sqliteOptions.MigrationsAssembly(migrationsAssembly));
});

builder.Services.AddIdentity<IdentityUser,  IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton<ICorsPolicyService>((container) => {
    var logger = container.GetRequiredService<ILogger<DefaultCorsPolicyService>>();
    return new DefaultCorsPolicyService(logger)
    {
        AllowAll = true
    };
});

builder.Services.AddIdentityServer(setupAction: options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    options.EmitStaticAudienceClaim = true;
}).AddConfigurationStore(options => options.ConfigureDbContext = b => b.UseSqlite(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly)))
    .AddOperationalStore(options => options.ConfigureDbContext = b => b.UseSqlite(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly)))
    .AddAspNetIdentity<IdentityUser>();

//.AddInMemoryClients(Config.Clients)
//.AddInMemoryApiResources(Config.ApiResources)
//.AddInMemoryApiScopes(Config.ApiScopes)
//.AddInMemoryIdentityResources(Config.IdentityResources);

builder.Services.AddAuthentication();
    //.AddCookie("My auth cookie", options =>
    //{
    //    options.Cookie.HttpOnly = true;
    //});
var app = builder.Build();

app.UseIdentityServer();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages().RequireAuthorization();

if (args.Contains("/seed")){
    Log.Information("Seeding database...");
    SeedData.EnsureSeedData(app);
    Log.Information("Done seeding database...exiting");
    return;
}

app.Run();
