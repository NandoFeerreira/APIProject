using APIProject.API.Extensions;
using APIProject.Infrastructure.DependencyInjection;
using APIProject.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using APIProject.API.Middlewares;
using APIProject.Domain.Interfaces;
using APIProject.Infrastructure.Persistencia.Repositorios;
using FluentValidation;

namespace APIProject.API;

// Necessário para testes de integração
public partial class Program { }

// Implementação principal da classe Program
public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configurar serviços
        builder.Services.AddControllers();

        // Adicionar serviços da aplicação e infraestrutura
        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);


        // Configurar Swagger para ambiente não-teste
        if (!builder.Environment.IsEnvironment("Testing"))
        {
            API.Extensions.ServiceCollectionExtensions.AddOpenApi(builder.Services);
        }

        // Construir o app
        var app = builder.Build();

        // Configurar migração do banco de dados
        if (!app.Environment.IsEnvironment("Testing"))
        {
            // Verificar se deve aplicar migrações automaticamente
            bool aplicarMigracoesAutomaticamente = builder.Configuration.GetValue<bool>("AplicarMigracoesAutomaticamente",
                // Por padrão, aplicar automaticamente apenas em ambiente de desenvolvimento
                app.Environment.IsDevelopment());

            if (aplicarMigracoesAutomaticamente)
            {
                // Aplicar migrações automaticamente
                await BancoDadosInicializador.InicializarAsync(app.Services);
                app.Logger.LogInformation("Migrações aplicadas automaticamente");
            }
            else
            {
                // Apenas verificar se o banco existe, sem aplicar migrações
                // Útil para produção onde as migrações devem ser controladas
                await BancoDadosInicializador.VerificarBancoExisteAsync(app.Services);
                app.Logger.LogInformation("Banco de dados verificado sem aplicar migrações automaticamente");
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Project v1");
                    c.RoutePrefix = "swagger";
                });
            }
        }

        app.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.UseTratamentoExcecoes();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
