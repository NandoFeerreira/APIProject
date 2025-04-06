using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace APIProject.Infrastructure.Persistencia
{
    public static class BancoDadosInicializador
    {
        /// <summary>
        /// Inicializa o banco de dados, aplicando migrações pendentes se necessário
        /// </summary>
        public static async Task InicializarAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
            var context = services.GetRequiredService<ApplicationDbContext>();

            try
            {
                logger.LogInformation("Inicializando o banco de dados...");

                if (context.Database.IsSqlServer())
                {
                    logger.LogInformation("Aplicando migrações...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrações aplicadas com sucesso.");
                }
                else if (context.Database.IsInMemory())
                {
                    logger.LogInformation("Usando banco de dados em memória. Criando estrutura...");
                    await context.Database.EnsureCreatedAsync();
                    logger.LogInformation("Estrutura do banco em memória criada com sucesso.");
                }
                else
                {
                    logger.LogInformation("Verificando se o banco de dados existe...");
                    await context.Database.EnsureCreatedAsync();
                    logger.LogInformation("Banco de dados verificado com sucesso.");
                }
               
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocorreu um erro ao inicializar o banco de dados");
                throw;
            }
        }

        /// <summary>
        /// Apenas verifica se o banco de dados existe, sem aplicar migrações
        /// </summary>
        public static async Task VerificarBancoExisteAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
            var context = services.GetRequiredService<ApplicationDbContext>();

            try
            {
                logger.LogInformation("Verificando se o banco de dados existe...");

                bool existe = await context.Database.CanConnectAsync();

                if (existe)
                {
                    logger.LogInformation("Conexão com o banco de dados estabelecida com sucesso.");
                   
                    bool temMigracoesPendentes = (await context.Database.GetPendingMigrationsAsync()).Any();

                    if (temMigracoesPendentes)
                    {
                        logger.LogWarning("Há migrações pendentes que precisam ser aplicadas ao banco de dados.");
                    }
                }
                else
                {
                    logger.LogCritical("Não foi possível conectar ao banco de dados!");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocorreu um erro ao verificar o banco de dados");
                throw;
            }
        }
    }
}
