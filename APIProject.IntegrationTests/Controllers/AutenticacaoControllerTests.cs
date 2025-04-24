using APIProject.Application.DTOs.Autenticacao;
using APIProject.Application.DTOs.Usuarios;
using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using APIProject.Infrastructure.Persistencia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace APIProject.IntegrationTests.Controllers
{
    public class AutenticacaoControllerTests : IntegrationTestBase
    {
        private readonly ILogger<AutenticacaoControllerTests> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        
        public AutenticacaoControllerTests(TestFixture fixture) : base(fixture)
        {
            _logger = _fixture.Factory.Services.GetRequiredService<ILogger<AutenticacaoControllerTests>>();
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            ResetarBancoDados();
        }

        // [Fact]
        public async Task Login_ComCredenciaisValidas_RetornaToken()
        {
            // Arrange - Criar usuário diretamente no banco
            using var scope = _fixture.Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hashService = scope.ServiceProvider.GetRequiredService<IHashService>();
            
            // Limpar usuários existentes
            var usuariosExistentes = context.Usuarios.Where(u => u.Email == "teste@teste.com").ToList();
            if (usuariosExistentes.Any())
            {
                context.Usuarios.RemoveRange(usuariosExistentes);
                context.SaveChanges();
            }

            // Criar um novo usuário
            var senha = "senha123";
            var senhaCriptografada = hashService.CriarHash(senha);
            var usuario = new Usuario("Usuário Teste", "teste@teste.com", senhaCriptografada);
            usuario.Ativo = true;
            
            context.Usuarios.Add(usuario);
            context.SaveChanges();
            
            _logger.LogInformation("Usuário criado: {Id}, {Email}, Hash={Hash}", 
                usuario.Id, usuario.Email, senhaCriptografada);

            // Act - Testar login
            var loginDto = new LoginRequestDto
            {
                Email = "teste@teste.com",
                Senha = senha
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginDto, _jsonOptions), 
                Encoding.UTF8, 
                "application/json");

            var response = await _client.PostAsync("/api/autenticacao/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("Status: {Status}, Resposta: {Content}", 
                response.StatusCode, responseContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = JsonSerializer.Deserialize<TokenResponseDto>(responseContent, _jsonOptions);
                Assert.NotNull(tokenResponse);
                Assert.NotEmpty(tokenResponse.AccessToken);
                Assert.NotEmpty(tokenResponse.RefreshToken);
            }
        }

        // Outros testes omitidos para clareza
    }
}