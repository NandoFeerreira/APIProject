﻿using APIProject.Domain.Excecoes;
using FluentValidation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APIProject.Application.Common
{
    /// <summary>
    /// Classe base para handlers de comandos que inclui validação
    /// </summary>
    /// <typeparam name="TCommand">Tipo do comando</typeparam>
    /// <typeparam name="TValidator">Tipo do validador</typeparam>
    public abstract class BaseCommandHandler<TCommand, TValidator>
        where TValidator : IValidator<TCommand>
    {
        protected readonly TValidator _validator;

        protected BaseCommandHandler(TValidator validator)
        {
            _validator = validator;
        }

        /// <summary>
        /// Valida o comando usando o validador fornecido
        /// </summary>
        /// <param name="command">Comando a ser validado</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <exception cref="ValidacaoException">Lançada quando o comando é inválido</exception>
        protected async Task ValidateCommand(TCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            
            if (!validationResult.IsValid)
            {
                var erros = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new ValidacaoException(erros);
            }
        }
    }
}
