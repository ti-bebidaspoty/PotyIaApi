using PotyIaApi.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace PotyIaApi.Helpers
{
    public class Helper : IHelper
    {
        public string Criptografar(string senhaAberta)
        {
            byte[] hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(senhaAberta));
            var builder = new StringBuilder();
            for (int i = 0; i < hashedBytes.Length; i++)
                builder.Append(hashedBytes[i].ToString("x2"));
            return builder.ToString();
        }
    }
}
