namespace APIProject.Infrastructure.Configuracoes
{
    public class RedisConfiguracoes
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string InstanceName { get; set; } = string.Empty;
        public int AbsoluteExpirationInMinutes { get; set; } = 30;
        public int SlidingExpirationInMinutes { get; set; } = 10;
    }
}