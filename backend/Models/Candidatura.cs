using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDoors.Api.Models
{
    public class Candidatura
    {
        // ID auto-incremento (int4)
        public int Id { get; set; }

        // FKs — três conexões importantes
        public Guid EstudanteId { get; set; }

        public int VagaId { get; set; }

        public Guid EmpresaId { get; set; }

        // Status da candidatura: "pendente", "analisando", "aprovada", "rejeitada"
        public string? Status { get; set; }

        // Carta opcional que o estudante escreve
        public string? CartaApresentacao { get; set; }

        // 🔥 SCORE DA IA — campo onde a magia vai acontecer
        // A IA da Claude vai analisar o perfil do estudante x requisitos da vaga
        // e gravar aqui um número entre 0 e 100
        public decimal? ScoreCompatibilidade { get; set; }

        // Posição no ranking — ex: 1ª candidatura com maior score
        public int? PosicaoRanking { get; set; }

        // Se a empresa já visualizou
        public bool? VisualizadoEmpresa { get; set; }

        public DateTime? DataVisualizacao { get; set; }

        public DateTime? CriadoEm { get; set; }

        public DateTime? AtualizadoEm { get; set; }
    }
}