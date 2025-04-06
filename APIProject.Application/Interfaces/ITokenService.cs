using APIProject.Application.DTOs;
using APIProject.Domain.Entidades;
using System.Collections.Generic;
using System.Security.Claims;

namespace APIProject.Application.Interfaces
{
    public interface ITokenService
    {
        TokenDto GerarToken(Usuario usuario);
        string GerarRefreshToken();
        ClaimsPrincipal? ObterPrincipalDeTokenExpirado(string token);
    }
}