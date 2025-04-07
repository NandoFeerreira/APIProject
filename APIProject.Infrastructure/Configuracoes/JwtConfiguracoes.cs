namespace APIProject.Infrastructure.Configuracoes
{
    public class JwtConfiguracoes
    {
        public string Chave { get; set; }  = string.Empty;
        public string Emissor { get; set; }  = string.Empty;
         public string Audiencia { get; set; }  = string.Empty;
        public int ExpiracaoMinutos { get; set; }
    }
}