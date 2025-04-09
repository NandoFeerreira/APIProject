﻿using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using FluentValidation.TestHelper;
using Xunit;

namespace APIProject.UnitTests.Validadores
{
    public class RegistrarUsuarioComandoValidadorTests
    {
        private readonly RegistrarUsuarioComandoValidador _validador;

        public RegistrarUsuarioComandoValidadorTests()
        {
            _validador = new RegistrarUsuarioComandoValidador();
        }

        [Fact]
        public void Validar_NomeVazio_DeveRetornarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "",
                Email = "teste@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Nome);
        }

        // O validador não verifica o tamanho mínimo do nome, apenas se está vazio
        // e se não excede 100 caracteres
        [Fact]
        public void Validar_NomeMuitoLongo_DeveRetornarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = new string('A', 101), // 101 caracteres
                Email = "teste@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Nome);
        }

        [Fact]
        public void Validar_NomeValido_NaoDeveRetornarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Nome Teste",
                Email = "teste@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldNotHaveValidationErrorFor(x => x.Nome);
        }

        [Fact]
        public void Validar_EmailVazio_DeveRetornarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Nome Teste",
                Email = "",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validar_EmailInvalido_DeveRetornarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Nome Teste",
                Email = "emailinvalido",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validar_EmailValido_NaoDeveRetornarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Nome Teste",
                Email = "teste@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validar_SenhaVazia_DeveRetornarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Nome Teste",
                Email = "teste@teste.com",
                Senha = "",
                ConfirmacaoSenha = ""
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Senha);
        }

        [Fact]
        public void Validar_SenhaMuitoCurta_DeveRetornarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Nome Teste",
                Email = "teste@teste.com",
                Senha = "123",
                ConfirmacaoSenha = "123"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Senha);
        }

        [Fact]
        public void Validar_SenhasSemCaracteresEspeciais_DeveRetornarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Nome Teste",
                Email = "teste@teste.com",
                Senha = "Senha123",
                ConfirmacaoSenha = "Senha123"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.Senha);
        }

        [Fact]
        public void Validar_SenhasDiferentes_DeveRetornarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Nome Teste",
                Email = "teste@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha456!"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldHaveValidationErrorFor(x => x.ConfirmacaoSenha);
        }

        [Fact]
        public void Validar_SenhasValidas_NaoDeveRetornarErro()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Nome Teste",
                Email = "teste@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldNotHaveValidationErrorFor(x => x.Senha);
            resultado.ShouldNotHaveValidationErrorFor(x => x.ConfirmacaoSenha);
        }

        [Fact]
        public void Validar_ComandoValido_NaoDeveRetornarErros()
        {
            // Arrange
            var comando = new RegistrarUsuarioComando
            {
                Nome = "Nome Teste",
                Email = "teste@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            // Act & Assert
            var resultado = _validador.TestValidate(comando);
            resultado.ShouldNotHaveAnyValidationErrors();
        }
    }
}
