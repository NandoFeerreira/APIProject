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
        /// Verifica se o banco de dados existe e se há migrações pendentes
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

                    // Verificar migrações pendentes
                    var migracoesPendentes = await context.Database.GetPendingMigrationsAsync();
                    bool temMigracoesPendentes = migracoesPendentes.Any();

                    if (temMigracoesPendentes)
                    {
                        logger.LogWarning("Há migrações pendentes que precisam ser aplicadas ao banco de dados:");
                        foreach (var migracao in migracoesPendentes)
                        {
                            logger.LogWarning("- {MigracaoNome}", migracao);
                        }
                        logger.LogWarning("Execute 'dotnet ef database update' para aplicar as migrações.");
                    }

                    // Verificar se há alterações no modelo que precisam de novas migrations
                    bool temAlteracoesModelo = false;
                    try
                    {
                        // Verificar se há diferença entre as migrações aplicadas e as disponíveis
                        var migracoesAplicadas = context.Database.GetAppliedMigrations().ToList();
                        var migracoesDisponiveis = context.Database.GetMigrations().ToList();

                        // Se há migrações disponíveis que não foram aplicadas
                        temAlteracoesModelo = migracoesDisponiveis.Except(migracoesAplicadas).Any();
                    }
                    catch (Exception ex)
                    {
                        // Ignora erros ao verificar alterações no modelo
                        logger.LogWarning("Erro ao verificar alterações no modelo: {Mensagem}", ex.Message);
                    }

                    if (temAlteracoesModelo)
                    {
                        logger.LogWarning("Há alterações no modelo que precisam de novas migrations.");
                        logger.LogWarning("Execute 'dotnet ef migrations add NomeDaMigracao' para criar uma nova migration.");
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
