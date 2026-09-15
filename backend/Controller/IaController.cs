using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Models;
using OpenDoors.Api.Services;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/ia")]
    public class IaController : ControllerBase
    {
        private readonly AnalisarCurriculoService _curriculoService;
        private readonly AnalisarTesteService _testeService;
        private readonly GerarScoreService _scoreService;
        private readonly Supabase.Client _supabase;

        public IaController(
            AnalisarCurriculoService curriculoService,
            AnalisarTesteService testeService,
            GerarScoreService scoreService,
            Supabase.Client supabase)
        {
            _curriculoService = curriculoService;
            _testeService = testeService;
            _scoreService = scoreService;
            _supabase = supabase;
        }

        // ===========================================
        // POST /api/ia/analisar-curriculo
        // Recebe PDF via form-data, extrai habilidades
        // ===========================================
        [HttpPost("analisar-curriculo")]
        public async Task<IActionResult> AnalisarCurriculo(
            [FromForm] Guid estudanteId,
            IFormFile curriculo)
        {
            if (estudanteId == Guid.Empty || curriculo == null)
                return BadRequest(new { erro = "estudanteId e curriculo são obrigatórios" });

            try
            {
                await using var stream = curriculo.OpenReadStream();
                var resultado = await _curriculoService.AnalisarAsync(estudanteId, stream, curriculo.FileName ?? "curriculo.pdf");
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = "Erro interno ao analisar currículo.", detalhe = ex.Message });
            }
        }

        // ===========================================
        // POST /api/ia/analisar-teste
        // Recebe respostas do teste vocacional, gera perfil
        // ===========================================
        [HttpPost("analisar-teste")]
        public async Task<IActionResult> AnalisarTeste([FromBody] AnalisarTesteRequestDto body)
        {
            if (body.EstudanteId == Guid.Empty || body.Respostas == null || body.Respostas.Count == 0)
                return BadRequest(new { erro = "estudanteId e respostas são obrigatórios" });

            PerfilVocacionalDto resultado;
            try
            {
                resultado = await _testeService.AnalisarAsync(body.Respostas);
            }
            catch (ArgumentException ex)
            {
                return UnprocessableEntity(new { erro = ex.Message });
            }
            catch (Exception)
            {
                resultado = GerarPerfilFallback(body.Respostas);
            }

            // Salva ou atualiza o teste vocacional no banco
            var testesExistentes = await _supabase
                .From<TesteVocacional>()
                .Where(t => t.EstudanteId == body.EstudanteId)
                .Get();

            var testeExistente = testesExistentes.Models.FirstOrDefault();

            if (testeExistente != null)
            {
                testeExistente.PerfilDominante = resultado.PerfilDominante;
                testeExistente.AreasSugeridas = resultado.AreasSugeridas;
                testeExistente.PontosFortes = resultado.PontosFortes;
                testeExistente.DescricaoPerfil = resultado.DescricaoPerfil;
                testeExistente.AnalisadoIa = true;
                await testeExistente.Update<TesteVocacional>();
            }
            else
            {
                await _supabase.From<TesteVocacional>().Insert(new TesteVocacional
                {
                    EstudanteId = body.EstudanteId,
                    PerfilDominante = resultado.PerfilDominante,
                    AreasSugeridas = resultado.AreasSugeridas,
                    PontosFortes = resultado.PontosFortes,
                    DescricaoPerfil = resultado.DescricaoPerfil,
                    AnalisadoIa = true
                });
            }

            // Marca o estudante como tendo teste vocacional
            var estudante = await _supabase
                .From<Estudante>()
                .Where(e => e.Id == body.EstudanteId)
                .Single();

            if (estudante != null)
            {
                estudante.TemTesteVocacional = true;
                await estudante.Update<Estudante>();
            }

            return Ok(resultado);
        }

        private static PerfilVocacionalDto GerarPerfilFallback(List<RespostaVocacionalDto> respostas)
        {
            var pesos = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["Realista"] = 0,
                ["Investigativo"] = 0,
                ["Artistico"] = 0,
                ["Social"] = 0,
                ["Empreendedor"] = 0,
                ["Convencional"] = 0,
            };

            foreach (var r in respostas)
            {
                if (!int.TryParse(r.Resposta, out var valorLikert)) continue;
                var afinidade = Math.Clamp(8 - valorLikert, 1, 7);
                var categoria = (r.Categoria ?? "").ToLowerInvariant();

                if (categoria.Contains("realista")) pesos["Realista"] += afinidade;
                else if (categoria.Contains("investigativo")) pesos["Investigativo"] += afinidade;
                else if (categoria.Contains("artistico") || categoria.Contains("artístico")) pesos["Artistico"] += afinidade;
                else if (categoria.Contains("social")) pesos["Social"] += afinidade;
                else if (categoria.Contains("empreendedor")) pesos["Empreendedor"] += afinidade;
                else if (categoria.Contains("convencional")) pesos["Convencional"] += afinidade;
            }

            var ordenado = pesos.OrderByDescending(p => p.Value).Select(p => p.Key).ToList();
            var topo = ordenado.Take(3).ToList();
            var dominante = topo.FirstOrDefault() ?? "Versatil";

            var mapaAreas = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["Realista"] = ["Logistica", "Operacoes", "Manutencao"],
                ["Investigativo"] = ["Pesquisa", "Analise de Dados", "Qualidade"],
                ["Artistico"] = ["Design", "Comunicacao", "Conteudo"],
                ["Social"] = ["Educacao", "Recursos Humanos", "Atendimento"],
                ["Empreendedor"] = ["Gestao Comercial", "Lideranca", "Negocios"],
                ["Convencional"] = ["Administrativo", "Financeiro", "Processos"],
            };

            var areas = topo
                .SelectMany(t => mapaAreas.TryGetValue(t, out var arr) ? arr : ["Areas diversas"])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(3)
                .ToList();

            var pontos = new List<string>
            {
                "Capacidade de adaptacao",
                "Potencial para atuar em diferentes contextos",
                "Boa base para explorar trilhas profissionais"
            };

            return new PerfilVocacionalDto
            {
                PerfilDominante = dominante,
                AreasSugeridas = areas,
                PontosFortes = pontos,
                DescricaoPerfil = "Analise gerada em modo de contingencia para manter o teste funcional. O perfil indica interesses predominantes e sugere exploracao de areas compativeis."
            };
        }

        // ===========================================
        // POST /api/ia/gerar-score
        // Calcula compatibilidade Estudante x Vaga
        // ===========================================
        [HttpPost("gerar-score")]
        public async Task<IActionResult> GerarScore([FromBody] GerarScoreRequestDto body)
        {
            if (body.EstudanteId == Guid.Empty || body.VagaId == 0)
                return BadRequest(new { erro = "estudanteId e vagaId são obrigatórios" });

            try
            {
                var resultado = await _scoreService.GerarAsync(body.EstudanteId, body.VagaId);
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = "Erro interno ao gerar score.", detalhe = ex.Message });
            }
        }
    }
}
