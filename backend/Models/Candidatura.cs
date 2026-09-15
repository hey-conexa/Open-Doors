using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDoors.Api.Models
{
    [Table("candidaturas")]
    public class Candidatura : BaseModel
    {
        // ID auto-incremento (int4), igual a Vaga
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        // FKs — três conexões importantes
        [Column("estudante_id")]
        public Guid EstudanteId { get; set; }

        [Column("vaga_id")]
        public int VagaId { get; set; }

        [Column("empresa_id")]
        public Guid EmpresaId { get; set; }

        // Status da candidatura: "pendente", "analisando", "aprovada", "rejeitada"
        [Column("status")]
        public string? Status { get; set; }

        // Carta opcional que o estudante escreve
        [Column("carta_apresentacao")]
        public string? CartaApresentacao { get; set; }

        // 🔥 SCORE DA IA — campo onde a magia vai acontecer
        // A IA da Claude vai analisar o perfil do estudante x requisitos da vaga
        // e gravar aqui um número entre 0 e 100
        [Column("score_compatibilidade")]
        public decimal? ScoreCompatibilidade { get; set; }

        // Posição no ranking — ex: 1ª candidatura com maior score
        [Column("posicao_ranking")]
        public int? PosicaoRanking { get; set; }

        // Se a empresa já visualizou
        [Column("visualizado_empresa")]
        public bool? VisualizadoEmpresa { get; set; }

        [Column("data_visualizacao")]
        public DateTime? DataVisualizacao { get; set; }

        [Column("criado_em")]
        public DateTime? CriadoEm { get; set; }

        [Column("atualizado_em")]
        public DateTime? AtualizadoEm { get; set; }
    }
}