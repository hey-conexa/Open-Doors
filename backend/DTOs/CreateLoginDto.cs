using OpenDoors.Api.Models;

namespace OpenDoors.Api.DTOs
{
    /// <summary>
    /// DTO para criar um novo registro de Login.
    /// </summary>
    public class CreateLoginDto
    {
        /// <summary>
        /// UUID da conta associada (referência ao usuário autenticado no Supabase Auth).
        /// </summary>
        public Guid ContaId { get; set; }

        /// <summary>
        /// Email do usuário.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usuário.
        /// NOTA: Considere usar hash (BCrypt) antes de salvar no banco de dados.
        /// </summary>
        public string Senha { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de usuário: Estudante ou Empresa.
        /// </summary>
        public TiposUsuario TipoUsuario { get; set; }
    }
}