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
        private readonly AppDbContext _dbContext;
        private readonly JwtService _jwtService;

        public LoginService(AppDbContext dbContext, JwtService jwtService)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Autentica um usuário validando email e senha contra o banco de dados.
        /// </summary>
        /// <param name="dto">DTO contendo ContaId, Email e Senha</param>
        /// <returns>DTO com o token JWT gerado</returns>
        /// <exception cref="MissingRequiredFieldException">Quando campos obrigatórios estão ausentes</exception>
        /// <exception cref="InvalidCredentialsException">Quando email ou senha são inválidos</exception>
        public async Task<LoginResponseDto> AutenticarAsync(LoginDto dto)
        {
            // Validação de campos obrigatórios
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new MissingRequiredFieldException("Email");

            if (string.IsNullOrWhiteSpace(dto.Senha))
                throw new MissingRequiredFieldException("Senha");

            if (dto.ContaId == Guid.Empty)
                throw new MissingRequiredFieldException("ContaId");

            // Busca a conta no banco de dados pela chave primária ContaId
            var loginResult = await _dbContext.Logins
                .FirstOrDefaultAsync(l => l.ContaId == dto.ContaId);

            if (loginResult == null)
                throw new InvalidCredentialsException();

            // Compara o email informado com o armazenado
            if (!loginResult.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase))
                throw new InvalidCredentialsException();

            // Verifica a senha utilizando BCrypt
            if (!BCrypt.Net.BCrypt.Verify(dto.Senha, loginResult.Senha))
                throw new InvalidCredentialsException();

            // Gera o token JWT
            var token = _jwtService.CriarToken(loginResult);

            // Retorna o DTO com o token
            return new LoginResponseDto { Token = token };
        }

        /// <summary>
        /// Cria um novo registro de Login no banco de dados.
        /// </summary>
        /// <param name="dto">DTO contendo informações do novo login (ContaId, Email, Senha, TipoUsuario)</param>
        /// <returns>Objeto Login criado</returns>
        /// <exception cref="MissingRequiredFieldException">Quando campos obrigatórios estão ausentes</exception>
        public async Task<Login> CriarLoginAsync(CreateLoginDto dto)
        {
            // Validação de campos obrigatórios
            if (dto.ContaId == Guid.Empty)
                throw new MissingRequiredFieldException("ContaId");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new MissingRequiredFieldException("Email");

            if (string.IsNullOrWhiteSpace(dto.Senha))
                throw new MissingRequiredFieldException("Senha");

            // Criptografa a senha usando BCrypt
            var senhaHasheada = BCrypt.Net.BCrypt.HashPassword(dto.Senha);

            // Cria novo registro de Login
            var novoLogin = new Login
            {
                ContaId = dto.ContaId,
                Email = dto.Email,
                Senha = senhaHasheada,
                TipoUsuario = dto.TipoUsuario
            };

            // Insere no banco de dados
            await _dbContext.Logins.AddAsync(novoLogin);
            await _dbContext.SaveChangesAsync();

            return novoLogin;
        }
    }
}