using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDoors.Api.Models
{
    [Table("testes_respostas")]
    public class TesteResposta
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        public int TesteId { get; set; }

        public int PerguntaId { get; set; }

        public string Pergunta { get; set; } = string.Empty;

        public string Resposta { get; set; } = string.Empty;
    }
}