using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDoors.Api.Models
{
    [Table("empresas")]
    public class Empresa : BaseModel
    {
        // ⚠️ ATENÇÃO: id da empresa é UUID (vem do Supabase Auth)
        // E é "shouldInsert: true" porque a gente PRECISA enviar o id
        // (não é o banco que gera)
        [PrimaryKey("id", true)]
        public Guid Id { get; set; }

        [Column("razao_social")]
        public string RazaoSocial { get; set; } = string.Empty;

        [Column("nome_fantasia")]
        public string? NomeFantasia { get; set; }

        [Column("cnpj")]
        public string Cnpj { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("telefone")]
        public string? Telefone { get; set; }

        [Column("site")]
        public string? Site { get; set; }

        [Column("logo_url")]
        public string? LogoUrl { get; set; }

        [Column("cidade")]
        public string? Cidade { get; set; }

        [Column("estado")]
        public string? Estado { get; set; }

        [Column("cep")]
        public string? Cep { get; set; }

        [Column("setor")]
        public string? Setor { get; set; }

        [Column("porte")]
        public string? Porte { get; set; }

        [Column("descricao")]
        public string? Descricao { get; set; }

        [Column("responsavel_nome")]
        public string? ResponsavelNome { get; set; }

        [Column("responsavel_cargo")]
        public string? ResponsavelCargo { get; set; }

        [Column("responsavel_email")]
        public string? ResponsavelEmail { get; set; }

        [Column("vagas_ativas")]
        public int? VagasAtivas { get; set; }

        [Column("total_contratacoes")]
        public int? TotalContratacoes { get; set; }

        [Column("criado_por_admin")]
        public bool? CriadoPorAdmin { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("criado_em")]
        public DateTime? CriadoEm { get; set; }

        [Column("atualizado_em")]
        public DateTime? AtualizadoEm { get; set; }
    }
}