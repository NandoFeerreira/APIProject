namespace APIProject.Infrastructure.Configuracoes
{
    public class JwtConfiguracoes
    {
        public string Chave { get; set; }
        public string Emissor { get; set; }
        public string Audiencia { get; set; }
        public int ExpiracaoMinutos { get; set; }
    }
}