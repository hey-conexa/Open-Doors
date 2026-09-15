using System.Text;
using Newtonsoft.Json;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Services
{
    /// <summary>
    /// Encapsula chamadas à Jooble API (jooble.org).
    /// Igual ao padrão do GroqService: recebe HttpClient e IConfiguration via DI.
    /// A API key fica em appsettings.Development.json → "Jooble:ApiKey"
    /// </summary>
    public class JoobleService
    {
        private readonly HttpClient _http;
        private readonly string _endpoint;

        public JoobleService(IConfiguration config, HttpClient http)
        {
            _http = http;

            var apiKey = config["Jooble:ApiKey"]
                ?? throw new InvalidOperationException("Jooble:ApiKey não configurada no appsettings.Development.json");

            // A Jooble API recebe a key direto na URL, sem header Authorization
            _endpoint = $"https://jooble.org/api/{apiKey}";
        }

        /// <summary>
        /// Busca vagas na Jooble API com os filtros informados.
        /// </summary>
        public async Task<JoobleResponseDto> BuscarVagasAsync(JoobleBuscaDto filtros)
        {
            var payload = new
            {
                keywords = filtros.Keywords ?? string.Empty,
                location = filtros.Location ?? "Brasil",
                page     = filtros.Page,
                ResultOnPage = filtros.ResultsPerPage
            };

            var json    = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(_endpoint, content);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JoobleResponseDto>(body)
                ?? new JoobleResponseDto();
        }

        /// <summary>
        /// Converte um JoobleJobDto para o modelo Vaga do banco,
        /// parseando cidade/estado a partir do campo "location".
        /// Usa EmpresaId nula pois são vagas externas (sem empresa cadastrada no sistema).
        /// </summary>
        public static Vaga ConverterParaVaga(JoobleJobDto job)
        {
            // location vem no formato "Cidade, Estado" ou só "Cidade"
            var partes  = job.Location?.Split(',') ?? Array.Empty<string>();
            var cidade  = partes.Length > 0 ? partes[0].Trim() : null;
            var estado  = partes.Length > 1 ? partes[1].Trim() : null;

            // Tipo do contrato → modalidade
            var modalidade = job.Type?.ToLower() switch
            {
                var t when t != null && t.Contains("remot") => "Remoto",
                var t when t != null && t.Contains("part")  => "Part-time",
                _ => "Presencial"
            };

            return new Vaga
            {
                // Não tem EmpresaId real — vagas externas ficam com Guid.Empty
                // Você pode criar uma "empresa virtual" para vagas externas futuramente
                EmpresaId  = Guid.Empty,
                Titulo     = job.Title,
                Descricao  = job.Snippet,
                Area       = null,   // Jooble não retorna área categorizada
                Nivel      = null,
                Modalidade = modalidade,
                Cidade     = cidade,
                Estado     = estado,
                Status     = "aberta",
                CriadoEm   = DateTime.UtcNow,
                // Expira em 30 dias (padrão para vagas importadas)
                ExpiraEm  = DateTime.UtcNow.AddDays(30)
            };
        }
    }
}
