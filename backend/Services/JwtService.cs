using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Services
{
    /// <summary>
    /// Responsável pela criação de tokens JWT.
    /// A chave secreta fica em appsettings.Development.json → "JWT:Secret"
    /// A validação dos tokens é configurada no Program.cs com a mesma chave, Issuer e Audience.
    /// </summary>
    public class JwtService
    {
        public const string Issuer = "OpenDoors.Api";
        public const string Audience = "OpenDoors.Frontend";

        private readonly string _jwtSecret;
        private readonly int _expirationMinutes;

        public JwtService(IConfiguration config)
        {
            _jwtSecret = config["JWT:Secret"]
                ?? throw new InvalidOperationException("JWT:Secret não configurada no appsettings.Development.json");

            // Sem refresh token, a sessão dura o tempo do token (padrão: 8 horas)
            _expirationMinutes = int.TryParse(config["JWT:ExpirationMinutes"], out var minutos) && minutos > 0
                ? minutos
                : 480;
        }

        public DateTime CalcularExpiracao() => DateTime.UtcNow.AddMinutes(_expirationMinutes);

        /// <summary>
        /// Cria um token JWT contendo as informações de ContaId e TipoUsuario.
        /// </summary>
        /// <param name="login">Dados do login contendo ContaId e TipoUsuario</param>
        /// <param name="expiraEm">Momento (UTC) em que o token expira</param>
        /// <returns>Token JWT em formato string</returns>
        public string CriarToken(Login login, DateTime expiraEm)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var claims = new List<Claim>
            {
                new Claim("conta_id", login.ContaId.ToString()),
                new Claim("tipo_usuario", login.TipoUsuario.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = Issuer,
                Audience = Audience,
                Expires = expiraEm,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
