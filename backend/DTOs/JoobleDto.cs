using Newtonsoft.Json;

namespace OpenDoors.Api.DTOs
{
    // Resposta raiz da Jooble API
    public class JoobleResponseDto
    {
        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("jobs")]
        public List<JoobleJobDto> Jobs { get; set; } = new();
    }

    // Cada vaga retornada pela Jooble API
    public class JoobleJobDto
    {
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("location")]
        public string Location { get; set; } = string.Empty;

        [JsonProperty("snippet")]
        public string Snippet { get; set; } = string.Empty;

        [JsonProperty("salary")]
        public string? Salary { get; set; }

        [JsonProperty("source")]
        public string? Source { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("link")]
        public string? Link { get; set; }

        [JsonProperty("company")]
        public string? Company { get; set; }

        [JsonProperty("updated")]
        public string? Updated { get; set; }

        [JsonProperty("id")]
        public string? ExternalId { get; set; }
    }

    // Parâmetros de busca que o controller recebe do frontend
    public class JoobleBuscaDto
    {
        // Cargo ou área: "Engenharia de Software", "Marketing", etc.
        public string? Keywords { get; set; }

        // Cidade/Estado: "São Paulo", "Remoto", etc.
        public string? Location { get; set; }

        // Página (começa em 1)
        public int Page { get; set; } = 1;

        // Resultados por página (máx. recomendado: 20)
        public int ResultsPerPage { get; set; } = 10;
    }
}
