using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using APIProject.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace APIProject.Application.Usuarios.Comandos.RegistrarUsuario
{
    public class RegistrarUsuarioComandoHandler : IRequestHandler<RegistrarUsuarioComando, UsuarioDto>
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IMapper _mapper;
        private readonly IHashService _hashService;

        public RegistrarUsuarioComandoHandler(
            IUsuarioRepositorio usuarioRepositorio,
            IMapper mapper,
            IHashService hashService)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _mapper = mapper;
            _hashService = hashService;
        }

        public async Task<UsuarioDto> Handle(RegistrarUsuarioComando request, CancellationToken cancellationToken)
        {
            // Verificar se o email já existe
            if (await _usuarioRepositorio.EmailExisteAsync(request.Email))
            {
                throw new Exception("Email já cadastrado");
            }

            // Criar novo usuário
            var usuario = new Usuario(
                request.Nome,
                request.Email,
                _hashService.CriarHash(request.Senha)
            );

            await _usuarioRepositorio.AdicionarAsync(usuario);
            await _usuarioRepositorio.SalvarAsync();

            return _mapper.Map<UsuarioDto>(usuario);
        }
    }
}
    
