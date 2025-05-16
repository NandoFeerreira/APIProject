using APIProject.Application.Common;
using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Excecoes;
using APIProject.Domain.Interfaces;
using APIProject.Domain.Interfaces.Servicos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
            // Validar o comando
            await ValidateCommand(request, cancellationToken);

            // Buscar o usuário pelo email (já incluindo os refresh tokens)
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

            // Gerar o token antes de fazer qualquer alteração no usuário
            var tokenDto = _tokenService.GerarToken(usuario);

            // Tentar salvar as alterações com retry simples
            const int maxRetries = 3;
            int retryCount = 0;
            bool salvou = false;

            while (!salvou && retryCount < maxRetries)
            {
                try
                {
                    // Invalidar tokens existentes
                    if (usuario.RefreshTokens != null)
                    {
                        foreach (var token in usuario.RefreshTokens.Where(rt => rt.EstaAtivo))
                        {
                            token.Invalidado = true;
                        }
                    }

                    // Adicionar o novo refresh token
                    usuario.AdicionarRefreshToken(
                        tokenDto.RefreshToken,
                        DateTime.UtcNow.AddDays(1)
                    );

                    // Registrar o login
                    _usuarioServico.RegistrarLogin(usuario);

                    // Salvar as alterações
                    await _unitOfWork.CommitAsync();
                    salvou = true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    retryCount++;

                    if (retryCount >= maxRetries)
                    {
                        // Se atingimos o número máximo de tentativas, vamos tentar uma abordagem diferente
                        try
                        {
                            // Buscar o usuário novamente
                            usuario = await _unitOfWork.Usuarios.ObterPorIdComRefreshTokensAsync(usuario.Id);
                            if (usuario != null)
                            {
                                // Invalidar tokens existentes
                                if (usuario.RefreshTokens != null)
                                {
                                    foreach (var token in usuario.RefreshTokens.Where(rt => rt.EstaAtivo))
                                    {
                                        token.Invalidado = true;
                                    }
                                }

                                // Adicionar o novo refresh token
                                usuario.AdicionarRefreshToken(
                                    tokenDto.RefreshToken,
                                    DateTime.UtcNow.AddDays(1)
                                );

                                // Registrar o login
                                _usuarioServico.RegistrarLogin(usuario);

                                // Salvar as alterações
                                await _unitOfWork.CommitAsync();
                                salvou = true;
                            }
                        }
                        catch (Exception)
                        {
                            // Se ainda falhar, vamos apenas continuar e retornar o token
                            // O usuário poderá usar o token, mas o refresh token pode não funcionar
                        }
                    }
                    else
                    {
                        // Buscar o usuário novamente para a próxima tentativa
                        usuario = await _unitOfWork.Usuarios.ObterPorEmailAsync(request.Email);
                        if (usuario == null)
                        {
                            // Se o usuário não existir mais, vamos parar as tentativas
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Para outros erros, lançamos a exceção
                    throw new Exception($"Erro ao processar o login: {ex.Message}", ex);
                }
            }

            return tokenDto;
        }
    }

}
