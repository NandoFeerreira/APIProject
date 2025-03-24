using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using APIProject.Domain.Interfaces.Servicos;
using Bogus;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace APIProject.UnitTests.Handlers
{
    public class LoginUsuarioComandoHandlerTests
    {
        private readonly Mock<IUsuarioRepositorio> _usuarioRepositorioMock;
        private readonly Mock<IHashService> _hashServiceMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IUsuarioServico> _usuarioServicoMock;
        private readonly LoginUsuarioComandoHandler _handler;
        private readonly Faker _faker;

        public LoginUsuarioComandoHandlerTests()
        {
            _usuarioRepositorioMock = new Mock<IUsuarioRepositorio>();
            _hashServiceMock = new Mock<IHashService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _usuarioServicoMock = new Mock<IUsuarioServico>();
            _handler = new LoginUsuarioComandoHandler(
                _usuarioRepositorioMock.Object,
                _hashServiceMock.Object,
                _tokenServiceMock.Object,
                _usuarioServicoMock.Object);
            _faker = new Faker();
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

            var usuario = new Usuario(_faker.Person.FullName, comando.Email, _faker.Random.AlphaNumeric(10));

            var tokenEsperado = new TokenDto
            {
                Token = "jwt-token",
                Expiracao = DateTime.UtcNow.AddHours(1)
            };

            _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync(usuario);

            _hashServiceMock.Setup(x => x.VerificarHash(comando.Senha, usuario.Senha))
                .Returns(true);

            _tokenServiceMock.Setup(x => x.GerarToken(usuario))
                .Returns(tokenEsperado);

            // Act
            var resultado = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(tokenEsperado.Token, resultado.Token);
            Assert.Equal(tokenEsperado.Expiracao, resultado.Expiracao);

            _usuarioRepositorioMock.Verify(x => x.ObterPorEmailAsync(comando.Email), Times.Once);
            _hashServiceMock.Verify(x => x.VerificarHash(comando.Senha, usuario.Senha), Times.Once);
            _tokenServiceMock.Verify(x => x.GerarToken(usuario), Times.Once);
        }

        [Fact]
        public async Task Handle_ComUsuarioInexistente_LancaExcecao()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = "inexistente@teste.com",
                Senha = "senha123"
            };

            _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync((Usuario?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _handler.Handle(comando, CancellationToken.None));

            Assert.Equal("Usuário ou senha inválidos", exception.Message);
            _usuarioRepositorioMock.Verify(x => x.ObterPorEmailAsync(comando.Email), Times.Once);
        }

        [Fact]
        public async Task Handle_ComUsuarioInativo_LancaExcecao()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = "inativo@teste.com",
                Senha = "senha123"
            };

            var usuario = new Usuario(_faker.Person.FullName, comando.Email, _faker.Random.AlphaNumeric(10));
            usuario.Desativar();

            _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync(usuario);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _handler.Handle(comando, CancellationToken.None));

            Assert.Equal("Usuário inativo", exception.Message);
            _usuarioRepositorioMock.Verify(x => x.ObterPorEmailAsync(comando.Email), Times.Once);
        }

        [Fact]
        public async Task Handle_ComSenhaInvalida_LancaExcecao()
        {
            // Arrange
            var comando = new LoginUsuarioComando
            {
                Email = "teste@teste.com",
                Senha = "senhaerrada"
            };

            var usuario = new Usuario(_faker.Person.FullName, comando.Email, _faker.Random.AlphaNumeric(10));

            _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(comando.Email))
                .ReturnsAsync(usuario);

            _hashServiceMock.Setup(x => x.VerificarHash(comando.Senha, usuario.Senha))
                .Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _handler.Handle(comando, CancellationToken.None));

            Assert.Equal("Usuário ou senha inválidos", exception.Message);
            _usuarioRepositorioMock.Verify(x => x.ObterPorEmailAsync(comando.Email), Times.Once);
            _hashServiceMock.Verify(x => x.VerificarHash(comando.Senha, usuario.Senha), Times.Once);
        }
    }
}