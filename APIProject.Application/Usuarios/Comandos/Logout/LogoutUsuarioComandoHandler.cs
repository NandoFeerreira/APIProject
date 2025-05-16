// APIProject.Application/Usuarios/Comandos/Logout/LogoutUsuarioComandoHandler.cs
using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace APIProject.Application.Usuarios.Comandos.Logout
{
    public class LogoutUsuarioComandoHandler : IRequestHandler<LogoutUsuarioComando>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheService _cacheService;
        private const string USER_REFRESH_TOKENS_KEY = "user:refreshtokens:";

        public LogoutUsuarioComandoHandler(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _cacheService = cacheService;
        }

        public async Task Handle(LogoutUsuarioComando request, CancellationToken cancellationToken)
        {
            var usuario = await _unitOfWork.Usuarios.ObterPorIdComRefreshTokensAsync(request.UsuarioId)
                ?? throw new Exception("Usuário não encontrado");

            // Remover refresh tokens
            if (usuario.RefreshTokens.Any())
            {
                var tokensParaRemover = usuario.RefreshTokens.ToList();
                foreach (var token in tokensParaRemover)
                {
                    usuario.RefreshTokens.Remove(token);
                }
                
                // Invalidar cache de refresh tokens do usuário
                string cacheKey = $"{USER_REFRESH_TOKENS_KEY}{request.UsuarioId}";
                await _cacheService.RemoveAsync(cacheKey);
            }
            
            string? authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                string jwtToken = authHeader["Bearer ".Length..];

                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtSecurityToken = tokenHandler.ReadJwtToken(jwtToken);

                    var jti = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

                    if (!string.IsNullOrEmpty(jti))
                    {
                        // Criar objeto TokenInvalidado
                        var tokenInvalidado = new TokenInvalidado
                        {
                            Jti = jti,
                            Token = jwtToken,
                            UsuarioId = request.UsuarioId,
                            DataExpiracao = jwtSecurityToken.ValidTo
                        };
                        
                        // Adicionar à blacklist
                        await _unitOfWork.TokensInvalidados.AdicionarTokenInvalidadoAsync(tokenInvalidado);
                    }
                }
                catch
                {
                    // Falha ao processar o token, continuamos mesmo assim
                }
            }
            
            await _unitOfWork.TokensInvalidados.RemoverTokensExpiradosAsync(request.UsuarioId);

            await _unitOfWork.CommitAsync();
        }
    }
}
