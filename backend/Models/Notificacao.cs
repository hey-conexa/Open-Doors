namespace OpenDoors.Api.Models
{
    public class Notificacao
    {
        public int Id { get; set; }

        // "estudante" ou "empresa" - define quem recebe
        public string DestinatarioTipo { get; set; } = string.Empty;

        // Um dos dois é null dependendo de quem recebe
        public Guid? EstudanteId { get; set; }

        public Guid? EmpresaId { get; set; }

        // Tipo: "candidatura_recebida", "match_novo", "vaga_atualizada", etc
        public string Tipo { get; set; } = string.Empty;

        public string Titulo { get; set; } = string.Empty;

        public string Mensagem { get; set; } = string.Empty;

        // Apontam pra qual registro a notificação se refere (ex: vagas, candidaturas)
        public string? ReferenciaTabela { get; set; }

        public int? ReferenciaId { get; set; }

        public bool? Lida { get; set; }

        public DateTime? DataLeitura { get; set; }

        public DateTime? CriadoEm { get; set; }
    }
}