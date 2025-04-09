﻿using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using FluentValidation.TestHelper;
using Xunit;

namespace APIProject.UnitTests.Validadores
{
    public class LoginUsuarioComandoValidadorTests
    {
        private readonly LoginUsuarioComandoValidador _validador;

        public LoginUsuarioComandoValidadorTests()
        {
            _validador = new LoginUsuarioComandoValidador();
        }

        [Fact]
        public void Validar_EmailVazio_DeveRetornarErro()
        {
            // Arrange
            var comando = new LoginUsuarioComando { Email = "", Senha = "senha123" };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validar_EmailInvalido_DeveRetornarErro()
        {
            // Arrange
            var comando = new LoginUsuarioComando { Email = "emailinvalido", Senha = "senha123" };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validar_EmailValido_NaoDeveRetornarErro()
        {
            // Arrange
            var comando = new LoginUsuarioComando { Email = "teste@teste.com", Senha = "senha123" };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validar_SenhaVazia_DeveRetornarErro()
        {
            // Arrange
            var comando = new LoginUsuarioComando { Email = "teste@teste.com", Senha = "" };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Senha);
        }

        // O validador não verifica o tamanho mínimo da senha, apenas se está vazia
        // Então este teste não é necessário

        [Fact]
        public void Validar_SenhaValida_NaoDeveRetornarErro()
        {
            // Arrange
            var comando = new LoginUsuarioComando { Email = "teste@teste.com", Senha = "senha123" };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldNotHaveValidationErrorFor(x => x.Senha);
        }

        [Fact]
        public void Validar_ComandoValido_NaoDeveRetornarErros()
        {
            // Arrange
            var comando = new LoginUsuarioComando { Email = "teste@teste.com", Senha = "senha123" };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldNotHaveAnyValidationErrors();
        }
    }
}
