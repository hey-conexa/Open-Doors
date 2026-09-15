using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDoors.Api.Models
{
    [Table("notificacoes")]
    public class Notificacao : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        // "estudante" ou "empresa" - define quem recebe
        [Column("destinatario_tipo")]
        public string DestinatarioTipo { get; set; } = string.Empty;

        // Um dos dois é null dependendo de quem recebe
        [Column("estudante_id")]
        public Guid? EstudanteId { get; set; }

        [Column("empresa_id")]
        public Guid? EmpresaId { get; set; }

        // Tipo: "candidatura_recebida", "match_novo", "vaga_atualizada", etc
        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [Column("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Column("mensagem")]
        public string Mensagem { get; set; } = string.Empty;

        // Apontam pra qual registro a notificação se refere (ex: vagas, candidaturas)
        [Column("referencia_tabela")]
        public string? ReferenciaTabela { get; set; }

        [Column("referencia_id")]
        public int? ReferenciaId { get; set; }

        [Column("lida")]
        public bool? Lida { get; set; }

        [Column("data_leitura")]
        public DateTime? DataLeitura { get; set; }

        [Column("criado_em")]
        public DateTime? CriadoEm { get; set; }
    }
}