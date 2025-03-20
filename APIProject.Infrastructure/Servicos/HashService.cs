using APIProject.Application.Interfaces;
using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace APIProject.Infrastructure.Servicos
{
    public class HashService : IHashService
    {
        public string CriarHash(string senha)
        {
            // Gerar um salt aleatório
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Derivar uma chave de 256 bits da senha com HMACSHA256
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: senha,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Combinar o salt e a senha hasheada
            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        public bool VerificarHash(string senha, string hashArmazenado)
        {
            // Extrair o salt e o hash
            var partes = hashArmazenado.Split(':');
            if (partes.Length != 2)
                return false;

            var salt = Convert.FromBase64String(partes[0]);
            var hashOriginal = partes[1];

            // Computar o hash da senha fornecida com o mesmo salt
            string hashComputado = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: senha,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Comparar os hashes
            return hashOriginal == hashComputado;
        }
    }
}