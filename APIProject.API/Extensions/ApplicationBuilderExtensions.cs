using APIProject.API.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace APIProject.API.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TratamentoExcecoesMiddleware>();
        }

        public static IApplicationBuilder UseTokenValidacao(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TokenValidacaoMiddleware>();
        }
    }
}