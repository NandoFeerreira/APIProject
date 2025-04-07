using APIProject.API.Extensions;
using APIProject.API.Middlewares;
using APIProject.Infrastructure.DependencyInjection;
using APIProject.Infrastructure.Persistencia;
using FluentValidation;
using Microsoft.OpenApi.Models;

namespace APIProject.API;
      

// Necessário para testes de integração
public partial class Program { }

// Implementação principal da classe Program
public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Configuração da API
        builder.Services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddRouting(options => options.LowercaseUrls = true);

        // 2. Configuração de CORS (unifique em um único lugar)
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Development", policy =>
            {
                policy.WithOrigins("http://localhost:5135", "https://localhost:7027")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        // 3. Configuração de Swagger
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Project", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Digite 'Bearer' seguido de espaço e o token JWT"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                In = ParameterLocation.Header
                            },
                            new List<string>()
                        }
            });
            });
        }

        // 4. Configurações de serviços da aplicação
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        var app = builder.Build();

        // 5. Verificação e Migração de Banco de Dados
        if (!app.Environment.IsEnvironment("Testing"))
        {
            bool aplicarMigracoesAutomaticamente = builder.Configuration.GetValue<bool>(
                "AplicarMigracoesAutomaticamente", app.Environment.IsDevelopment());

            if (aplicarMigracoesAutomaticamente)
            {
                await BancoDadosInicializador.InicializarAsync(app.Services);
                app.Logger.LogInformation("Migrações aplicadas automaticamente");
            }
            else
            {
                await BancoDadosInicializador.VerificarBancoExisteAsync(app.Services);
                app.Logger.LogInformation("Banco de dados verificado sem aplicar migrações automaticamente");
            }
        }

        // 6. Configuração do pipeline na ordem correta
        app.UseTratamentoExcecoes();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Project v1");
                c.RoutePrefix = "swagger";
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.ConfigObject.AdditionalItems.Add("persistAuthorization", true);
                c.ConfigObject.AdditionalItems.Add("tryItOutEnabled", true);
            });
          
            app.Use(async (context, next) =>
            {
                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.Headers.Append("Access-Control-Allow-Origin", context.Request.Headers.Origin);
                    context.Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization");
                    context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                    context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
                    context.Response.StatusCode = 200;
                    await context.Response.CompleteAsync();
                    return;
                }
                await next();
            });
        }

        // Ordem correta dos middlewares
        app.UseRouting();
        app.UseCors("Development");  // Use a política nomeada
                                     // app.UseHttpsRedirection(); // Descomentado em produção
        app.UseAuthentication();
        app.UseExceptionMiddleware();
        app.UseAuthorization();
        app.MapControllers();

        await app.RunAsync();
    }
}

