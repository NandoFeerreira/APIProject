
using APIProject.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace APIProject.Infrastructure.Servicos
{
    public class TokenLimpezaBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenLimpezaBackgroundService> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(12);

        public TokenLimpezaBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<TokenLimpezaBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de limpeza de tokens iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    await unitOfWork.TokensInvalidados.RemoverTodosTokensExpiradosAsync();
                    await unitOfWork.CommitAsync();

                    _logger.LogInformation("Limpeza de tokens expirados concluída");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao limpar tokens expirados");
                }

                await Task.Delay(_intervalo, stoppingToken);
            }
        }
    }
}