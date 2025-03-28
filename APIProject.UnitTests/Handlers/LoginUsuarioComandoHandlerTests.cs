using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Domain.Entidades;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using APIProject.Domain.Interfaces.Servicos;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace APIProject.UnitTests.Application.Usuarios.Comandos.LoginUsuario
{
    public class LoginUsuarioComandoHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUsuarioRepositorio> _usuarioRepositorioMock;
        private readonly Mock<IHashService> _hashServiceMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IUsuarioServico> _usuarioServicoMock;
        private readonly Mock<IValidator<LoginUsuarioComando>> _validatorMock;
        private readonly LoginUsuarioComandoHandler _handler;

        public LoginUsuarioComandoHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _usuarioRepositorioMock = new Mock<IUsuarioRepositorio>();
            _hashServiceMock = new Mock<IHashService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _usuarioServicoMock = new Mock<IUsuarioServico>();
            _validatorMock = new Mock<IValidator<LoginUsuarioComando>>();

            // Configurar o UnitOfWork para retornar o repositório de usuários mockado
            _unitOfWorkMock.Setup(uow => uow.Usuarios).Returns(_usuarioRepositorioMock.Object);

            // Configurar o validator para retornar sucesso por padrão
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<LoginUsuarioComando>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _handler = new LoginUsuarioComandoHandler(
                _unitOfWorkMock.Object,
                _hashServiceMock.Object,
                _tokenServiceMock.Object,
                _usuarioServicoMock.Object,
                _validatorMock.Object);
        }

        [Fact]
        public async Task Handle_ComCredenciaisValidas_RetornaToken()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = "teste@teste.com",
                Senha = "senha123"
            };

            var usuario = new Usuario("Teste", "teste@teste.com", "hash");
            usuario.Ativo = true;

            var tokenDto = new TokenDto { Token = "token_valido" };

            _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync(usuario);

            _hashServiceMock.Setup(x => x.VerificarHash(comando.Senha, usuario.Senha))
                .Returns(true);

            _tokenServiceMock.Setup(x => x.GerarToken(usuario))
                .Returns(tokenDto);

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.Equal(tokenDto, result);
            _usuarioServicoMock.Verify(x => x.RegistrarLogin(usuario), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ComValidacaoInvalida_LancaValidacaoException()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = "email_invalido",
                Senha = ""
            };

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Email", "Email inválido"),
                new ValidationFailure("Senha", "Senha é obrigatória")
            };

            _validatorMock.Setup(x => x.ValidateAsync(comando, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidacaoException>(
                () => _handler.Handle(comando, CancellationToken.None));

            Assert.Contains("Email", exception.Erros.Keys);
            Assert.Contains("Senha", exception.Erros.Keys);
        }

        [Fact]
        public async Task Handle_ComUsuarioInexistente_LancaOperacaoNaoAutorizadaException()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = "inexistente@teste.com",
                Senha = "senha123"
            };

            _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync((Usuario)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OperacaoNaoAutorizadaException>(
                () => _handler.Handle(comando, CancellationToken.None));

            Assert.Equal("Usuário ou senha inválidos", exception.Message);
        }

        [Fact]
        public async Task Handle_ComUsuarioInativo_LancaOperacaoNaoAutorizadaException()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = "teste@teste.com",
                Senha = "senha123"
            };

            var usuario = new Usuario("Teste", "teste@teste.com", "hash");
            usuario.Ativo = false;

            _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync(usuario);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OperacaoNaoAutorizadaException>(
                () => _handler.Handle(comando, CancellationToken.None));

            Assert.Equal("Usuário inativo", exception.Message);
        }

        [Fact]
        public async Task Handle_ComSenhaInvalida_LancaOperacaoNaoAutorizadaException()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = "teste@teste.com",
                Senha = "senha_errada"
            };

            var usuario = new Usuario("Teste", "teste@teste.com", "hash");
            usuario.Ativo = true;

            _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync(usuario);

            _hashServiceMock.Setup(x => x.VerificarHash(comando.Senha, usuario.Senha))
                .Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OperacaoNaoAutorizadaException>(
                () => _handler.Handle(comando, CancellationToken.None));

            Assert.Equal("Usuário ou senha inválidos", exception.Message);
        }
    }
}
