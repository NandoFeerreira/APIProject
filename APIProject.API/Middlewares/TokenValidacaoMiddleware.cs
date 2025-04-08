
using APIProject.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace APIProject.API.Middlewares
{
    public class TokenValidacaoMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenValidacaoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
        {
            string? authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                string token = authHeader["Bearer ".Length..];

                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();

                    if (tokenHandler.CanReadToken(token))
                    {
                        var jwtToken = tokenHandler.ReadJwtToken(token);
                        var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

                        if (!string.IsNullOrEmpty(jti) && await unitOfWork.TokensInvalidados.TokenEstaInvalidadoAsync(jti))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsync("Token inválido ou revogado");
                            return;
                        }
                    }
                }
                catch
                {
                    // Se não conseguirmos ler o token, deixamos passar para que o middleware de autenticação trate
                }
            }

            await _next(context);
        }
    }
}