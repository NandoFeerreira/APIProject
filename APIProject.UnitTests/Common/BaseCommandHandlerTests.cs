﻿using APIProject.Application.Interfaces;
using APIProject.Domain.Interfaces;
using APIProject.Domain.Interfaces.Servicos;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using System.Threading;
using System.Threading.Tasks;

namespace APIProject.UnitTests.Common
{
    /// <summary>
    /// Classe base para testes de handlers de comandos
    /// </summary>
    public abstract class BaseCommandHandlerTests<TCommand, TResponse>
        where TCommand : IRequest<TResponse>
    {
        protected readonly Mock<IUnitOfWork> UnitOfWorkMock;
        protected readonly Mock<IUsuarioRepositorio> UsuarioRepositorioMock;
        protected readonly Mock<IHashService> HashServiceMock;
        protected readonly Mock<ITokenService> TokenServiceMock;
        protected readonly Mock<IUsuarioServico> UsuarioServicoMock;
        protected readonly Mock<IValidator<TCommand>> ValidatorMock;
        protected readonly Mock<IMapper> MapperMock;

        protected BaseCommandHandlerTests()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            UsuarioRepositorioMock = new Mock<IUsuarioRepositorio>();
            HashServiceMock = new Mock<IHashService>();
            TokenServiceMock = new Mock<ITokenService>();
            UsuarioServicoMock = new Mock<IUsuarioServico>();
            ValidatorMock = new Mock<IValidator<TCommand>>();
            MapperMock = new Mock<IMapper>();

            // Configurar o UnitOfWork para retornar o repositório de usuários mockado
            UnitOfWorkMock.Setup(uow => uow.Usuarios).Returns(UsuarioRepositorioMock.Object);

            // Configurar o validator para retornar sucesso por padrão
            ValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        /// <summary>
        /// Configura o validador para falhar com a mensagem de erro especificada
        /// </summary>
        protected void SetupValidatorToFail(string propertyName, string errorMessage)
        {
            var validationFailure = new ValidationFailure(propertyName, errorMessage);
            var validationResult = new ValidationResult(new[] { validationFailure });

            ValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);
        }
    }
}
