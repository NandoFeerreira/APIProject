namespace APIProject.Application.Interfaces
{
    public interface IHashService
    {
        string CriarHash(string senha);
        bool VerificarHash(string senha, string hash);
    }
}