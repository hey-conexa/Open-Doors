namespace OpenDoors.Api.Models
{
    /// <summary>
    /// PDF do currículo do estudante, guardado no próprio PostgreSQL (um por estudante).
    /// O texto extraído fica salvo junto para a IA não precisar ler o PDF de novo.
    /// </summary>
    public class CurriculoArquivo
    {
        // ID auto-incremento
        public int Id { get; set; }

        // Estudante dono do currículo (único: um novo envio substitui o anterior)
        public Guid EstudanteId { get; set; }

        public string NomeArquivo { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/pdf";

        // Conteúdo do PDF (bytea)
        public byte[] Conteudo { get; set; } = Array.Empty<byte>();

        // Texto extraído do PDF
        public string Texto { get; set; } = string.Empty;

        public DateTime EnviadoEm { get; set; }
    }
}
