using APIProject.API.Extensions;
using APIProject.Application;
using APIProject.Infrastructure.Persistencia;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);

// Adicionar autenticação
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Updated MediatR registration for version 12.4.1
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    // Add other assemblies if needed, like Application assembly
    cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly);
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Inicializar banco de dados
await SeedData.InicializarBancoDeDados(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Adicionar middleware de exceção
app.UseExceptionMiddleware();

app.UseHttpsRedirection();

// Adicionar middleware de autenticação
app.UseAuthentication();
app.UseAuthorization();

// Mapear os controladores
app.MapControllers();



app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
