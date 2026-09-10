using Microsoft.IdentityModel.Tokens;
using PotyIaApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PotyIaApi.Services
{
    /// <summary>
    /// Responsável pela emissão do Access Token (JWT próprio do PotyIA).
    /// </summary>
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GerarAccessToken(UsuarioAutenticadoModel usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioID),
                new Claim(ClaimTypes.Name, usuario.Nome),
            };

            var minutos = int.TryParse(_config["Jwt:AccessTokenExpirationMinutes"], out var m) ? m : 120;

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutos),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
