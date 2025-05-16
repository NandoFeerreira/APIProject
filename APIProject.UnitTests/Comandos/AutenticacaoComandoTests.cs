using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using APIProject.Domain.Entidades;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using APIProject.Domain.Interfaces.Servicos;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace APIProject.UnitTests.Comandos
{
    /// <summary>
    /// Fixture compartilhada para os testes de autenticação
    /// </summary>
    public class AutenticacaoFixture
    {
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }
        public Mock<IUsuarioRepositorio> UsuarioRepositorioMock { get; }
        public Mock<IHashService> HashServiceMock { get; }
        public Mock<ITokenService> TokenServiceMock { get; }
        public Mock<IUsuarioServico> UsuarioServicoMock { get; }
        public Mock<IValidator<LoginUsuarioComando>> LoginValidatorMock { get; }
        public Mock<IValidator<RegistrarUsuarioComando>> RegistroValidatorMock { get; }
        public Mock<IMapper> MapperMock { get; }
        public Mock<ICacheService> CacheServiceMock { get; }

        public AutenticacaoFixture()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            UsuarioRepositorioMock = new Mock<IUsuarioRepositorio>();
            HashServiceMock = new Mock<IHashService>();
            TokenServiceMock = new Mock<ITokenService>();
            UsuarioServicoMock = new Mock<IUsuarioServico>();
            LoginValidatorMock = new Mock<IValidator<LoginUsuarioComando>>();
            RegistroValidatorMock = new Mock<IValidator<RegistrarUsuarioComando>>();
            MapperMock = new Mock<IMapper>();
            CacheServiceMock = new Mock<ICacheService>();

            // Configurar o UnitOfWork para retornar o repositório de usuários mockado
            UnitOfWorkMock.Setup(uow => uow.Usuarios).Returns(UsuarioRepositorioMock.Object);

            // Configurar os validadores para retornar sucesso por padrão
            LoginValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<LoginUsuarioComando>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            RegistroValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<RegistrarUsuarioComando>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
                
            // Configurar o cache para retornar null por padrão (simulando cache vazio)
            CacheServiceMock
                .Setup(x => x.GetAsync<int?>(It.IsAny<string>()))
                .ReturnsAsync((int?)null);
                
            CacheServiceMock
                .Setup(x => x.GetAsync<DateTime?>(It.IsAny<string>()))
                .ReturnsAsync((DateTime?)null);
        }

        public LoginUsuarioComandoHandler CreateLoginHandler()
        {
            return new LoginUsuarioComandoHandler(
                UnitOfWorkMock.Object,
                HashServiceMock.Object,
                TokenServiceMock.Object,
                UsuarioServicoMock.Object,
                LoginValidatorMock.Object,
                CacheServiceMock.Object);
        }

        public RegistrarUsuarioComandoHandler CreateRegistroHandler()
        {
            return new RegistrarUsuarioComandoHandler(
                UnitOfWorkMock.Object,
                MapperMock.Object,
                HashServiceMock.Object,
                RegistroValidatorMock.Object);
        }

        public static Usuario CriarUsuarioTeste(string nome, string email, string senhaHash, bool ativo = true)
        {
            var usuario = new Usuario(nome, email, senhaHash)
            {
                Ativo = ativo
            };
            return usuario;
        }
    }

    public class AutenticacaoComandoTests : IClassFixture<AutenticacaoFixture>
    {
        private readonly AutenticacaoFixture _fixture;

        public AutenticacaoComandoTests(AutenticacaoFixture fixture)
        {
            _fixture = fixture;

            // Resetar as configurações dos mocks antes de cada teste
            _fixture.UsuarioRepositorioMock.Reset();
            _fixture.HashServiceMock.Reset();
            _fixture.TokenServiceMock.Reset();
            _fixture.UsuarioServicoMock.Reset();
            _fixture.LoginValidatorMock.Reset();
            _fixture.RegistroValidatorMock.Reset();
            _fixture.MapperMock.Reset();
            _fixture.UnitOfWorkMock.Reset();
            _fixture.CacheServiceMock.Reset();

            // Reconfigurar o UnitOfWork após o reset
            _fixture.UnitOfWorkMock.Setup(uow => uow.Usuarios).Returns(_fixture.UsuarioRepositorioMock.Object);

            // Reconfigurar os validadores com sucesso por padrão
            _fixture.LoginValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<LoginUsuarioComando>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _fixture.RegistroValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<RegistrarUsuarioComando>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
                
            // Reconfigurar o cache
            _fixture.CacheServiceMock
                .Setup(x => x.GetAsync<int?>(It.IsAny<string>()))
                .ReturnsAsync((int?)null);
                
            _fixture.CacheServiceMock
                .Setup(x => x.GetAsync<DateTime?>(It.IsAny<string>()))
                .ReturnsAsync((DateTime?)null);
        }
    }
}
