using System.Text;
using Microsoft.Extensions.Configuration;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Models;
using UglyToad.PdfPig;

namespace OpenDoors.Api.Services
{
    /// <summary>
    /// Analisa currículo (PDF) usando IA, extrai habilidades e salva no Supabase.
    /// </summary>
    public class AnalisarCurriculoService
    {
        private readonly GroqService _groq;
        private readonly Supabase.Client _supabase;
        private readonly string _supabaseUrl;

        public AnalisarCurriculoService(GroqService groq, Supabase.Client supabase, IConfiguration config)
        {
            _groq = groq;
            _supabase = supabase;
            _supabaseUrl = config["Supabase:Url"]?.TrimEnd('/') ?? "";
        }

        /// <summary>
        /// Extrai texto de todas as páginas do PDF.
        /// </summary>
        public static string ExtrairTextoPdf(Stream pdfStream)
        {
            var sb = new StringBuilder();
            using var doc = PdfDocument.Open(pdfStream);
            foreach (var pagina in doc.GetPages())
            {
                foreach (var palavra in pagina.GetWords())
                    sb.Append(palavra.Text).Append(' ');
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }

        public async Task<CurriculoAnalisadoDto> AnalisarAsync(Guid estudanteId, Stream pdfStream, string fileName = "curriculo.pdf")
        {
            // Lê o stream completo em memória (precisamos usar duas vezes: upload + extração)
            using var ms = new MemoryStream();
            await pdfStream.CopyToAsync(ms);
            ms.Position = 0;

            // 1. Faz upload para o Supabase Storage (bucket "curriculos")
            string? curriculoUrl = null;
            try
            {
                var storagePath = $"{estudanteId}/{DateTime.UtcNow:yyyyMMddHHmmss}_{fileName}";
                var bytes = ms.ToArray();
                await _supabase.Storage
                    .From("curriculos")
                    .Upload(bytes, storagePath, new Supabase.Storage.FileOptions { ContentType = "application/pdf", Upsert = true });

                curriculoUrl = $"{_supabaseUrl}/storage/v1/object/public/curriculos/{storagePath}";
            }
            catch
            {
                // Upload falhou — continua sem URL (não bloqueia a análise)
            }

            // 2. Extrai texto e analisa com IA
            ms.Position = 0;
            var textoCurriculo = ExtrairTextoPdf(ms);

            if (string.IsNullOrWhiteSpace(textoCurriculo))
                throw new InvalidOperationException("O PDF enviado não contém texto legível.");

            const string system =
                "Você é um especialista em RH e análise de currículos. " +
                "Analise o currículo fornecido e extraia as informações em JSON. " +
                "Responda APENAS com JSON válido, sem texto adicional, sem markdown, sem ```json.";

            var user = $$"""
                Analise este currículo e retorne um JSON com:
                {
                  "habilidades": ["lista", "de", "habilidades", "técnicas"],
                  "experiencias": [
                    {
                      "cargo": "nome do cargo",
                      "empresa": "nome da empresa",
                      "periodo": "período",
                      "descricao": "descrição resumida"
                    }
                  ],
                  "nivelExperiencia": "junior/pleno/senior",
                  "areasAtuacao": ["área1", "área2"]
                }

                Currículo:
                {{textoCurriculo}}
                """;

            var dados = await _groq.ChatJsonAsync<CurriculoAnalisadoDto>(system, user);

            // 3. Atualiza o estudante no banco com habilidades + URL do currículo
            var estudante = await _supabase
                .From<Estudante>()
                .Where(e => e.Id == estudanteId)
                .Single();

            if (estudante == null)
                throw new KeyNotFoundException($"Estudante não encontrado: {estudanteId}");

            estudante.HabilidadesExtraidas = dados.Habilidades;
            estudante.TemCurriculo = true;
            if (curriculoUrl != null)
                estudante.CurriculoUrl = curriculoUrl;

            await estudante.Update<Estudante>();

            // Devolve a URL para o frontend também
            dados.CurriculoUrl = curriculoUrl;

            return dados;
        }
    }
}