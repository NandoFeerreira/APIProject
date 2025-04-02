using APIProject.Application.DTOs;
using APIProject.Application.Interfaces;
using APIProject.Domain.Entidades;
using APIProject.Infrastructure.Configuracoes;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenService : ITokenService
{
    private readonly JwtConfiguracoes _jwtConfiguracoes;

    public TokenService(IOptions<JwtConfiguracoes> jwtConfiguracoes)
    {
        _jwtConfiguracoes = jwtConfiguracoes.Value;
    }

    public TokenDto GerarToken(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Name, usuario.Nome),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguracoes.Chave));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var expiracao = DateTime.UtcNow.AddMinutes(_jwtConfiguracoes.ExpiracaoMinutos);

        var token = new JwtSecurityToken(
            issuer: _jwtConfiguracoes.Emissor,
            audience: _jwtConfiguracoes.Audiencia,
            claims: claims,
            expires: expiracao,
            signingCredentials: credenciais
        );

        return new TokenDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiracao = expiracao
        };
    }
}
