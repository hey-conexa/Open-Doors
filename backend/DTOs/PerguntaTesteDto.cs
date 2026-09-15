namespace OpenDoors.Api.DTOs
{
    public class PerguntaTesteDto
    {
        public int Id { get; set; }
        public string Pergunta { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public string Categoria { get; set; } = "geral";
        public string? MesReferencia { get; set; }
        public string Tipo { get; set; } = "fixa";
    }

    public class CreatePerguntaTesteDto
    {
        public string Pergunta { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public string? Categoria { get; set; }
        public string? MesReferencia { get; set; }  // null = fixa, "yyyy-MM" = mensal
    }
}
