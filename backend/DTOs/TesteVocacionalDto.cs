namespace OpenDoors.Api.DTOs
{
    public class TesteVocacionalDto
    {
        public int Id { get; set; }
        public Guid EstudanteId { get; set; }
        public string? PerfilDominante { get; set; }
        public List<string>? AreasSugeridas { get; set; }
        public List<string>? PontosFortes { get; set; }
        public string? DescricaoPerfil { get; set; }
        public bool? AnalisadoIa { get; set; }
        public DateTime? ConcluidoEm { get; set; }
    }
}