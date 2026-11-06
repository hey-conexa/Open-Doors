
namespace OpenDoors.Api.Models
{
    public class Empresa
    {
        // UUID gerado pelo backend no cadastro (é o conta_id do login)
        public Guid Id { get; set; }

        public string RazaoSocial { get; set; } = string.Empty;

        public string? NomeFantasia { get; set; }

        public string Cnpj { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Telefone { get; set; }

        public string? Site { get; set; }

        public string? LogoUrl { get; set; }

        public string? Cidade { get; set; }

        public string? Estado { get; set; }

        public string? Cep { get; set; }

        public string? Setor { get; set; }

        public string? Porte { get; set; }

        public string? Descricao { get; set; }

        public string? ResponsavelNome { get; set; }

        public string? ResponsavelCargo { get; set; }

        public string? ResponsavelEmail { get; set; }

        public int? VagasAtivas { get; set; }

        public int? TotalContratacoes { get; set; }

        public bool? CriadoPorAdmin { get; set; }

        public string? Status { get; set; }

        public DateTime? CriadoEm { get; set; }

        public DateTime? AtualizadoEm { get; set; }
    }
}