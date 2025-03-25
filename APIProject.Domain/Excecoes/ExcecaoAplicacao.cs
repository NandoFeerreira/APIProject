namespace APIProject.Domain.Excecoes
{
    /// <summary>
    /// Exceção base para todas as exceções de negócio da aplicação
    /// </summary>
    public abstract class ExcecaoAplicacao : Exception
    {
        public ExcecaoAplicacao(string mensagem) : base(mensagem) { }
        public ExcecaoAplicacao(string mensagem, Exception innerException) : base(mensagem, innerException) { }
    }

    /// <summary>
    /// Exceção para entidades não encontradas
    /// </summary>
    public class EntidadeNaoEncontradaException : ExcecaoAplicacao
    {
        public EntidadeNaoEncontradaException(string entidade, object chave)
            : base($"Entidade '{entidade}' ({chave}) não foi encontrada.") { }
    }

    /// <summary>
    /// Exceção para operações não autorizadas
    /// </summary>
    public class OperacaoNaoAutorizadaException : ExcecaoAplicacao
    {
        public OperacaoNaoAutorizadaException(string mensagem) : base(mensagem) { }
    }

    /// <summary>
    /// Exceção para validações de negócio
    /// </summary>
    public class ValidacaoException : ExcecaoAplicacao
    {
        public IDictionary<string, string[]> Erros { get; }

        public ValidacaoException()
            : base("Um ou mais erros de validação ocorreram.")
        {
            Erros = new Dictionary<string, string[]>();
        }

        public ValidacaoException(IDictionary<string, string[]> erros)
            : this()
        {
            Erros = erros;
        }

        public ValidacaoException(string propriedade, string mensagem)
            : this()
        {
            Erros = new Dictionary<string, string[]>
            {
                { propriedade, new[] { mensagem } }
            };
        }
    }

    /// <summary>
    /// Exceção para dados duplicados
    /// </summary>
    public class DadosDuplicadosException : ExcecaoAplicacao
    {
        public DadosDuplicadosException(string entidade, string campo, string valor)
            : base($"Já existe um(a) {entidade} com {campo} '{valor}'.") { }
    }
}
