var builder = WebApplication.CreateBuilder(args);

// 🔧 Define a porta vinda do Railway (ou usa 3000 localmente)
var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
builder.WebHost.UseUrls($"http://*:{port}");

// ⏱️ Configura limites do Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

// ➕ Registra serviços MVC
builder.Services.AddControllersWithViews();

// 🛠️ Constrói o app
var app = builder.Build();

// 🌐 Middleware
app.UseStaticFiles();
app.UseRouting();

// 📍 Rota padrão
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Music}/{action=Index}/{id?}");

// 🚀 Inicia o app
app.Run();
