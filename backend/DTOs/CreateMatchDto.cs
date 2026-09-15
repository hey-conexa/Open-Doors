namespace OpenDoors.Api.DTOs
{
    public class CreateMatchDto
    {
        public Guid EstudanteId { get; set; }
        public int VagaId { get; set; }
        public Guid EmpresaId { get; set; }
        public decimal ScoreTotal { get; set; }
        public decimal? ScoreCurriculo { get; set; }
        public decimal? ScoreVocacional { get; set; }
        public decimal? ScoreHabilidades { get; set; }
        public List<string>? PontosFortes { get; set; }
        public List<string>? PontosFracos { get; set; }
        public string? Justificativa { get; set; }
    }
}