using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDoors.Api.Models
{
    [Table("matches")]
    public class Match : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        // Conexões — quem está sendo "matchado" com qual vaga de qual empresa
        [Column("estudante_id")]
        public Guid EstudanteId { get; set; }

        [Column("vaga_id")]
        public int VagaId { get; set; }

        [Column("empresa_id")]
        public Guid EmpresaId { get; set; }

        // 🎯 SCORE TOTAL — número de 0 a 100 dizendo o quão bom é o match
        // Esse é o campo "estrela" que a IA da Claude vai calcular
        [Column("score_total")]
        public decimal ScoreTotal { get; set; }

        // 📊 SCORES PARCIAIS — quebra do score total em 3 dimensões
        // Isso permite mostrar PRO QUE o estudante combina, não só "combina ou não"

        [Column("score_curriculo")]
        public decimal? ScoreCurriculo { get; set; }

        [Column("score_vocacional")]
        public decimal? ScoreVocacional { get; set; }

        [Column("score_habilidades")]
        public decimal? ScoreHabilidades { get; set; }

        // 💬 ANÁLISE QUALITATIVA gerada pela IA

        // Lista de pontos onde o estudante se destaca
        // Ex: ["Domina C# e .NET", "Já fez projeto similar", "Disponibilidade compatível"]
        [Column("pontos_fortes")]
        public List<string>? PontosFortes { get; set; }

        // Lista de pontos a melhorar
        // Ex: ["Falta experiência com Docker", "Não tem inglês fluente"]
        [Column("pontos_fracos")]
        public List<string>? PontosFracos { get; set; }

        // Texto narrativo explicando o match
        // Ex: "Carlos é um excelente candidato porque..."
        [Column("justificativa")]
        public string? Justificativa { get; set; }

        [Column("gerado_em")]
        public DateTime? GeradoEm { get; set; }

        [Column("atualizado_em")]
        public DateTime? AtualizadoEm { get; set; }
    }
}