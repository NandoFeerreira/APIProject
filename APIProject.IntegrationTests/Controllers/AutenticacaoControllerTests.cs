using System.Net;
using System.Net.Http.Json;
using APIProject.Application.DTOs;
using Xunit;

namespace APIProject.IntegrationTests.Controllers
{
    public class AutenticacaoControllerTests : IntegrationTestBase
    {
        [Fact]
        public async Task Login_ComCredenciaisValidas_RetornaToken()
        {
            // Arrange
            var loginDto = new LoginUsuarioDto
            {
                Email = "teste@teste.com",
                Senha = "senha123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/autenticacao/login", loginDto);
            var tokenDto = await response.Content.ReadFromJsonAsync<TokenDto>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(tokenDto);
            Assert.NotNull(tokenDto.Token);
            Assert.NotEqual(default, tokenDto.Expiracao);
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

            // Act
            var response = await _client.PostAsJsonAsync("/api/autenticacao/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_ComModeloInvalido_RetornaBadRequest()
        {
            // Arrange
            var loginDto = new LoginUsuarioDto
            {
                Email = "",
                Senha = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/autenticacao/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}