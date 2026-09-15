using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDoors.Api.Models
{
    [Table("estudantes")]
    public class Estudante : BaseModel
    {
        // UUID vindo do Supabase Auth (igual a Empresa)
        [PrimaryKey("id", true)]
        public Guid Id { get; set; }

        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("telefone")]
        public string? Telefone { get; set; }

        [Column("cpf")]
        public string? Cpf { get; set; }

        [Column("data_nascimento")]
        public DateTime? DataNascimento { get; set; }

        [Column("cidade")]
        public string? Cidade { get; set; }

        [Column("estado")]
        public string? Estado { get; set; }

        [Column("foto_perfil_url")]
        public string? FotoPerfilUrl { get; set; }

        [Column("instituicao")]
        public string? Instituicao { get; set; }

        [Column("curso")]
        public string? Curso { get; set; }

        [Column("semestre")]
        public int? Semestre { get; set; }

        [Column("turno")]
        public string? Turno { get; set; }

        [Column("previsao_conclusao")]
        public string? PrevisaoConclusao { get; set; }

        [Column("curriculo_url")]
        public string? CurriculoUrl { get; set; }

        [Column("habilidades_extraidas")]
        public List<string>? HabilidadesExtraidas { get; set; }

        [Column("tem_curriculo")]
        public bool? TemCurriculo { get; set; }

        [Column("tem_teste_vocacional")]
        public bool? TemTesteVocacional { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("criado_em")]
        public DateTime? CriadoEm { get; set; }

        [Column("atualizado_em")]
        public DateTime? AtualizadoEm { get; set; }
    }
}