using APIProject.Application.DTOs;
using APIProject.Application.Usuarios.Comandos.LoginUsuario;
using APIProject.Application.Usuarios.Comandos.RegistrarUsuario;
using APIProject.Domain.Entidades;
using AutoMapper;

namespace APIProject.Application.Mapeamentos
{
    public class UsuarioMapeamentoProfile : Profile
    {
        public UsuarioMapeamentoProfile()
        {
            CreateMap<Usuario, UsuarioDto>();
            CreateMap<RegistroUsuarioDto, RegistrarUsuarioComando>();
            CreateMap<LoginUsuarioDto, LoginUsuarioComando>();
        }
    }
}