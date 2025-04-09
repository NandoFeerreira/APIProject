﻿using APIProject.Application.Usuarios.Comandos.RefreshToken;
using FluentValidation.TestHelper;
using Xunit;

namespace APIProject.UnitTests.Validadores
{
    public class RefreshTokenComandoValidadorTests
    {
        private readonly RefreshTokenComandoValidador _validador;

        public RefreshTokenComandoValidadorTests()
        {
            _validador = new RefreshTokenComandoValidador();
        }

        [Fact]
        public void Validar_TokenVazio_DeveRetornarErro()
        {
            // Arrange
            var comando = new RefreshTokenComando { Token = "", RefreshToken = "refresh_token" };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Token);
        }

        [Fact]
        public void Validar_TokenValido_NaoDeveRetornarErro()
        {
            // Arrange
            var comando = new RefreshTokenComando { Token = "token_valido", RefreshToken = "refresh_token" };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldNotHaveValidationErrorFor(x => x.Token);
        }

        [Fact]
        public void Validar_RefreshTokenVazio_DeveRetornarErro()
        {
            // Arrange
            var comando = new RefreshTokenComando { Token = "token_valido", RefreshToken = "" };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.RefreshToken);
        }

        [Fact]
        public void Validar_RefreshTokenValido_NaoDeveRetornarErro()
        {
            // Arrange
            var comando = new RefreshTokenComando { Token = "token_valido", RefreshToken = "refresh_token" };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldNotHaveValidationErrorFor(x => x.RefreshToken);
        }

        [Fact]
        public void Validar_ComandoValido_NaoDeveRetornarErros()
        {
            // Arrange
            var comando = new RefreshTokenComando { Token = "token_valido", RefreshToken = "refresh_token" };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldNotHaveAnyValidationErrors();
        }
    }
}
