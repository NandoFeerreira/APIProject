using APIProject.Application.Common;
using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using APIProject.Domain.Interfaces.Servicos;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace APIProject.Application.Usuarios.Comandos.LoginUsuario
{
    public class LoginUsuarioComandoHandler : BaseCommandHandler<LoginUsuarioComando, IValidator<LoginUsuarioComando>>,
                                             IRequestHandler<LoginUsuarioComando, TokenDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHashService _hashService;
        private readonly ITokenService _tokenService;
        private readonly IUsuarioServico _usuarioServico;

        public LoginUsuarioComandoHandler(
            IUnitOfWork unitOfWork,
            IHashService hashService,
            ITokenService tokenService,
            IUsuarioServico usuarioServico,
            IValidator<LoginUsuarioComando> validator) : base(validator)
        {
            _unitOfWork = unitOfWork;
            _hashService = hashService;
            _tokenService = tokenService;
            _usuarioServico = usuarioServico;
        }

        public async Task<TokenDto> Handle(LoginUsuarioComando request, CancellationToken cancellationToken)
        {
            // Validar o comando usando o método da classe base
            await ValidateCommand(request, cancellationToken);

            // Buscar o usuário pelo email
            var usuario = await _unitOfWork.Usuarios.ObterPorEmailAsync(request.Email);
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
            if (!_hashService.VerificarHash(request.Senha, usuario.Senha!))
            {
                throw new OperacaoNaoAutorizadaException("Usuário ou senha inválidos");
            }

            // Invalidate all existing refresh tokens
            foreach (var token in usuario.RefreshTokens.Where(rt => rt.EstaAtivo))
            {
                token.Invalidado = true;
            }

            var tokenDto = _tokenService.GerarToken(usuario);

            usuario.AdicionarRefreshToken(
                tokenDto.RefreshToken,
                DateTime.UtcNow.AddDays(1)
            );

            _usuarioServico.RegistrarLogin(usuario);
            await _unitOfWork.CommitAsync();

            return tokenDto;
        }
    }

}
