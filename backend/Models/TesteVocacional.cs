namespace OpenDoors.Api.Models
{
    public class TesteVocacional
    {
        public int Id { get; set; }

        // UNIQUE — cada estudante tem 1 teste só
        public Guid EstudanteId { get; set; }

        // Perfil dominante: "Analítico", "Criativo", "Comunicativo", etc
        public string? PerfilDominante { get; set; }

        // Áreas que combinam com o perfil
        // Ex: ["Análise de Dados", "Engenharia de Software", "BI"]
        public List<string>? AreasSugeridas { get; set; }

        // Pontos fortes identificados
        public List<string>? PontosFortes { get; set; }

        // Descrição em texto do perfil
        public string? DescricaoPerfil { get; set; }

        // Flag: a IA já analisou esse teste?
        public bool? AnalisadoIa { get; set; }

        public DateTime? ConcluidoEm { get; set; }
    }
}