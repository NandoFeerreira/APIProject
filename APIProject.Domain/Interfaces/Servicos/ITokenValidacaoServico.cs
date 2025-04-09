﻿using System.Threading.Tasks;

namespace APIProject.Domain.Interfaces.Servicos
{
    /// <summary>
    /// Interface para o serviço de validação de tokens
    /// </summary>
    public interface ITokenValidacaoServico
    {
        /// <summary>
        /// Verifica se um token JWT é válido
        /// </summary>
        /// <param name="token">O token JWT a ser validado</param>
        /// <returns>True se o token for válido, False caso contrário</returns>
        Task<(bool EstaValido, string? MensagemErro)> ValidarTokenAsync(string token);
    }
}
