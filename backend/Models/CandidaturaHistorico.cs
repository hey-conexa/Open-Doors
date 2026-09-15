using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDoors.Api.Models
{
    [Table("candidaturas_historico")]
    public class CandidaturaHistorico : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("candidatura_id")]
        public int CandidaturaId { get; set; }

        [Column("status")]
        public string Status { get; set; } = string.Empty;

        [Column("observacao")]
        public string? Observacao { get; set; }

        [Column("criado_em")]
        public DateTime? CriadoEm { get; set; }
    }
}