using APIProject.Application.DTOs;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace APIProject.IntegrationTests.Controllers
{
    public class UsuarioControllerTests
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public UsuarioControllerTests()
        {
            _factory = new CustomWebApplicationFactory("TestDb-Usuario");
            _client = _factory.CreateClient();
            _factory.SeedTestData();
        }

        [Fact]
        public async Task Login_ComCredenciaisValidas_RetornaToken()
        {
            // Arrange
            var loginCommand = new LoginUsuarioComando
            {
                Email = "teste@teste.com",
                Senha = "senha123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/usuarios/login", loginCommand);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenDto>();
            Assert.NotNull(tokenResponse);
            Assert.NotEmpty(tokenResponse.Token);
        }

        [Fact]
        public async Task Login_ComCredenciaisInvalidas_RetornaUnauthorized()
        {
            // Arrange
            var loginCommand = new LoginUsuarioComando
            {
                Email = "teste@teste.com",
                Senha = "senhaErrada"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/usuarios/login", loginCommand);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
