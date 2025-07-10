var builder = WebApplication.CreateBuilder(args);

// ? Aqui voc� pode registrar servi�os
// builder.Services.AddSingleton<...>();

// ? Configurar Kestrel (se necess�rio)
builder.WebHost.ConfigureKestrel(options =>
{
    // configura��o de portas, limites de requisi��o, etc
});

var app = builder.Build();

// configurar middlewares e rotas
app.MapControllers(); // ou MapDefaultControllerRoute()

app.Run();
