var builder = WebApplication.CreateBuilder(args);

// ? Aqui você pode registrar serviços
// builder.Services.AddSingleton<...>();

// ? Configurar Kestrel (se necessário)
builder.WebHost.ConfigureKestrel(options =>
{
    // configuração de portas, limites de requisição, etc
});

var app = builder.Build();

// configurar middlewares e rotas
app.MapControllers(); // ou MapDefaultControllerRoute()

app.Run();
