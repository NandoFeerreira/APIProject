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
    public class LoginUsuarioComandoHandler : IRequestHandler<LoginUsuarioComando, TokenDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHashService _hashService;
        private readonly ITokenService _tokenService;
        private readonly IUsuarioServico _usuarioServico;
        private readonly IValidator<LoginUsuarioComando> _validator;

        public LoginUsuarioComandoHandler(
            IUnitOfWork unitOfWork,
            IHashService hashService,
            ITokenService tokenService,
            IUsuarioServico usuarioServico,
            IValidator<LoginUsuarioComando> validator)
        {
            _unitOfWork = unitOfWork;
            _hashService = hashService;
            _tokenService = tokenService;
            _usuarioServico = usuarioServico;
            _validator = validator;
        }

        public async Task<TokenDto> Handle(LoginUsuarioComando request, CancellationToken cancellationToken)
        {
            // Validar comando usando o validador existente
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var erros = validationResult.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new ValidacaoException(erros);
            }

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
            if (!_hashService.VerificarHash(request.Senha, usuario.Senha))
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
