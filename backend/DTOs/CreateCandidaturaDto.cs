namespace OpenDoors.Api.DTOs
{
    public class CreateCandidaturaDto
    {
        public Guid EstudanteId { get; set; }
        public int VagaId { get; set; }
        public Guid EmpresaId { get; set; }
        public string? Status { get; set; }
        public string? CartaApresentacao { get; set; }
    }
}