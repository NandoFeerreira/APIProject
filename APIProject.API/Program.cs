using APIProject.API.Extensions;
using APIProject.Infrastructure.DependencyInjection;
using APIProject.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using APIProject.API.Middlewares;

namespace APIProject.API;

// Necessário para testes de integração
public partial class Program { }

// Implementação principal da classe Program
public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configurar serviços
        builder.Services.AddControllers();

        // Adicionar serviços da aplicação e infraestrutura
        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);

        // Configurar Swagger para ambiente não-teste
        if (!builder.Environment.IsEnvironment("Testing"))
        {
           API.Extensions.ServiceCollectionExtensions.AddOpenApi(builder.Services);
        }

        // Construir o app
        var app = builder.Build();

        // Configurar pipeline de requisição
        // Configurar migração do banco de dados apenas para ambientes não-teste
        if (!app.Environment.IsEnvironment("Testing"))
        {
            // Aplicar migrações
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();

                // Verifica se não estamos usando banco em memória antes de migrar
                if (!context.Database.IsInMemory())
                {
                    context.Database.Migrate();
                }
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

        // Middleware comum a todos os ambientes
        app.UseTratamentoExcecoes();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}

