using APIProject.Application.DTOs;
using APIProject.Application.DTOs.Autenticacao;
using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using APIProject.Infrastructure.Configuracoes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APIProject.Infrastructure.Servicos
{
    /// <summary>
    /// Implementação do serviço de geração e validação de tokens JWT
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly JwtConfiguracoes _jwtConfiguracoes;
        private readonly ILogger<TokenService> _logger;

        /// <summary>
        /// Construtor do serviço de token
        /// </summary>
        /// <param name="jwtConfiguracoes">Configurações do JWT</param>
        /// <param name="logger">Serviço de log</param>
        public TokenService(
            IOptions<JwtConfiguracoes> jwtConfiguracoes,
            ILogger<TokenService> logger)
        {
            _jwtConfiguracoes = jwtConfiguracoes?.Value ?? throw new ArgumentNullException(nameof(jwtConfiguracoes));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Construtor para testes
        /// </summary>
        /// <param name="jwtConfiguracoes">Configurações do JWT</param>
        public TokenService(IOptions<JwtConfiguracoes> jwtConfiguracoes)
        {
            _jwtConfiguracoes = jwtConfiguracoes?.Value ?? throw new ArgumentNullException(nameof(jwtConfiguracoes));
            _logger = new LoggerFactory().CreateLogger<TokenService>();
        }

        /// <summary>
        /// Gera um novo token JWT para o usuário
        /// </summary>
        /// <param name="usuario">Usuário para o qual o token será gerado</param>
        /// <returns>DTO com o token, refresh token e data de expiração</returns>
        public TokenDto GerarToken(Usuario usuario)
        {
            if (usuario == null)
            {
                throw new ArgumentNullException(nameof(usuario));
            }

            try
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Name, usuario.Nome ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguracoes.Chave));
                var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
                var expiracao = DateTime.UtcNow.AddMinutes(_jwtConfiguracoes.ExpiracaoMinutos);

                var token = new JwtSecurityToken(
                    issuer: _jwtConfiguracoes.Emissor,
                    audience: _jwtConfiguracoes.Audiencia,
                    claims: claims,
                    expires: expiracao,
                    signingCredentials: credenciais
                );

                var refreshToken = GerarRefreshToken();

                return new TokenDto
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiracao = expiracao
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar token para o usuário {UsuarioId}", usuario.Id);
                throw;
            }
        }

        /// <summary>
        /// Gera um novo refresh token
        /// </summary>
        /// <returns>String contendo o refresh token</returns>
        public string GerarRefreshToken()
        {
            try
            {
                var randomNumber = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar refresh token");
                throw;
            }
        }

        /// <summary>
        /// Obtém as claims de um token expirado
        /// </summary>
        /// <param name="token">Token JWT expirado</param>
        /// <returns>ClaimsPrincipal contendo as claims do token ou null se o token for inválido</returns>
        public ClaimsPrincipal? ObterPrincipalDeTokenExpirado(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguracoes.Chave)),
                    ValidateLifetime = false
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao validar token expirado");
                return null;
            }
        }
    }
}
