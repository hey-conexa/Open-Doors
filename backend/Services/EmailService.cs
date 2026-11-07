using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace OpenDoors.Api.Services
{
    /// <summary>
    /// Envio de e-mails por SMTP (funciona com Gmail, Outlook, Brevo etc.).
    /// Configuração em appsettings.Development.json → "Email":
    ///   SmtpHost, SmtpPort (587), Usuario, Senha, Remetente, NomeRemetente
    /// Sem "Email:SmtpHost" configurado, o e-mail não é enviado: o conteúdo vai para o log
    /// (útil para testar localmente).
    /// </summary>
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public bool Configurado => !string.IsNullOrWhiteSpace(_config["Email:SmtpHost"]);

        public async Task EnviarAsync(string para, string assunto, string html, string textoSimples)
        {
            if (!Configurado)
            {
                _logger.LogWarning("Email:SmtpHost não configurado. E-mail NÃO enviado para {Para}. Assunto: {Assunto}. Conteúdo: {Texto}",
                    para, assunto, textoSimples);
                return;
            }

            var remetente = _config["Email:Remetente"] ?? _config["Email:Usuario"]
                ?? throw new InvalidOperationException("Email:Remetente não configurado");

            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress(_config["Email:NomeRemetente"] ?? "Open Doors", remetente));
            mensagem.To.Add(MailboxAddress.Parse(para));
            mensagem.Subject = assunto;
            mensagem.Body = new BodyBuilder { HtmlBody = html, TextBody = textoSimples }.ToMessageBody();

            var porta = int.TryParse(_config["Email:SmtpPort"], out var p) ? p : 587;

            using var smtp = new SmtpClient
            {
                // Consulta se o certificado do servidor foi revogado (CRL/OCSP). Fica ligada por padrão.
                // Desligue com "Email:VerificarRevogacao": false só em redes cujo proxy bloqueia essas
                // consultas; a cadeia, o nome do servidor e a validade do certificado continuam verificados.
                CheckCertificateRevocation = !bool.TryParse(_config["Email:VerificarRevogacao"], out var verificar) || verificar
            };
            await smtp.ConnectAsync(_config["Email:SmtpHost"], porta,
                porta == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);

            if (!string.IsNullOrWhiteSpace(_config["Email:Usuario"]))
            {
                var senha = _config["Email:Senha"] ?? string.Empty;

                // Senha de app do Gmail é exibida como "abcd efgh ijkl mnop", mas os espaços não fazem parte dela
                if (_config["Email:SmtpHost"]!.Contains("gmail.com", StringComparison.OrdinalIgnoreCase))
                    senha = senha.Replace(" ", string.Empty);

                await smtp.AuthenticateAsync(_config["Email:Usuario"], senha);
            }

            await smtp.SendAsync(mensagem);
            await smtp.DisconnectAsync(true);
        }
    }
}
