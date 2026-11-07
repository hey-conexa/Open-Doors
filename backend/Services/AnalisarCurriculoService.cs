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

        private static List<string> ExtrairHabilidadesMencionadas(string texto)
        {
            var catalogo = new[]
            {
                "C#", ".NET", "ASP.NET Core", "Entity Framework Core", "Node.js", "TypeScript", "JavaScript", "React", "Python", "Express",
                "SQL Server", "PostgreSQL", "Redis", "BullMQ", "SOLID", "POO", "Clean Architecture", "Design Patterns", "JWT",
                "xUnit", "AutoMapper", "Git", "GitHub", "Swagger", "VBA", "Power BI", "Excel", "Scrum", "Comunicação",
                "Trabalho em equipe", "Resolução de problemas", "Proatividade", "Liderança", "Pensamento analítico", "Gestão do tempo"
            };

            return catalogo
                .Where(habilidade => texto.Contains(habilidade, StringComparison.OrdinalIgnoreCase))
                .ToList();
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
                                Analise este currículo fielmente e retorne um JSON com:
                {
                                    "habilidades": ["todas as linguagens, frameworks, bibliotecas, bancos, ferramentas, metodologias e competências comportamentais mencionadas"],
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

                Regras obrigatórias:
                - Não limite a resposta a seis itens. Liste todas as habilidades explicitamente presentes no currículo.
                - Preserve os nomes como aparecem no currículo, incluindo C#, .NET, ASP.NET Core, bancos, ferramentas e metodologias.
                - Não invente habilidades que não estejam no texto.

                Currículo:
                {{textoCurriculo}}
                """;

            var dados = await _groq.ChatJsonAsync<CurriculoAnalisadoDto>(system, user);
            var habilidadesMencionadas = ExtrairHabilidadesMencionadas(textoCurriculo);
            dados.Habilidades = dados.Habilidades
                .Concat(habilidadesMencionadas)
                .Where(habilidade => !string.IsNullOrWhiteSpace(habilidade))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

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
