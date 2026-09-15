namespace OpenDoors.Api.DTOs
{
    // ===========================================
    // REQUESTS (entrada dos endpoints)
    // ===========================================

    public class AnalisarTesteRequestDto
    {
        public Guid EstudanteId { get; set; }
        public List<RespostaVocacionalDto> Respostas { get; set; } = new();
    }

    public class RespostaVocacionalDto
    {
        public int PerguntaId { get; set; }
        public string Pergunta { get; set; } = "";
        public string Resposta { get; set; } = ""; // valor 1-7 (escala Likert)

        // Categoria da pergunta (ex: RIASEC_Social, BigFive_Abertura)
        // Opcional — se não vier do frontend, o service usa "geral"
        public string? Categoria { get; set; }
    }

    public class GerarScoreRequestDto
    {
        public Guid EstudanteId { get; set; }
        public int VagaId { get; set; }
    }

    // ===========================================
    // RESPONSES (saída dos endpoints)
    // ===========================================

    public class CurriculoAnalisadoDto
    {
        public List<string> Habilidades { get; set; } = new();
        public List<ExperienciaDto> Experiencias { get; set; } = new();
        public string NivelExperiencia { get; set; } = "";
        public List<string> AreasAtuacao { get; set; } = new();
        public string? CurriculoUrl { get; set; }
    }

    public class ExperienciaDto
    {
        public string Cargo { get; set; } = "";
        public string Empresa { get; set; } = "";
        public string Periodo { get; set; } = "";
        public string Descricao { get; set; } = "";
    }

    public class PerfilVocacionalDto
    {
        public string PerfilDominante { get; set; } = "";
        public List<string> AreasSugeridas { get; set; } = new();
        public List<string> PontosFortes { get; set; } = new();
        public string DescricaoPerfil { get; set; } = "";
    }

    public class ScoreCompatibilidadeDto
    {
        public decimal ScoreTotal { get; set; }
        public decimal ScoreCurriculo { get; set; }
        public decimal ScoreVocacional { get; set; }
        public decimal ScoreHabilidades { get; set; }
        public List<string> PontosFortes { get; set; } = new();
        public List<string> PontosFracos { get; set; } = new();
        public string Justificativa { get; set; } = "";
    }
}
