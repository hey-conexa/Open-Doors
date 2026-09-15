using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDoors.Api.Models
{
    [Table("testes_respostas")]
    public class TesteResposta : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("teste_id")]
        public int TesteId { get; set; }

        [Column("pergunta_id")]
        public int PerguntaId { get; set; }

        [Column("pergunta")]
        public string Pergunta { get; set; } = string.Empty;

        [Column("resposta")]
        public string Resposta { get; set; } = string.Empty;
    }
}