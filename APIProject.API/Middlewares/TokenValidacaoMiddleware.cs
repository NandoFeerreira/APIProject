
using APIProject.Domain.Interfaces.Servicos;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace APIProject.API.Middlewares
{
    /// <summary>
    /// Middleware para validação de tokens JWT
    /// </summary>
    public class TokenValidacaoMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenValidacaoMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context, ITokenValidacaoServico tokenValidacaoServico)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string? authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                string token = authHeader["Bearer ".Length..];
                var (estaValido, mensagemErro) = await tokenValidacaoServico.ValidarTokenAsync(token);

                if (!estaValido)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync(mensagemErro ?? "Token inválido");
                    return;
                }
            }

            await _next(context);
        }
    }
}