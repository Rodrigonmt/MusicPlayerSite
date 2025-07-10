var builder = WebApplication.CreateBuilder(args);

// ✅ Registrar serviços do MVC
builder.Services.AddControllers();

// ✅ Configurar Kestrel (se necessário)
builder.WebHost.ConfigureKestrel(options =>
{
    // configurações de servidor
});

builder.Services.AddControllers(); // ✅ Necessário para MapControllers funcionar


var app = builder.Build();

// ✅ Mapear rotas de controllers
app.MapControllers(); // ou .MapDefaultControllerRoute() se tiver views Razor

app.Run();
