using System.Net.Http.Headers;
using System.Net.Http.Json;
using APIProject.Application.DTOs;
using APIProject.Application.DTOs.Autenticacao;
using APIProject.Application.DTOs.Usuarios;
using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using APIProject.Infrastructure.Persistencia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace APIProject.IntegrationTests
{
    public abstract class IntegrationTestBase : IClassFixture<TestFixture>, IDisposable
    {
        protected readonly TestFixture _fixture;
        protected readonly HttpClient _client;
        private bool _disposed;

        protected IntegrationTestBase(TestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Specific cleanup for test if needed
                _disposed = true;
            }
        }

        /// <summary>
        /// Define o token de autenticação para as requisições
        /// </summary>
        protected void SetAuthenticationToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Realiza login e retorna o token de autenticação
        /// </summary>
        protected async Task<string?> LoginAsync(string email, string senha)
        {
            var loginDto = new LoginRequestDto
            {
                Email = email,
                Senha = senha
            };

            var response = await _client.PostAsJsonAsync("/api/autenticacao/login", loginDto);
            if (response.IsSuccessStatusCode)
            {
                var tokenDto = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
                return tokenDto?.AccessToken;
            }

            return null;
        }

        /// <summary>
        /// Registra um novo usuário e retorna o DTO do usuário criado
        /// </summary>
        protected async Task<UsuarioResponseDto?> RegistrarUsuarioAsync(string nome, string email, string senha)
        {
            var registroDto = new RegistroRequestDto
            {
                Nome = nome,
                Email = email,
                Senha = senha,
                ConfirmacaoSenha = senha
            };

            var response = await _client.PostAsJsonAsync("/api/autenticacao/registrar", registroDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UsuarioResponseDto>();
            }

            return null;
        }

        /// <summary>
        /// Obtém um usuário do banco de dados pelo email
        /// </summary>
        protected Usuario? ObterUsuarioPorEmail(string email)
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IntegrationTestBase>>();

            // Buscar diretamente no contexto
            var usuario = context.Usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario != null)
            {
                logger.LogInformation("Usuário encontrado no banco de dados: {Email}, {Nome}, {Ativo}",
                    usuario.Email, usuario.Nome, usuario.Ativo);
            }
            else
            {
                logger.LogWarning("Usuário não encontrado no banco de dados: {Email}", email);

                // Listar todos os usuários no banco de dados
                var usuarios = context.Usuarios.ToList();
                logger.LogInformation("Total de usuários no banco de dados: {Count}", usuarios.Count);
                foreach (var u in usuarios)
                {
                    logger.LogInformation("Usuário no banco de dados: {Email}, {Nome}, {Ativo}",
                        u.Email, u.Nome, u.Ativo);
                }
            }

            return usuario;
        }

        /// <summary>
        /// Limpa o banco de dados e recarrega os dados de teste
        /// </summary>
        protected void ResetarBancoDados()
        {
            _fixture.ResetDatabase();
        }
    }
}