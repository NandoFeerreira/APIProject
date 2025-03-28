using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using APIProject.Domain.Entidades;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace APIProject.UnitTests.Application.Usuarios.Comandos.RegistrarUsuario
{
    public class RegistrarUsuarioComandoHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUsuarioRepositorio> _usuarioRepositorioMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IHashService> _hashServiceMock;
        private readonly Mock<IValidator<RegistrarUsuarioComando>> _validatorMock;
        private readonly RegistrarUsuarioComandoHandler _handler;

        public RegistrarUsuarioComandoHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _usuarioRepositorioMock = new Mock<IUsuarioRepositorio>();
            _mapperMock = new Mock<IMapper>();
            _hashServiceMock = new Mock<IHashService>();
            _validatorMock = new Mock<IValidator<RegistrarUsuarioComando>>();

            // Configurar o UnitOfWork para retornar o repositório de usuários mockado
            _unitOfWorkMock.Setup(uow => uow.Usuarios).Returns(_usuarioRepositorioMock.Object);

            // Configurar o validator para retornar sucesso por padrão
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<RegistrarUsuarioComando>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _handler = new RegistrarUsuarioComandoHandler(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _hashServiceMock.Object,
                _validatorMock.Object);
        }

        [Fact]
        public async Task Handle_ComDadosValidos_RetornaUsuarioCriado()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Novo Usuário",
                Email = "novo@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            var senhaHash = "senha_hash";
            var usuario = new Usuario(comando.Nome, comando.Email, senhaHash);
            var usuarioDto = new UsuarioDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email
            };

            _usuarioRepositorioMock.Setup(x => x.EmailExisteAsync(comando.Email))
                .ReturnsAsync(false);

            _hashServiceMock.Setup(x => x.CriarHash(comando.Senha))
                .Returns(senhaHash);

            _mapperMock.Setup(x => x.Map<UsuarioDto>(It.IsAny<Usuario>()))
                .Returns(usuarioDto);

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.Equal(usuarioDto, result);
            _usuarioRepositorioMock.Verify(x => x.AdicionarAsync(It.IsAny<Usuario>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ComValidacaoInvalida_LancaValidacaoException()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "",
                Email = "email_invalido",
                Senha = "123",
                ConfirmacaoSenha = "456"
            };

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Nome", "Nome é obrigatório"),
                new ValidationFailure("Email", "Email inválido"),
                new ValidationFailure("Senha", "Senha deve ter pelo menos 6 caracteres"),
                new ValidationFailure("ConfirmacaoSenha", "Senhas não conferem")
            };

            _validatorMock.Setup(x => x.ValidateAsync(comando, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidacaoException>(
                () => _handler.Handle(comando, CancellationToken.None));

            Assert.Contains("Nome", exception.Erros.Keys);
            Assert.Contains("Email", exception.Erros.Keys);
            Assert.Contains("Senha", exception.Erros.Keys);
            Assert.Contains("ConfirmacaoSenha", exception.Erros.Keys);
        }

        [Fact]
        public async Task Handle_ComEmailJaExistente_LancaDadosDuplicadosException()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Usuário Existente",
                Email = "existente@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            _usuarioRepositorioMock.Setup(x => x.EmailExisteAsync(comando.Email))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DadosDuplicadosException>(
                () => _handler.Handle(comando, CancellationToken.None));

            Assert.Equal($"Já existe um(a) Usuário com email '{comando.Email}'.", exception.Message);
        }
    }
}
