namespace OpenDoors.Api.DTOs
{
    public class CandidaturaDto
    {
        public int Id { get; set; }
        public Guid EstudanteId { get; set; }
        public int VagaId { get; set; }
        public Guid EmpresaId { get; set; }
        public string? Status { get; set; }
        public string? CartaApresentacao { get; set; }
        public decimal? ScoreCompatibilidade { get; set; }
        public int? PosicaoRanking { get; set; }
        public bool? VisualizadoEmpresa { get; set; }
        public DateTime? DataVisualizacao { get; set; }
        public DateTime? CriadoEm { get; set; }
        public DateTime? AtualizadoEm { get; set; }
    }
}