using Microsoft.EntityFrameworkCore;
using OpenDoors.Api.Exceptions;
using OpenDoors.Api.Infrastructure.Database;
using OpenDoors.Api.Models;
using OpenDoors.Api.DTOs;
using BCrypt.Net;

namespace OpenDoors.Api.Services
{
    /// <summary>
    /// Serviço responsável pela autenticação de usuários.
    /// Busca credenciais no banco de dados e gera tokens JWT.
    /// </summary>
    public class LoginService
    {
        public const int TamanhoMinimoSenha = 8;

        private readonly AppDbContext _dbContext;
        private readonly JwtService _jwtService;

        public LoginService(AppDbContext dbContext, JwtService jwtService)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
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

            // Gera o token JWT
            var expiraEm = _jwtService.CalcularExpiracao();
            var token = _jwtService.CriarToken(loginResult, expiraEm);

            return new LoginResponseDto
            {
                Token = token,
                ExpiraEm = expiraEm,
                ContaId = loginResult.ContaId,
                TipoUsuario = loginResult.TipoUsuario.ToString()
            };
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
