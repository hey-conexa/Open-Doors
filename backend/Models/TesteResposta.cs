namespace OpenDoors.Api.Models
{
    public class TesteResposta
    {
        public int Id { get; set; }

        public int TesteId { get; set; }

        public int PerguntaId { get; set; }

        public string Pergunta { get; set; } = string.Empty;

        public string Resposta { get; set; } = string.Empty;

        public DateTime? CriadoEm { get; set; }

        public DateTime? AtualizadoEm { get; set; }
    }
}