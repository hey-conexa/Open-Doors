using Microsoft.EntityFrameworkCore;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Infrastructure.Database;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Services
{
    /// <summary>
    /// Calcula score de compatibilidade Estudante x Vaga via IA,
    /// salva o match e atualiza ranking automaticamente.
    /// </summary>
    public class GerarScoreService
    {
        private readonly GroqService _groq;
        private readonly AppDbContext _dbContext;

        public GerarScoreService(GroqService groq, AppDbContext dbContext)
        {
            _groq = groq;
            _dbContext = dbContext;
        }

        public async Task<ScoreCompatibilidadeDto> GerarAsync(Guid estudanteId, int vagaId)
        {
            // Busca dados no banco
            var estudante = await _dbContext.Estudantes
                .FirstOrDefaultAsync(e => e.Id == estudanteId);

            if (estudante == null)
                throw new KeyNotFoundException($"Estudante não encontrado: {estudanteId}");

            var vaga = await _dbContext.Vagas
                .FirstOrDefaultAsync(v => v.Id == vagaId);

            if (vaga == null)
                throw new KeyNotFoundException($"Vaga não encontrada: {vagaId}");

            var teste = await _dbContext.TestesVocacionais
                .FirstOrDefaultAsync(t => t.EstudanteId == estudanteId);

            // Monta contextos
            var habilidades = estudante.HabilidadesExtraidas ?? new List<string>();
            var temCurriculo = estudante.TemCurriculo == true || !string.IsNullOrWhiteSpace(estudante.CurriculoUrl);

            // Se tiver currículo salvo, tenta buscar o texto do PDF para enriquecer a análise
            var textoCurriculo = "";
            if (!string.IsNullOrWhiteSpace(estudante.CurriculoUrl))
            {
                try
                {
                    using var http = new System.Net.Http.HttpClient();
                    var pdfBytes = await http.GetByteArrayAsync(estudante.CurriculoUrl);
                    using var pdfStream = new System.IO.MemoryStream(pdfBytes);
                    textoCurriculo = AnalisarCurriculoService.ExtrairTextoPdf(pdfStream);
                    if (textoCurriculo.Length > 2000)
                        textoCurriculo = textoCurriculo.Substring(0, 2000) + "...";
                }
                catch { /* se falhar, usa só as habilidades */ }
            }

            var contextoEstudante = $"""
                Estudante:
                - Curso: {estudante.Curso}
                - Semestre: {estudante.Semestre}
                - Cidade: {estudante.Cidade} / {estudante.Estado}
                - Possui currículo: {(temCurriculo ? "Sim" : "Não")}
                - Habilidades extraídas do currículo: {(habilidades.Count > 0 ? string.Join(", ", habilidades) : "Nenhuma cadastrada")}
                {(string.IsNullOrWhiteSpace(textoCurriculo) ? "" : $"- Conteúdo do currículo:\n{textoCurriculo}")}
                """;

            var contextoTeste = "";
            if (teste != null)
            {
                var areas = string.Join(", ", teste.AreasSugeridas ?? new());
                var pontos = string.Join(", ", teste.PontosFortes ?? new());
                contextoTeste = $"""
                    Perfil vocacional:
                    - Perfil dominante: {teste.PerfilDominante}
                    - Áreas sugeridas: {areas}
                    - Pontos fortes: {pontos}
                    """;
            }

            var cursosAceitos = string.Join(", ", vaga.CursosAceitos ?? new());
            var habilidadesReq = string.Join(", ", vaga.HabilidadesRequeridas ?? new());
            var habilidadesDif = string.Join(", ", vaga.HabilidadesDiferenciais ?? new());
            var contextoVaga = $"""
                Vaga:
                - Título: {vaga.Titulo}
                - Área: {vaga.Area}
                - Cursos aceitos: {cursosAceitos}
                - Semestre mínimo: {vaga.SemestreMinimo}
                - Habilidades requeridas: {habilidadesReq}
                - Habilidades diferenciais: {habilidadesDif}
                - Modalidade: {vaga.Modalidade}
                - Cidade: {vaga.Cidade} / {vaga.Estado}
                """;

            // Chama a IA
            const string system =
                "Você é um especialista em recrutamento e seleção. " +
                "Analise a compatibilidade entre o estudante e a vaga. " +
                "Responda APENAS com JSON válido, sem texto adicional, sem markdown, sem ```json.";

            var user = $$"""
                Analise a compatibilidade e retorne um JSON com:
                {
                  "scoreCurriculo": 0-100 (quanto o currículo/experiência se alinha com a vaga),
                  "scoreVocacional": 0-100 (quanto o perfil vocacional se alinha com a área da vaga),
                  "scoreHabilidades": 0-100 (quanto as habilidades do estudante cobrem as requeridas pela vaga),
                  "pontosFortes": ["ponto1", "ponto2"],
                  "pontosFracos": ["ponto1", "ponto2"],
                  "justificativa": "explicação resumida em 2-3 frases"
                }

                {{contextoEstudante}}
                {{contextoTeste}}
                {{contextoVaga}}
                """;

            ScoreCompatibilidadeDto dados;
            try
            {
                dados = await _groq.ChatJsonAsync<ScoreCompatibilidadeDto>(system, user);
            }
            catch
            {
                dados = GerarFallbackScore(estudante, vaga, teste);
            }

            // Garante que o scoreTotal seja sempre calculado com pesos fixos
            // a partir dos sub-scores (a IA pode retornar um total inconsistente)
            // Pesos: Currículo 25% | Vocacional 40% | Habilidades 35%
            dados.ScoreTotal = Math.Round(
                Math.Clamp(
                    (dados.ScoreCurriculo  * 0.25m) +
                    (dados.ScoreVocacional * 0.40m) +
                    (dados.ScoreHabilidades * 0.35m),
                    0m, 100m),
                1);

            try
            {
                var matchExistente = await _dbContext.Matches
                    .FirstOrDefaultAsync(m => m.EstudanteId == estudanteId && m.VagaId == vagaId);

                if (matchExistente != null)
                {
                    matchExistente.ScoreTotal = dados.ScoreTotal;
                    matchExistente.ScoreCurriculo = dados.ScoreCurriculo;
                    matchExistente.ScoreVocacional = dados.ScoreVocacional;
                    matchExistente.ScoreHabilidades = dados.ScoreHabilidades;
                    matchExistente.PontosFortes = dados.PontosFortes;
                    matchExistente.PontosFracos = dados.PontosFracos;
                    matchExistente.Justificativa = dados.Justificativa;
                    _dbContext.Matches.Update(matchExistente);
                }
                else
                {
                    await _dbContext.Matches.AddAsync(new Match
                    {
                        EstudanteId = estudanteId,
                        VagaId = vagaId,
                        EmpresaId = vaga.EmpresaId,
                        ScoreTotal = dados.ScoreTotal,
                        ScoreCurriculo = dados.ScoreCurriculo,
                        ScoreVocacional = dados.ScoreVocacional,
                        ScoreHabilidades = dados.ScoreHabilidades,
                        PontosFortes = dados.PontosFortes,
                        PontosFracos = dados.PontosFracos,
                        Justificativa = dados.Justificativa,
                    });
                }

                await _dbContext.SaveChangesAsync();
                await AtualizarRankingAsync(vagaId);
            }
            catch
            {
                // Se a persistencia falhar, ainda retornamos o score para o frontend.
            }

            return dados;
        }

        private static ScoreCompatibilidadeDto GerarFallbackScore(Estudante estudante, Vaga vaga, TesteVocacional? teste)
        {
            var habilidadesAluno = (estudante.HabilidadesExtraidas ?? new List<string>())
                .Select(h => h.Trim().ToLowerInvariant())
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .ToHashSet();

            var habilidadesVaga = ((vaga.HabilidadesRequeridas ?? new List<string>())
                .Concat(vaga.HabilidadesDiferenciais ?? new List<string>()))
                .Select(h => h.Trim().ToLowerInvariant())
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .ToHashSet();

            var intersec = habilidadesVaga.Count == 0 ? 0 : habilidadesVaga.Count(habilidadesAluno.Contains);
            var scoreHabilidades = habilidadesVaga.Count == 0
                ? (habilidadesAluno.Count > 0 ? 55m : 40m)
                : Math.Clamp((decimal)intersec / habilidadesVaga.Count * 100m, 0m, 100m);

            var scoreCurriculo = (estudante.TemCurriculo == true) || !string.IsNullOrWhiteSpace(estudante.CurriculoUrl) ? 75m : 42m;

            decimal scoreVocacional = 55m;
            if (teste?.AreasSugeridas != null && teste.AreasSugeridas.Count > 0)
            {
                var areaVaga = (vaga.Area ?? string.Empty).Trim().ToLowerInvariant();
                var matchArea = teste.AreasSugeridas.Any(a => !string.IsNullOrWhiteSpace(a) && areaVaga.Contains(a.Trim().ToLowerInvariant()));
                scoreVocacional = matchArea ? 82m : 52m;
            }

            var scoreTotal = Math.Clamp((scoreCurriculo * 0.25m) + (scoreHabilidades * 0.35m) + (scoreVocacional * 0.40m), 0m, 100m);

            return new ScoreCompatibilidadeDto
            {
                ScoreTotal = Math.Round(scoreTotal, 2),
                ScoreCurriculo = Math.Round(scoreCurriculo, 2),
                ScoreHabilidades = Math.Round(scoreHabilidades, 2),
                ScoreVocacional = Math.Round(scoreVocacional, 2),
                PontosFortes = new List<string>
                {
                    "Compatibilidade calculada por habilidades e perfil",
                    "Resultado gerado em modo de contingencia"
                },
                PontosFracos = new List<string>
                {
                    "Analise sem IA pode ser menos detalhada"
                },
                Justificativa = "Score calculado por regras de compatibilidade entre habilidades, curriculo e area vocacional."
            };
        }

        private async Task AtualizarRankingAsync(int vagaId)
        {
            var matches = await _dbContext.Matches
                .Where(m => m.VagaId == vagaId)
                .OrderByDescending(m => m.ScoreTotal)
                .ToListAsync();

            int posicao = 1;
            foreach (var match in matches)
            {
                var candidaturas = await _dbContext.Candidaturas
                    .Where(c => c.EstudanteId == match.EstudanteId && c.VagaId == vagaId)
                    .ToListAsync();

                foreach (var candidatura in candidaturas)
                {
                    candidatura.PosicaoRanking = posicao;
                    _dbContext.Candidaturas.Update(candidatura);
                }
                posicao++;
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
