namespace OpenDoors.Api.Models
{
    public class Match
    {
        public int Id { get; set; }

        // Conexões — quem está sendo "matchado" com qual vaga de qual empresa
        public Guid EstudanteId { get; set; }

        public int VagaId { get; set; }

        public Guid EmpresaId { get; set; }

        // 🎯 SCORE TOTAL — número de 0 a 100 dizendo o quão bom é o match
        // Esse é o campo "estrela" que a IA da Claude vai calcular
        public decimal ScoreTotal { get; set; }

        // 📊 SCORES PARCIAIS — quebra do score total em 3 dimensões
        // Isso permite mostrar PRO QUE o estudante combina, não só "combina ou não"

        public decimal? ScoreCurriculo { get; set; }

        public decimal? ScoreVocacional { get; set; }

        public decimal? ScoreHabilidades { get; set; }

        // 💬 ANÁLISE QUALITATIVA gerada pela IA

        // Lista de pontos onde o estudante se destaca
        // Ex: ["Domina C# e .NET", "Já fez projeto similar", "Disponibilidade compatível"]
        public List<string>? PontosFortes { get; set; }

        // Lista de pontos a melhorar
        // Ex: ["Falta experiência com Docker", "Não tem inglês fluente"]
        public List<string>? PontosFracos { get; set; }

        // Texto narrativo explicando o match
        // Ex: "Carlos é um excelente candidato porque..."
        public string? Justificativa { get; set; }

        public DateTime? GeradoEm { get; set; }

        public DateTime? AtualizadoEm { get; set; }
    }
}