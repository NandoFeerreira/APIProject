﻿using APIProject.Application.DTOs;
using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using APIProject.Infrastructure.Persistencia;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace APIProject.IntegrationTests.Controllers
{
    public class AutenticacaoControllerTests
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public AutenticacaoControllerTests()
        {
            try
            {
                // Criar factory com nome de banco de dados único para evitar colisões
                _factory = new CustomWebApplicationFactory($"TestDb-Usuario-{Guid.NewGuid()}");
                _client = _factory.CreateClient();
                _factory.SeedTestData();
            }
            catch (Exception)
            {
                    throw;
            }
        }

        

        [Fact]
        public async Task Login_ComCredenciaisInvalidas_RetornaUnauthorized()
        {
            // Arrange
            var loginDto = new LoginUsuarioDto
            {
                Email = "teste@teste.com",
                Senha = "senhaErrada"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/autenticacao/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Registrar_ComDadosValidos_RetornaUsuarioCriado()
        {
            // Gerar email único para evitar conflitos
            string emailUnico = $"novo{Guid.NewGuid().ToString()[..8]}@teste.com";

            // Arrange
            var registroDto = new RegistroUsuarioDto
            {
                Nome = "Novo Usuário",
                Email = emailUnico,
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/autenticacao/registrar", registroDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            if (response.IsSuccessStatusCode)
            {
                var usuarioResponse = await response.Content.ReadFromJsonAsync<UsuarioDto>();
                Assert.NotNull(usuarioResponse);
                Assert.Equal(registroDto.Email, usuarioResponse.Email);
                Assert.Equal(registroDto.Nome, usuarioResponse.Nome);
            }
        }
        
    }
}

