using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace APIProject.Infrastructure.Persistencia
{
    /// <summary>
    /// Factory para criar o DbContext durante as migrações.
    /// Necessário para o comando dotnet ef migrations
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Determinar o diretório raiz do projeto
            var projectDir = Directory.GetCurrentDirectory();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Encontrar o diretório da API de forma mais robusta
            string apiDir = EncontrarDiretorioAPI(projectDir);

            // Carregar configuração do appsettings.json
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(apiDir)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();

            // Verificar se deve usar banco de dados em memória
            var useInMemoryDb = configuration.GetValue<bool>("UseInMemoryDatabase", false) ||
                               environment.Equals("Testing", StringComparison.OrdinalIgnoreCase);

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            if (useInMemoryDb)
            {
                // Usar banco de dados em memória para ambiente de teste
                string dbName = $"APIProjectDb-{Guid.NewGuid()}";
                Console.WriteLine($"Usando banco de dados em memória: {dbName}");
                optionsBuilder.UseInMemoryDatabase(dbName);
            }
            else
            {
                // Usar SQL Server para outros ambientes
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Não foi possível encontrar a ConnectionString 'DefaultConnection' no arquivo appsettings.json");
                }

                // Log seguro da string de conexão (ocultando informações sensíveis)
                Console.WriteLine("Usando conexão SQL Server configurada em appsettings.json");

                optionsBuilder.UseSqlServer(connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    });
            }

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        /// <summary>
        /// Encontra o diretório da API de forma mais robusta
        /// </summary>
        private static string EncontrarDiretorioAPI(string diretorioAtual)
        {
            // Primeiro, tenta encontrar o diretório da API no mesmo nível
            string apiDir = Path.Combine(diretorioAtual, "..", "APIProject.API");
            if (Directory.Exists(apiDir))
                return apiDir;

            // Tenta subir um nível
            apiDir = Path.Combine(diretorioAtual, "..", "..", "APIProject.API");
            if (Directory.Exists(apiDir))
                return apiDir;

            // Tenta subir dois níveis (comum em projetos de teste)
            apiDir = Path.Combine(diretorioAtual, "..", "..", "..", "APIProject.API");
            if (Directory.Exists(apiDir))
                return apiDir;

            // Tenta encontrar o diretório da API em qualquer lugar na solução
            var diretorioSolucao = EncontrarDiretorioSolucao(diretorioAtual);
            if (diretorioSolucao != null)
            {
                apiDir = Path.Combine(diretorioSolucao, "APIProject.API");
                if (Directory.Exists(apiDir))
                    return apiDir;
            }

            // Se não encontrou, lança uma exceção
            throw new DirectoryNotFoundException(
                "Não foi possível encontrar o diretório da API. " +
                "Verifique se o projeto APIProject.API existe e está na estrutura esperada.");
        }

        /// <summary>
        /// Tenta encontrar o diretório raiz da solução
        /// </summary>
        private static string? EncontrarDiretorioSolucao(string diretorioInicial)
        {
            var diretorioAtual = new DirectoryInfo(diretorioInicial);

            // Procura o arquivo .sln subindo até 3 níveis
            for (int i = 0; i < 5; i++)
            {
                if (diretorioAtual == null)
                    return null;

                // Verifica se há arquivos .sln no diretório atual
                if (diretorioAtual.GetFiles("*.sln").Length > 0)
                    return diretorioAtual.FullName;

                // Sobe um nível
                diretorioAtual = diretorioAtual.Parent;
            }

            return null;
        }
    }
}
