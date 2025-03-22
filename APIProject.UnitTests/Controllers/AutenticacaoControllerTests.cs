using APIProject.API.Controllers;
using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using Bogus;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace APIProject.UnitTests.Controllers
{
    public class AutenticacaoControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IHashService> _hashServiceMock;
        private readonly AutenticacaoController _controller;
        private readonly Faker _faker;

        public AutenticacaoControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _tokenServiceMock = new Mock<ITokenService>();
            _hashServiceMock = new Mock<IHashService>();
            _controller = new AutenticacaoController(_mediatorMock.Object);
            _faker = new Faker();
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
                Id = _faker.Random.Int(1, 100),
                Nome = _faker.Person.FullName,
                Email = loginDto.Email,
                Senha = _faker.Random.AlphaNumeric(10)
            };

            var tokenDto = new TokenDto
            {
                Token = "jwt-token",
                Expiracao = DateTime.UtcNow.AddHours(1)
            };

            _hashServiceMock.Setup(x => x.VerificarHash(loginDto.Senha, usuario.Senha))
                .Returns(true);

            _tokenServiceMock.Setup(x => x.GerarToken(usuario))
                .Returns(tokenDto);

            // Act
            var resultado = await _controller.Login(loginDto) as ActionResult<TokenDto>;

            // Assert
            Assert.NotNull(resultado);
            var objectResult = Assert.IsType<OkObjectResult>(resultado.Result);
            Assert.Equal(200, objectResult.StatusCode);
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

            _hashServiceMock.Setup(x => x.VerificarHash(loginDto.Senha, It.IsAny<string>()))
                .Returns(false);

            // Act
            var resultado = await _controller.Login(loginDto) as ActionResult<TokenDto>;

            // Assert
            Assert.NotNull(resultado.Result);
            var unauthorizedResult = Assert.IsType<UnauthorizedResult>(resultado.Result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
            Assert.IsType<UnauthorizedResult>(resultado.Result);
        }
    }
}