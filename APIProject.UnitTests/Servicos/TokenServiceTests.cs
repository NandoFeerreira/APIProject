﻿using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using APIProject.Infrastructure.Servicos;
using APIProject.Infrastructure.Configuracoes;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace APIProject.UnitTests.Servicos
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;
        private readonly JwtConfiguracoes _jwtConfiguracoes;

        public TokenServiceTests()
        {
            _jwtConfiguracoes = new JwtConfiguracoes
            {
                Chave = "ChaveSecretaSuperSeguraComPeloMenos256Bits",
                Emissor = "APIProject.Tests",
                Audiencia = "APIProject.Tests",
                ExpiracaoMinutos = 60
            };

            var options = new Mock<IOptions<JwtConfiguracoes>>();
            options.Setup(x => x.Value).Returns(_jwtConfiguracoes);

            _tokenService = new TokenService(options.Object);
        }

        [Fact]
        public void GerarToken_ComUsuarioValido_DeveRetornarTokenDto()
        {
            // Arrange
            var usuario = new Usuario("Teste", "teste@teste.com", "senha_hash");
            usuario.Id = Guid.NewGuid();

            // Act
            var resultado = _tokenService.GerarToken(usuario);

            // Assert
            Assert.NotNull(resultado);
            Assert.NotEmpty(resultado.Token);
            Assert.NotEmpty(resultado.RefreshToken);
            Assert.True(resultado.Expiracao > DateTime.UtcNow);
        }

        [Fact]
        public void GerarToken_ComUsuarioValido_DeveConterClaimsCorretas()
        {
            // Arrange
            var usuario = new Usuario("Teste", "teste@teste.com", "senha_hash");
            usuario.Id = Guid.NewGuid();

            // Act
            var resultado = _tokenService.GerarToken(usuario);

            // Decodificar o token para verificar as claims
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(resultado.Token);

            // Assert
            Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == usuario.Id.ToString());
            Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == usuario.Email);
            Assert.Contains(token.Claims, c => c.Type == "name" && c.Value == usuario.Nome);
            Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Jti);
        }

        [Fact]
        public void GerarToken_ComUsuarioValido_DeveDefinirExpiracaoCorreta()
        {
            // Arrange
            var usuario = new Usuario("Teste", "teste@teste.com", "senha_hash");
            usuario.Id = Guid.NewGuid();

            // Act
            var resultado = _tokenService.GerarToken(usuario);

            // Decodificar o token para verificar a expiração
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(resultado.Token);

            // Assert
            var expiracao = token.ValidTo;
            var agora = DateTime.UtcNow;
            var diferencaMinutos = (expiracao - agora).TotalMinutes;

            // A diferença deve ser aproximadamente igual ao valor configurado (com uma pequena margem de erro)
            Assert.InRange(diferencaMinutos, _jwtConfiguracoes.ExpiracaoMinutos - 1, _jwtConfiguracoes.ExpiracaoMinutos + 1);
        }


    }
}
