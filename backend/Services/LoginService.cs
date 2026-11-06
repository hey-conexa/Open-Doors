using Microsoft.EntityFrameworkCore;
using OpenDoors.Api.Exceptions;
using OpenDoors.Api.Infrastructure.Database;
using OpenDoors.Api.Models;
using OpenDoors.Api.DTOs;
using BCrypt.Net;
using System.Security.Cryptography;

namespace OpenDoors.Api.Services
{
    /// <summary>
    /// Serviço responsável pela autenticação de usuários.
    /// Busca credenciais no banco de dados e gera tokens JWT.
    /// </summary>
    public class LoginService
    {
        public const int TamanhoMinimoSenha = 8;
        private const int ValidadeCodigoMinutos = 15;
        private const int IntervaloReenvioSegundos = 60;
        private const int MaximoTentativas = 5;

        private readonly AppDbContext _dbContext;
        private readonly JwtService _jwtService;
        private readonly GoogleAuthService _googleAuth;
        private readonly EmailService _emailService;

        public LoginService(AppDbContext dbContext, JwtService jwtService, GoogleAuthService googleAuth, EmailService emailService)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
            _googleAuth = googleAuth;
            _emailService = emailService;
        }

        /// <summary>
        /// Padroniza o email para comparação e armazenamento (sem espaços e em minúsculas).
        /// </summary>
        public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

        /// <summary>
        /// Autentica um usuário validando email e senha contra o banco de dados.
        /// </summary>
        /// <param name="dto">DTO contendo Email e Senha</param>
        /// <returns>DTO com o token JWT gerado e os dados da conta</returns>
        /// <exception cref="MissingRequiredFieldException">Quando campos obrigatórios estão ausentes</exception>
        /// <exception cref="InvalidCredentialsException">Quando email ou senha são inválidos</exception>
        public async Task<LoginResponseDto> AutenticarAsync(LoginDto dto)
        {
            // Validação de campos obrigatórios
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new MissingRequiredFieldException("Email");

            if (string.IsNullOrWhiteSpace(dto.Senha))
                throw new MissingRequiredFieldException("Senha");

            var email = NormalizarEmail(dto.Email);

            // Busca a conta pelo email
            var loginResult = await _dbContext.Logins
                .FirstOrDefaultAsync(l => l.Email.ToLower() == email);

            if (loginResult == null)
                throw new InvalidCredentialsException();

            // Verifica a senha utilizando BCrypt
            if (!BCrypt.Net.BCrypt.Verify(dto.Senha, loginResult.Senha))
                throw new InvalidCredentialsException();

            return GerarResposta(loginResult);
        }

        /// <summary>
        /// Login com o Google: valida o token e entra na conta que tem o mesmo e-mail.
        /// </summary>
        /// <returns>Resposta com o token, ou null (com os dados do Google) quando ainda não existe conta</returns>
        public async Task<(LoginResponseDto? Resposta, GoogleAuthService.UsuarioGoogle Usuario)> AutenticarGoogleAsync(string? credential)
        {
            var usuario = await _googleAuth.ValidarAsync(credential);

            var login = await _dbContext.Logins
                .FirstOrDefaultAsync(l => l.Email.ToLower() == usuario.Email);

            return (login == null ? null : GerarResposta(login), usuario);
        }

        private LoginResponseDto GerarResposta(Login login)
        {
            var expiraEm = _jwtService.CalcularExpiracao();
            var token = _jwtService.CriarToken(login, expiraEm);

            return new LoginResponseDto
            {
                Token = token,
                ExpiraEm = expiraEm,
                ContaId = login.ContaId,
                TipoUsuario = login.TipoUsuario.ToString()
            };
        }

        /// <summary>
        /// Senha aleatória para contas criadas pelo Google (ninguém a conhece).
        /// A pessoa pode criar uma senha depois pelo "esqueci minha senha".
        /// </summary>
        public static string SenhaAleatoria() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        /// <summary>
        /// "Esqueci minha senha": gera um código de 6 dígitos e envia por e-mail.
        /// Não informa se o e-mail existe (evita descobrir quem tem conta).
        /// </summary>
        public async Task SolicitarRedefinicaoAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new MissingRequiredFieldException("Email");

            var normalizado = NormalizarEmail(email);
            var login = await _dbContext.Logins.FirstOrDefaultAsync(l => l.Email.ToLower() == normalizado);
            if (login == null)
                return;

            var agora = DateTime.UtcNow;
            var abertos = await _dbContext.CodigosRedefinicaoSenha
                .Where(c => c.ContaId == login.ContaId && !c.Usado)
                .ToListAsync();

            // Evita reenvio em sequência (um código por minuto)
            if (abertos.Any(c => c.CriadoEm > agora.AddSeconds(-IntervaloReenvioSegundos)))
                return;

            // Só o código mais recente vale
            abertos.ForEach(c => c.Usado = true);

            var codigo = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            _dbContext.CodigosRedefinicaoSenha.Add(new CodigoRedefinicaoSenha
            {
                ContaId = login.ContaId,
                CodigoHash = BCrypt.Net.BCrypt.HashPassword(codigo),
                CriadoEm = agora,
                ExpiraEm = agora.AddMinutes(ValidadeCodigoMinutos),
                Tentativas = 0,
                Usado = false
            });
            await _dbContext.SaveChangesAsync();

            await _emailService.EnviarAsync(
                login.Email,
                "Open Doors — código para redefinir sua senha",
                $"""
                <div style="font-family:Arial,sans-serif;max-width:480px;margin:auto;color:#1f2937">
                  <h2 style="color:#0e7490">Redefinição de senha</h2>
                  <p>Recebemos um pedido para redefinir a senha da sua conta Open Doors.</p>
                  <p>Use este código (válido por {ValidadeCodigoMinutos} minutos):</p>
                  <p style="font-size:32px;font-weight:bold;letter-spacing:8px;margin:24px 0">{codigo}</p>
                  <p style="color:#6b7280;font-size:13px">Se você não pediu, ignore este e-mail: sua senha continua a mesma.</p>
                </div>
                """,
                $"Seu código para redefinir a senha do Open Doors é {codigo}. Ele vale por {ValidadeCodigoMinutos} minutos. Se você não pediu, ignore este e-mail.");
        }

        /// <summary>
        /// Confere o código enviado por e-mail e define a nova senha.
        /// </summary>
        /// <exception cref="InvalidCredentialsException">Código errado, expirado ou já usado</exception>
        public async Task RedefinirSenhaAsync(string email, string codigo, string novaSenha)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new MissingRequiredFieldException("Email");

            if (string.IsNullOrWhiteSpace(codigo))
                throw new MissingRequiredFieldException("Código");

            if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < TamanhoMinimoSenha)
                throw new MissingRequiredFieldException($"Nova senha com ao menos {TamanhoMinimoSenha} caracteres");

            var normalizado = NormalizarEmail(email);
            var login = await _dbContext.Logins.FirstOrDefaultAsync(l => l.Email.ToLower() == normalizado);

            var agora = DateTime.UtcNow;
            var registro = login == null ? null : await _dbContext.CodigosRedefinicaoSenha
                .Where(c => c.ContaId == login.ContaId && !c.Usado && c.ExpiraEm > agora)
                .OrderByDescending(c => c.CriadoEm)
                .FirstOrDefaultAsync();

            if (login == null || registro == null)
                throw new InvalidCredentialsException("Código inválido ou expirado. Peça um novo código.");

            if (!BCrypt.Net.BCrypt.Verify(codigo.Trim(), registro.CodigoHash))
            {
                registro.Tentativas++;
                if (registro.Tentativas >= MaximoTentativas)
                    registro.Usado = true;
                await _dbContext.SaveChangesAsync();

                throw new InvalidCredentialsException(registro.Usado
                    ? "Muitas tentativas erradas. Peça um novo código."
                    : "Código incorreto.");
            }

            registro.Usado = true;
            login.Senha = BCrypt.Net.BCrypt.HashPassword(novaSenha);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Verifica se já existe um login com o email informado.
        /// </summary>
        public Task<bool> EmailJaCadastradoAsync(string email)
        {
            var normalizado = NormalizarEmail(email);
            return _dbContext.Logins.AnyAsync(l => l.Email.ToLower() == normalizado);
        }

        /// <summary>
        /// Adiciona um novo registro de Login ao contexto, SEM salvar.
        /// Quem chama deve executar SaveChangesAsync junto com o cadastro do perfil,
        /// para que perfil e login sejam gravados na mesma transação.
        /// </summary>
        /// <param name="dto">DTO contendo informações do novo login (ContaId, Email, Senha, TipoUsuario)</param>
        /// <returns>Objeto Login adicionado</returns>
        /// <exception cref="MissingRequiredFieldException">Quando campos obrigatórios estão ausentes</exception>
        public Login AdicionarLogin(CreateLoginDto dto)
        {
            // Validação de campos obrigatórios
            if (dto.ContaId == Guid.Empty)
                throw new MissingRequiredFieldException("ContaId");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new MissingRequiredFieldException("Email");

            if (string.IsNullOrWhiteSpace(dto.Senha))
                throw new MissingRequiredFieldException("Senha");

            var novoLogin = new Login
            {
                ContaId = dto.ContaId,
                Email = NormalizarEmail(dto.Email),
                Senha = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                TipoUsuario = dto.TipoUsuario
            };

            _dbContext.Logins.Add(novoLogin);
            return novoLogin;
        }

        /// <summary>
        /// Atualiza o email do login quando o perfil troca de email, SEM salvar.
        /// </summary>
        /// <returns>false se o novo email já pertence a outra conta</returns>
        public async Task<bool> SincronizarEmailAsync(Guid contaId, string novoEmail)
        {
            var normalizado = NormalizarEmail(novoEmail);

            var emUso = await _dbContext.Logins
                .AnyAsync(l => l.ContaId != contaId && l.Email.ToLower() == normalizado);
            if (emUso)
                return false;

            var login = await _dbContext.Logins.FirstOrDefaultAsync(l => l.ContaId == contaId);
            if (login != null)
                login.Email = normalizado;

            return true;
        }

        /// <summary>
        /// Troca a senha da conta, exigindo a senha atual.
        /// </summary>
        /// <exception cref="InvalidCredentialsException">Quando a senha atual não confere</exception>
        public async Task AlterarSenhaAsync(Guid contaId, string senhaAtual, string novaSenha)
        {
            if (string.IsNullOrWhiteSpace(senhaAtual))
                throw new MissingRequiredFieldException("Senha atual");

            if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < TamanhoMinimoSenha)
                throw new MissingRequiredFieldException($"Nova senha com ao menos {TamanhoMinimoSenha} caracteres");

            var login = await _dbContext.Logins.FirstOrDefaultAsync(l => l.ContaId == contaId);

            if (login == null || !BCrypt.Net.BCrypt.Verify(senhaAtual, login.Senha))
                throw new InvalidCredentialsException();

            login.Senha = BCrypt.Net.BCrypt.HashPassword(novaSenha);
            await _dbContext.SaveChangesAsync();
        }
    }
}
