using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using APIProject.Domain.Interfaces.Servicos;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace APIProject.Application.Usuarios.Comandos.LoginUsuario
{
    public class LoginUsuarioComandoHandler : IRequestHandler<LoginUsuarioComando, TokenDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHashService _hashService;
        private readonly ITokenService _tokenService;
        private readonly IUsuarioServico _usuarioServico;
        private readonly IValidator<LoginUsuarioComando> _validator;
        private readonly ICacheService _cacheService;
        private const string USER_LOGIN_ATTEMPT_KEY = "login:attempt:";
        private const string USER_FAILED_LOGIN_COUNT_KEY = "login:failed:count:";

        public LoginUsuarioComandoHandler(
            IUnitOfWork unitOfWork,
            IHashService hashService,
            ITokenService tokenService,
            IUsuarioServico usuarioServico,
            IValidator<LoginUsuarioComando> validator,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _hashService = hashService;
            _tokenService = tokenService;
            _usuarioServico = usuarioServico;
            _validator = validator;
            _cacheService = cacheService;
        }

        public async Task<TokenDto> Handle(LoginUsuarioComando request, CancellationToken cancellationToken)
        {            
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var erros = validationResult.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new ValidacaoException(erros);
            }

            // Verificar se há muitas tentativas de login para este e-mail
            string loginAttemptKey = $"{USER_LOGIN_ATTEMPT_KEY}{request.Email.ToLower()}";
            string failedCountKey = $"{USER_FAILED_LOGIN_COUNT_KEY}{request.Email.ToLower()}";
            
            // Verificamos se há muitas tentativas recentes
            var failedCountObj = await _cacheService.GetAsync<int?>(failedCountKey);
            int failedCount = failedCountObj ?? 0;
            
            if (failedCount >= 5)
            {
                // Verificar se a última tentativa foi recente
                var lastAttemptObj = await _cacheService.GetAsync<DateTime?>(loginAttemptKey);
                DateTime? lastAttempt = lastAttemptObj;
                
                if (lastAttempt.HasValue && DateTime.UtcNow.Subtract(lastAttempt.Value).TotalMinutes < 15)
                {
                    throw new OperacaoNaoAutorizadaException("Muitas tentativas de login. Tente novamente mais tarde.");
                }
                else
                {
                    // Reset do contador após o período de bloqueio
                    await _cacheService.RemoveAsync(failedCountKey);
                }
            }

            // Registrar tentativa de login
            await _cacheService.SetAsync(loginAttemptKey, DateTime.UtcNow, TimeSpan.FromHours(1));

            // Buscar o usuário pelo email
            var usuario = await _unitOfWork.Usuarios.ObterPorEmailAsync(request.Email);
            if (usuario == null)
            {
                // Incrementar contador de falhas
                await IncrementarContadorFalhasLogin(failedCountKey);
                throw new OperacaoNaoAutorizadaException("Usuário ou senha inválidos");
            }

            // Verificar se o usuário está ativo
            if (!usuario.Ativo)
            {
                // Incrementar contador de falhas
                await IncrementarContadorFalhasLogin(failedCountKey);
                throw new OperacaoNaoAutorizadaException("Usuário inativo");
            }

            // Verificar a senha
            if (!_hashService.VerificarHash(request.Senha, usuario.Senha!))
            {
                // Incrementar contador de falhas
                await IncrementarContadorFalhasLogin(failedCountKey);
                throw new OperacaoNaoAutorizadaException("Usuário ou senha inválidos");
            }

            // Reset do contador ao fazer login com sucesso
            await _cacheService.RemoveAsync(failedCountKey);

            // Invalidate all existing refresh tokens
            foreach (var token in usuario.RefreshTokens.Where(rt => rt.EstaAtivo))
            {
                token.Invalidado = true;
            }

            var tokenDto = _tokenService.GerarToken(usuario);

            usuario.AdicionarRefreshToken(
                tokenDto.RefreshToken,
                DateTime.UtcNow.AddDays(1)
            );

            _usuarioServico.RegistrarLogin(usuario);
            await _unitOfWork.CommitAsync();

            return tokenDto;
        }

        private async Task IncrementarContadorFalhasLogin(string key)
        {
            var failedCountObj = await _cacheService.GetAsync<int?>(key);
            int failedCount = failedCountObj ?? 0;
            failedCount++;
            
            // Armazenar por 1 hora para permitir reset após período de bloqueio
            await _cacheService.SetAsync(key, failedCount, TimeSpan.FromHours(1));
        }
    }
}
