namespace OpenDoors.Api.DTOs
{
    // Enviado pelo estudante ao responder o teste
    public class ResponderTesteDto
    {
        public Guid EstudanteId { get; set; }
        public List<RespostaItemDto> Respostas { get; set; } = new();
    }

    // Cada item da lista de respostas
    public class RespostaItemDto
    {
        public int PerguntaId { get; set; }
        public string Pergunta { get; set; } = string.Empty;
        public string Resposta { get; set; } = string.Empty;
    }
}
