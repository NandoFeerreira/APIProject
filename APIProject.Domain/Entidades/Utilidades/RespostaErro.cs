namespace APIProject.Domain.Entidades.Utilidades
{
    /// <summary>
    /// Modelo padronizado para resposta de erro
    /// </summary>
    public class RespostaErro
    {
        public string? Tipo { get; set; }
        public string? Titulo { get; set; }
        public int? Status { get; set; }
        public string? Detalhe { get; set; }
        public IDictionary<string, string[]> Erros { get; set; }

        public RespostaErro()
        {
            Erros = new Dictionary<string, string[]>();
        }
    }
}
