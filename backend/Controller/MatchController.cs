using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly Supabase.Client _supabase;

        public MatchesController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        // ===========================================
        // GET /api/matches — Lista TODOS os matches
        // ===========================================
        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            try
            {
                var resultado = await _supabase.From<Match>().Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // GET /api/matches/{id} — Busca um match específico
        // ===========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            try
            {
                var resultado = await _supabase
                    .From<Match>()
                    .Where(m => m.Id == id)
                    .Single();

                if (resultado == null)
                    return NotFound(new { mensagem = "Match não encontrado" });

                return Ok(MapearParaDto(resultado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // GET /api/matches/estudante/{estudanteId}
        // Matches que o estudante recebeu (ordenado por score do maior pro menor)
        // É a "lista de recomendações" pro estudante
        // ===========================================
        [HttpGet("estudante/{estudanteId}")]
        public async Task<IActionResult> ListarPorEstudante(Guid estudanteId)
        {
            try
            {
                var resultado = await _supabase
                    .From<Match>()
                    .Where(m => m.EstudanteId == estudanteId)
                    .Order(m => m.ScoreTotal, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // GET /api/matches/vaga/{vagaId}
        // Matches gerados pra uma vaga (a empresa vê os melhores candidatos)
        // ===========================================
        [HttpGet("vaga/{vagaId}")]
        public async Task<IActionResult> ListarPorVaga(int vagaId)
        {
            try
            {
                var resultado = await _supabase
                    .From<Match>()
                    .Where(m => m.VagaId == vagaId)
                    .Order(m => m.ScoreTotal, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // GET /api/matches/top/{estudanteId}
        // Top 5 melhores matches pro estudante (página inicial dele!)
        // ===========================================
        [HttpGet("top/{estudanteId}")]
        public async Task<IActionResult> TopMatchesEstudante(Guid estudanteId)
        {
            try
            {
                var resultado = await _supabase
                    .From<Match>()
                    .Where(m => m.EstudanteId == estudanteId)
                    .Order(m => m.ScoreTotal, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Limit(5)
                    .Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // GET /api/matches/excelentes
        // Matches com score acima de 80 (recomendações fortíssimas)
        // ===========================================
        [HttpGet("excelentes")]
        public async Task<IActionResult> ListarExcelentes()
        {
            try
            {
                var resultado = await _supabase
                    .From<Match>()
                    .Where(m => m.ScoreTotal >= 80)
                    .Order(m => m.ScoreTotal, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // POST /api/matches
        // Endpoint usado pela IA (próxima etapa) pra gravar resultados de análise
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CreateMatchDto dto)
        {
            try
            {
                if (dto.EstudanteId == Guid.Empty || dto.EmpresaId == Guid.Empty || dto.VagaId <= 0)
                    return BadRequest(new { erro = "EstudanteId, VagaId e EmpresaId são obrigatórios" });

                if (dto.ScoreTotal < 0 || dto.ScoreTotal > 100)
                    return BadRequest(new { erro = "ScoreTotal deve estar entre 0 e 100" });

                var novo = new Match
                {
                    EstudanteId = dto.EstudanteId,
                    VagaId = dto.VagaId,
                    EmpresaId = dto.EmpresaId,
                    ScoreTotal = dto.ScoreTotal,
                    ScoreCurriculo = dto.ScoreCurriculo,
                    ScoreVocacional = dto.ScoreVocacional,
                    ScoreHabilidades = dto.ScoreHabilidades,
                    PontosFortes = dto.PontosFortes,
                    PontosFracos = dto.PontosFracos,
                    Justificativa = dto.Justificativa
                };

                var resultado = await _supabase.From<Match>().Insert(novo);

                if (resultado.Models == null || resultado.Models.Count == 0)
                    return StatusCode(500, new { erro = "Falha ao criar match" });

                var criado = MapearParaDto(resultado.Models[0]);
                return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, criado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // DELETE /api/matches/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            try
            {
                var existente = await _supabase
                    .From<Match>()
                    .Where(m => m.Id == id)
                    .Single();

                if (existente == null)
                    return NotFound(new { mensagem = "Match não encontrado" });

                await _supabase
                    .From<Match>()
                    .Where(m => m.Id == id)
                    .Delete();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        private static MatchDto MapearParaDto(Match m)
        {
            return new MatchDto
            {
                Id = m.Id,
                EstudanteId = m.EstudanteId,
                VagaId = m.VagaId,
                EmpresaId = m.EmpresaId,
                ScoreTotal = m.ScoreTotal,
                ScoreCurriculo = m.ScoreCurriculo,
                ScoreVocacional = m.ScoreVocacional,
                ScoreHabilidades = m.ScoreHabilidades,
                PontosFortes = m.PontosFortes,
                PontosFracos = m.PontosFracos,
                Justificativa = m.Justificativa,
                GeradoEm = m.GeradoEm,
                AtualizadoEm = m.AtualizadoEm
            };
        }
    }
}