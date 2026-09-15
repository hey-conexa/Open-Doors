namespace OpenDoors.Api.DTOs
{
    public class TesteRespostaDto
    {
        public int Id { get; set; }
        public int TesteId { get; set; }
        public int PerguntaId { get; set; }
        public string Pergunta { get; set; } = string.Empty;
        public string Resposta { get; set; } = string.Empty;
    }
}