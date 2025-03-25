using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using APIProject.Domain.Interfaces.Servicos;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace APIProject.Application.Usuarios.Comandos.LoginUsuario
{
    public class LoginUsuarioComandoHandler : IRequestHandler<LoginUsuarioComando, TokenDto>
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IHashService _hashService;
        private readonly ITokenService _tokenService;
        private readonly IUsuarioServico _usuarioServico;

        public LoginUsuarioComandoHandler(
            IUsuarioRepositorio usuarioRepositorio,
            IHashService hashService,
            ITokenService tokenService,
            IUsuarioServico usuarioServico)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _hashService = hashService;
            _tokenService = tokenService;
            _usuarioServico = usuarioServico;
        }

        public async Task<TokenDto> Handle(LoginUsuarioComando request, CancellationToken cancellationToken)
        {
            // Buscar o usuário pelo email
            var usuario = await _usuarioRepositorio.ObterPorEmailAsync(request.Email);
            if (usuario == null)
            {
                throw new OperacaoNaoAutorizadaException("Usuário ou senha inválidos");
            }

            // Verificar se o usuário está ativo
            if (!usuario.Ativo)
            {
                throw new OperacaoNaoAutorizadaException("Usuário inativo");
            }

            // Verificar a senha
            if (!_hashService.VerificarHash(request.Senha, usuario.Senha))
            {
                throw new OperacaoNaoAutorizadaException("Usuário ou senha inválidos");
            }

            // Registrar o login
            _usuarioServico.RegistrarLogin(usuario);
            await _usuarioRepositorio.AtualizarAsync(usuario);
            await _usuarioRepositorio.SalvarAsync();

            // Gerar o token
            return _tokenService.GerarToken(usuario);
        }
    }
}