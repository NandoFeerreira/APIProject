using APIProject.Domain.Entidades.Utilidades;
using APIProject.Domain.Excecoes;
using System.Net;
using System.Text.Json;

namespace APIProject.API.Middlewares
{
    public class TratamentoExcecoesMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TratamentoExcecoesMiddleware> _logger;

        public TratamentoExcecoesMiddleware(RequestDelegate next, ILogger<TratamentoExcecoesMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exceção não tratada: {Mensagem}", ex.Message);
                await TratarExcecaoAsync(context, ex);
            }
        }

        private static async Task TratarExcecaoAsync(HttpContext context, Exception exception)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var resposta = new RespostaErro
            {
                Titulo = "Ocorreu um erro durante o processamento da sua requisição.",
                Status = (int)statusCode,
                Tipo = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Detalhe = exception.Message
            };

            switch (exception)
            {
                case EntidadeNaoEncontradaException:
                    statusCode = HttpStatusCode.NotFound;
                    resposta.Titulo = "Recurso não encontrado";
                    resposta.Tipo = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                    break;

                case OperacaoNaoAutorizadaException:
                    statusCode = HttpStatusCode.Unauthorized;
                    resposta.Titulo = "Operação não autorizada";
                    resposta.Tipo = "https://tools.ietf.org/html/rfc7235#section-3.1";
                    break;

                case ValidacaoException validacaoEx:
                    statusCode = HttpStatusCode.BadRequest;
                    resposta.Titulo = "Erro de validação";
                    resposta.Tipo = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    resposta.Erros = validacaoEx.Erros;
                    break;

                case DadosDuplicadosException:
                    statusCode = HttpStatusCode.Conflict;
                    resposta.Titulo = "Dados duplicados";
                    resposta.Tipo = "https://tools.ietf.org/html/rfc7231#section-6.5.8";
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            resposta.Status = (int)statusCode;

            var opcoes = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(resposta, opcoes);
            await context.Response.WriteAsync(json);
        }
    }

    
    public static class TratamentoExcecoesMiddlewareExtensions
    {
        public static IApplicationBuilder UseTratamentoExcecoes(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TratamentoExcecoesMiddleware>();
        }
    }
}
