using APIProject.Application.DTOs;
using APIProject.Application.DTOs.Autenticacao;
using APIProject.Domain.Entidades;
using System.Security.Claims;

namespace APIProject.Application.Interfaces
{
    /// <summary>
    /// Interface para o serviço de geração e validação de tokens JWT
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Gera um novo token JWT para o usuário
        /// </summary>
        /// <param name="usuario">Usuário para o qual o token será gerado</param>
        /// <returns>DTO com o token, refresh token e data de expiração</returns>
        TokenDto GerarToken(Usuario usuario);

        /// <summary>
        /// Gera um novo refresh token
        /// </summary>
        /// <returns>String contendo o refresh token</returns>
        string GerarRefreshToken();

        /// <summary>
        /// Obtém as claims de um token expirado
        /// </summary>
        /// <param name="token">Token JWT expirado</param>
        /// <returns>ClaimsPrincipal contendo as claims do token ou null se o token for inválido</returns>
        ClaimsPrincipal? ObterPrincipalDeTokenExpirado(string token);
    }
}