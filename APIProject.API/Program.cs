using APIProject.API.Extensions;
using APIProject.Infrastructure.Configuracoes;
using APIProject.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace APIProject.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

// Configurar ambiente de teste se necessário
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: true);
    
    // Configurar para usar banco de dados em memória nos testes
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("TestDb")
               .EnableServiceProviderCaching(false));
}

builder.Services.AddControllers();

// Adiciona serviços da aplicação
builder.Services.AddApplicationServices(builder.Configuration);

// Configurar autenticação e autorização
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtConfiguracoes = builder.Configuration.GetSection("JwtConfiguracoes").Get<JwtConfiguracoes>();
        var chave = Encoding.ASCII.GetBytes(jwtConfiguracoes.Chave);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(chave),
            ValidateIssuer = true,
            ValidIssuer = jwtConfiguracoes.Emissor,
            ValidateAudience = true,
            ValidAudience = jwtConfiguracoes.Audiencia,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

//await SeedData.InicializarBancoDeDados(app);


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Project v1");
        c.RoutePrefix = string.Empty;
    });
}

// Adicionar middleware de exceção
app.UseExceptionMiddleware();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Mapear os controladores
app.MapControllers();



        app.Run();
    }
}
