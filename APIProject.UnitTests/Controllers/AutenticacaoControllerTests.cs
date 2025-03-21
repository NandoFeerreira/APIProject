using APIProject.API.Controllers;
using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace APIProject.UnitTests.Controllers
{
    public class AutenticacaoControllerTests
    {
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IHashService> _hashServiceMock;
        private readonly AutenticacaoController _controller;

        public AutenticacaoControllerTests()
        {
            _tokenServiceMock = new Mock<ITokenService>();
            _hashServiceMock = new Mock<IHashService>();
            _controller = new AutenticacaoController(_tokenServiceMock.Object, _hashServiceMock.Object);
        }

        [Fact]
        public async Task Login_ComCredenciaisValidas_RetornaToken()
        {
            // Arrange
            var loginDto = new LoginUsuarioDto
            {
                Email = "teste@teste.com",
                Senha = "senha123"
            };

            var usuario = new Usuario
            {
                Id = 1,
                Nome = "Teste",
                Email = loginDto.Email,
                Senha = "hashedPassword"
            };

            var tokenDto = new TokenDto
            {
                Token = "jwt-token",
                Expiracao = DateTime.UtcNow.AddHours(1)
            };

            _hashServiceMock.Setup(x => x.VerificarSenha(loginDto.Senha, usuario.Senha))
                .Returns(true);

            _tokenServiceMock.Setup(x => x.GerarToken(usuario))
                .Returns(tokenDto);

            // Act
            var resultado = await _controller.Login(loginDto) as OkObjectResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(200, resultado.StatusCode);
            Assert.IsType<TokenDto>(resultado.Value);
            var token = resultado.Value as TokenDto;
            Assert.Equal(tokenDto.Token, token.Token);
        }

        [Fact]
        public async Task Login_ComCredenciaisInvalidas_RetornaUnauthorized()
        {
            // Arrange
            var loginDto = new LoginUsuarioDto
            {
                Email = "teste@teste.com",
                Senha = "senhaerrada"
            };

            _hashServiceMock.Setup(x => x.VerificarSenha(loginDto.Senha, It.IsAny<string>()))
                .Returns(false);

            // Act
            var resultado = await _controller.Login(loginDto) as UnauthorizedResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(401, resultado.StatusCode);
        }
    }
}