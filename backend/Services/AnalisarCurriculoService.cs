using System.Text;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Infrastructure.Database;
using OpenDoors.Api.Models;
using UglyToad.PdfPig;

namespace OpenDoors.Api.Services
{
    /// <summary>
    /// Analisa currículo (PDF) usando IA, extrai habilidades e salva no banco.
    /// O PDF fica guardado no PostgreSQL (tabela "curriculos") e é servido por
    /// GET /api/estudantes/{id}/curriculo.
    /// </summary>
    public class AnalisarCurriculoService
    {
        public const long TamanhoMaximoBytes = 5 * 1024 * 1024; // 5 MB

        private readonly GroqService _groq;
        private readonly AppDbContext _dbContext;

        public AnalisarCurriculoService(GroqService groq, AppDbContext dbContext)
        {
            _groq = groq;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Endereço (relativo à API) para baixar o currículo do estudante.
        /// </summary>
        public static string UrlCurriculo(Guid estudanteId) => $"/api/estudantes/{estudanteId}/curriculo";

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
            var estudante = await _dbContext.Estudantes
                .FirstOrDefaultAsync(e => e.Id == estudanteId);

            if (estudante == null)
                throw new KeyNotFoundException($"Estudante não encontrado: {estudanteId}");

            // Lê o arquivo em memória (usado duas vezes: extração do texto + gravação no banco)
            using var ms = new MemoryStream();
            await pdfStream.CopyToAsync(ms);

            if (ms.Length > TamanhoMaximoBytes)
                throw new InvalidOperationException("O currículo deve ter no máximo 5 MB.");

            var bytes = ms.ToArray();
            if (bytes.Length < 5 || Encoding.ASCII.GetString(bytes, 0, 5) != "%PDF-")
                throw new InvalidOperationException("O arquivo enviado não é um PDF válido.");

            // 1. Extrai texto e analisa com IA
            string textoCurriculo;
            try
            {
                ms.Position = 0;
                textoCurriculo = ExtrairTextoPdf(ms);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                throw new InvalidOperationException("Não foi possível ler o PDF enviado.");
            }

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

            // 2. Guarda (ou substitui) o PDF no PostgreSQL
            var arquivo = await _dbContext.Curriculos.FirstOrDefaultAsync(c => c.EstudanteId == estudanteId);
            if (arquivo == null)
            {
                arquivo = new CurriculoArquivo { EstudanteId = estudanteId };
                _dbContext.Curriculos.Add(arquivo);
            }

            arquivo.NomeArquivo = string.IsNullOrWhiteSpace(fileName) ? "curriculo.pdf" : Path.GetFileName(fileName);
            arquivo.ContentType = "application/pdf";
            arquivo.Conteudo = bytes;
            arquivo.Texto = textoCurriculo;
            arquivo.EnviadoEm = DateTime.UtcNow;

            // 3. Atualiza o estudante com habilidades + endereço do currículo
            estudante.HabilidadesExtraidas = dados.Habilidades;
            estudante.TemCurriculo = true;
            estudante.CurriculoUrl = UrlCurriculo(estudanteId);

            await _dbContext.SaveChangesAsync();

            // Devolve o endereço para o frontend também
            dados.CurriculoUrl = estudante.CurriculoUrl;

            return dados;
        }
    }

}
