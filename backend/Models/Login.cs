using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDoors.Api.Models
{
    public class Login
    {
        // ID auto-incremento
        public int Id { get; set; }

        // FK — referência à conta (usuário)
        public Guid ContaId { get; set; }

        // Email do usuário
        public string Email { get; set; } = string.Empty;

        // Senha do usuário
        public string Senha { get; set; } = string.Empty;

        public TiposUsuario TipoUsuario { get; set; }
    }

    public enum TiposUsuario
    {
        Estudante,
        Empresa
    }
}
