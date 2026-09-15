using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace OpenDoors.Api.Models
{
    // [Table] diz pro Supabase qual tabela do banco essa classe representa
    [Table("vagas")]
    public class Vaga : BaseModel
    {
        // [PrimaryKey] marca a chave primária. O false significa que não é gerada pelo cliente (é o banco que gera)
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        // [Column] diz qual coluna do banco essa propriedade representa
        [Column("empresa_id")]
        public Guid EmpresaId { get; set; }

        [Column("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Column("descricao")]
        public string? Descricao { get; set; }

        [Column("area")]
        public string? Area { get; set; }

        [Column("nivel")]
        public string? Nivel { get; set; }

        [Column("cursos_aceitos")]
        public List<string>? CursosAceitos { get; set; }

        [Column("semestre_minimo")]
        public int? SemestreMinimo { get; set; }

        [Column("habilidades_requeridas")]
        public List<string>? HabilidadesRequeridas { get; set; }

        [Column("habilidades_diferenciais")]
        public List<string>? HabilidadesDiferenciais { get; set; }

        [Column("carga_horaria")]
        public string? CargaHoraria { get; set; }

        [Column("modalidade")]
        public string? Modalidade { get; set; }

        [Column("cidade")]
        public string? Cidade { get; set; }

        [Column("estado")]
        public string? Estado { get; set; }

        [Column("bolsa")]
        public decimal? Bolsa { get; set; }

        [Column("beneficios")]
        public List<string>? Beneficios { get; set; }

        [Column("vagas_disponiveis")]
        public int? VagasDisponiveis { get; set; }

        [Column("candidaturas_recebidas")]
        public int? CandidaturasRecebidas { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("criado_em")]
        public DateTime? CriadoEm { get; set; }

        [Column("expira_em")]
        public DateTime? ExpiraEm { get; set; }

        [Column("atualizado_em")]
        public DateTime? AtualizadoEm { get; set; }
    }
}