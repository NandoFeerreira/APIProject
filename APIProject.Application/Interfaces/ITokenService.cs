using APIProject.Application.DTOs;
using APIProject.Domain.Entidades;
using System.Collections.Generic;

namespace APIProject.Application.Interfaces
{
    public interface ITokenService
    {
        TokenDto GerarToken(Usuario usuario);
    }
}