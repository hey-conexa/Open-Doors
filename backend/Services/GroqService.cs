using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace OpenDoors.Api.Services
{
    /// <summary>
    /// Encapsula chamadas à API Groq (LLM Llama 3.3).
    /// Será migrado para Claude API no período de aperfeiçoamento (24/05 - 03/06).
    /// </summary>
    public class GroqService
    {
        private readonly HttpClient _http;
        private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";

        public GroqService(IConfiguration config, HttpClient http)
        {
            _http = http;
            var apiKey = config["Groq:ApiKey"]
                ?? throw new InvalidOperationException("Groq:ApiKey não configurada no appsettings.Development.json");
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        }

        /// <summary>
        /// Envia mensagens ao modelo e retorna o texto limpo (sem markdown).
        /// </summary>
        public async Task<string> ChatAsync(string systemPrompt, string userPrompt)
        {
            var payload = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = userPrompt   }
                }
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(Endpoint, content);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            dynamic? parsed = JsonConvert.DeserializeObject(body);
            var texto = (string)(parsed?.choices[0].message.content ?? "");

            // Remove blocos de markdown caso a IA os inclua
            texto = Regex.Replace(texto, @"```json\s*", "");
            texto = Regex.Replace(texto, @"```\s*", "");

            return texto.Trim();
        }

        /// <summary>
        /// Chama ChatAsync e desserializa o resultado para o tipo T.
        /// </summary>
        public async Task<T> ChatJsonAsync<T>(string systemPrompt, string userPrompt)
        {
            var raw = await ChatAsync(systemPrompt, userPrompt);

            try
            {
                return JsonConvert.DeserializeObject<T>(raw)
                    ?? throw new InvalidOperationException("Resposta nula após deserialização.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"IA retornou JSON inválido: {ex.Message}\nConteúdo: {raw}");
            }
        }
    }
}