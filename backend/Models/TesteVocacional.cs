using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDoors.Api.Models
{
    [Table("testes_vocacionais")]
    public class TesteVocacional : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        // UNIQUE — cada estudante tem 1 teste só
        [Column("estudante_id")]
        public Guid EstudanteId { get; set; }

        // Perfil dominante: "Analítico", "Criativo", "Comunicativo", etc
        [Column("perfil_dominante")]
        public string? PerfilDominante { get; set; }

        // Áreas que combinam com o perfil
        // Ex: ["Análise de Dados", "Engenharia de Software", "BI"]
        [Column("areas_sugeridas")]
        public List<string>? AreasSugeridas { get; set; }

        // Pontos fortes identificados
        [Column("pontos_fortes")]
        public List<string>? PontosFortes { get; set; }

        // Descrição em texto do perfil
        [Column("descricao_perfil")]
        public string? DescricaoPerfil { get; set; }

        // Flag: a IA já analisou esse teste?
        [Column("analisado_ia")]
        public bool? AnalisadoIa { get; set; }

        [Column("concluido_em")]
        public DateTime? ConcluidoEm { get; set; }
    }
}