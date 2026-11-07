namespace OpenDoors.Api.Models
{
    public class PerguntaTeste
    {
        public int Id { get; set; }

        public string Pergunta { get; set; } = string.Empty;

        public int Ordem { get; set; }

        public bool Ativa { get; set; } = true;

        // Ex: "RIASEC_Realista", "BigFive_Abertura", "mensal_lideranca"
        public string Categoria { get; set; } = "geral";

        // Null = pergunta fixa permanente
        // Preenchido = pergunta mensal, ex: "2025-06"
        public string? MesReferencia { get; set; }

        // "fixa" ou "mensal"
        public string Tipo { get; set; } = "fixa";

        public DateTime? CriadoEm { get; set; }
    }
}
