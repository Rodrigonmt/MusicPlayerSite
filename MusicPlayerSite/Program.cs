var builder = WebApplication.CreateBuilder(args);

// Configura tempo limite do Kestrel ANTES de builder.Build()
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

// Registra servi�os MVC
builder.Services.AddControllersWithViews();

// Constr�i o app
var app = builder.Build();

// Configura pipeline HTTP
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Music}/{action=Index}/{id?}");

app.Run();
