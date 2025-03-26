using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

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

            // Voltar para o diretório da API, onde está o appsettings.json
            string apiDir = Path.Combine(projectDir, "..", "APIProject.API");
            if (!Directory.Exists(apiDir))
            {
                apiDir = Path.Combine(projectDir, "..", "..", "..", "APIProject.API");
                if (!Directory.Exists(apiDir))
                {
                    throw new DirectoryNotFoundException(
                        $"Não foi possível encontrar o diretório da API em {apiDir}. Verifique se o arquivo appsettings.json existe.");
                }
            }

            // Carregar configuração do appsettings.json
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(apiDir)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Não foi possível encontrar a ConnectionString 'DefaultConnection' no arquivo appsettings.json");
            }

            Console.WriteLine($"ConnectionString: {connectionString}");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                });

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
