using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDoors.Api.Models
{
    [Table("perguntas_teste")]
    public class PerguntaTeste : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("pergunta")]
        public string Pergunta { get; set; } = string.Empty;

        [Column("ordem")]
        public int Ordem { get; set; }

        [Column("ativa")]
        public bool Ativa { get; set; } = true;

        // Ex: "RIASEC_Realista", "BigFive_Abertura", "mensal_lideranca"
        [Column("categoria")]
        public string Categoria { get; set; } = "geral";

        // Null = pergunta fixa permanente
        // Preenchido = pergunta mensal, ex: "2025-06"
        [Column("mes_referencia")]
        public string? MesReferencia { get; set; }

        // "fixa" ou "mensal"
        [Column("tipo")]
        public string Tipo { get; set; } = "fixa";

        [Column("criado_em")]
        public DateTime? CriadoEm { get; set; }
    }
}
