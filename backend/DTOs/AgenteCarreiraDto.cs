namespace OpenDoors.Api.DTOs
{
    // ===========================================
    // ENTRADA: dados que o agente recebe.
    // Quem busca tudo isso no banco é o Controller.
    // ===========================================
    public class PerfilParaMelhoriaDto
    {
        public string? Curso { get; set; }
        public int? Semestre { get; set; }
        public string? Instituicao { get; set; }
        public string? PrevisaoConclusao { get; set; }
        public List<string> Habilidades { get; set; } = new();
        public string? PerfilVocacional { get; set; }
        public List<string> AreasSugeridas { get; set; } = new();
        public string TextoCurriculo { get; set; } = "";
    }

    // ===========================================
    // SAÍDA: o que o agente devolve pro front.
    // ===========================================
    public class MelhoriaCurriculoDto
    {
        public string Resumo { get; set; } = "";
        public List<string> PontosFortes { get; set; } = new();
        public List<SugestaoCurriculoDto> MelhoriasCurriculo { get; set; } = new();
        public List<CursoSugeridoDto> Cursos { get; set; } = new();
        public List<ProjetoPortfolioDto> ProjetosPortfolio { get; set; } = new();
        public List<ConteudoPublicacaoDto> ConteudosParaPublicar { get; set; } = new();
        public List<string> ProximosPassos { get; set; } = new();
    }

    public class SugestaoCurriculoDto
    {
        public string Problema { get; set; } = "";
        public string Sugestao { get; set; } = "";
    }

    public class CursoSugeridoDto
    {
        public string Tema { get; set; } = "";
        public string Motivo { get; set; } = "";
        public string Nivel { get; set; } = "";
    }

    public class ProjetoPortfolioDto
    {
        public string Titulo { get; set; } = "";
        public string Descricao { get; set; } = "";
        public List<string> Tecnologias { get; set; } = new();
        public string Demonstra { get; set; } = "";
    }

    // Ideias de conteúdo para a pessoa publicar (post no LinkedIn, tutorial, artigo...),
    // mostrando na prática o que ela sabe fazer.
    public class ConteudoPublicacaoDto
    {
        public string Formato { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Ideia { get; set; } = "";
        public string PorQue { get; set; } = "";
    }
}