using APIProject.Application.DTOs;
using APIProject.Application.DTOs.Autenticacao;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Domain.Entidades;
using APIProject.Domain.Excecoes;
using APIProject.UnitTests.Common;
using FluentValidation.Results;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace APIProject.UnitTests.Handlers
{
    public class LoginUsuarioComandoHandlerTests : BaseCommandHandlerTests<LoginUsuarioComando, TokenDto>
    {
        private readonly LoginUsuarioComandoHandler _handler;

        public LoginUsuarioComandoHandlerTests() : base()
        {
            _handler = new LoginUsuarioComandoHandler(
                UnitOfWorkMock.Object,
                HashServiceMock.Object,
                TokenServiceMock.Object,
                UsuarioServicoMock.Object,
                ValidatorMock.Object);
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

            var tokenDto = new TokenDto { Token = "token_valido", RefreshToken = "refresh_token" };

            UsuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync(usuario);

            HashServiceMock.Setup(x => x.VerificarHash(comando.Senha, usuario.Senha))
                .Returns(true);

            TokenServiceMock.Setup(x => x.GerarToken(usuario))
                .Returns(tokenDto);

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.Equal(tokenDto, result);
            UsuarioServicoMock.Verify(x => x.RegistrarLogin(usuario), Times.Once);
            UnitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
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

            ValidatorMock.Setup(x => x.ValidateAsync(comando, It.IsAny<CancellationToken>()))
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

            UsuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync((Usuario?)null);

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

            UsuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
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

            UsuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync(usuario);

            HashServiceMock.Setup(x => x.VerificarHash(comando.Senha, usuario.Senha))
                .Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OperacaoNaoAutorizadaException>(
                () => _handler.Handle(comando, CancellationToken.None));

            Assert.Equal("Usuário ou senha inválidos", exception.Message);
        }
    }
}
