namespace OpenDoors.Api.DTOs
{
    public class NotificacaoDto
    {
        public int Id { get; set; }
        public string DestinatarioTipo { get; set; } = string.Empty;
        public Guid? EstudanteId { get; set; }
        public Guid? EmpresaId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public string? ReferenciaTabela { get; set; }
        public int? ReferenciaId { get; set; }
        public bool? Lida { get; set; }
        public DateTime? DataLeitura { get; set; }
        public DateTime? CriadoEm { get; set; }
    }
}