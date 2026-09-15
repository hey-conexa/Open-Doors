namespace OpenDoors.Api.DTOs
{
    public class CandidaturaHistoricoDto
    {
        public int Id { get; set; }
        public int CandidaturaId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Observacao { get; set; }
        public DateTime? CriadoEm { get; set; }
    }
}