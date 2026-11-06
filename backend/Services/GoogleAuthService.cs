using Google.Apis.Auth;
using OpenDoors.Api.Exceptions;

namespace OpenDoors.Api.Services
{
    /// <summary>
    /// Valida o ID token ("credential") devolvido pelo botão "Entrar com o Google".
    /// O Client ID fica em appsettings → "Google:ClientId" (criado no Google Cloud Console).
    /// </summary>
    public class GoogleAuthService
    {
        private readonly string? _clientId;

        public GoogleAuthService(IConfiguration config)
        {
            var clientId = config["Google:ClientId"];
            _clientId = string.IsNullOrWhiteSpace(clientId) || clientId == "GOOGLE_CLIENT_ID" ? null : clientId;
        }

        public string? ClientId => _clientId;

        public record UsuarioGoogle(string Email, string Nome);

        /// <summary>
        /// Confere assinatura, validade e destinatário do token e devolve o e-mail verificado.
        /// </summary>
        /// <exception cref="InvalidCredentialsException">Token inválido, expirado ou com e-mail não verificado</exception>
        public async Task<UsuarioGoogle> ValidarAsync(string? credential)
        {
            if (_clientId == null)
                throw new InvalidOperationException("Login com Google não configurado (Google:ClientId).");

            if (string.IsNullOrWhiteSpace(credential))
                throw new MissingRequiredFieldException("Credencial do Google");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(credential,
                    new GoogleJsonWebSignature.ValidationSettings { Audience = new[] { _clientId } });
            }
            // Token adulterado ou malformado (ex.: Base64 inválido) também é credencial inválida
            catch (Exception ex) when (ex is InvalidJwtException or FormatException or ArgumentException)
            {
                throw new InvalidCredentialsException("Login com Google inválido ou expirado. Tente novamente.");
            }

            if (!payload.EmailVerified || string.IsNullOrWhiteSpace(payload.Email))
                throw new InvalidCredentialsException("O e-mail desta conta Google não está verificado.");

            return new UsuarioGoogle(LoginService.NormalizarEmail(payload.Email), payload.Name ?? string.Empty);
        }
    }
}
