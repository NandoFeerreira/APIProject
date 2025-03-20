namespace APIProject.Application.DTOs
{
    public class TokenDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiracao { get; set; }
    }
}