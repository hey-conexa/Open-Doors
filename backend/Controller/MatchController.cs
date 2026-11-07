using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Infrastructure.Database;
using OpenDoors.Api.Middleware;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public MatchesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ===========================================
        // GET /api/matches — Lista TODOS os matches
        // Apenas estudantes podem acessar
        // ===========================================
        [AuthorizeRole(TiposUsuario.Estudante)]
        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            try
            {
                var resultado = await _dbContext.Matches.ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // GET /api/matches/{id} — Busca um match específico
        // Apenas estudantes podem acessar
        // ===========================================
        [AuthorizeRole(TiposUsuario.Estudante)]
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            try
            {
                var resultado = await _dbContext.Matches
                    .FirstOrDefaultAsync(m => m.Id == id);

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
        // Apenas estudantes podem acessar
        // ===========================================
        [AuthorizeRole(TiposUsuario.Estudante)]
        [HttpGet("estudante/{estudanteId}")]
        public async Task<IActionResult> ListarPorEstudante(Guid estudanteId)
        {
            try
            {
                var resultado = await _dbContext.Matches
                    .Where(m => m.EstudanteId == estudanteId)
                    .OrderByDescending(m => m.ScoreTotal)
                    .ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
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
                var resultado = await _dbContext.Matches
                    .Where(m => m.VagaId == vagaId)
                    .OrderByDescending(m => m.ScoreTotal)
                    .ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
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
        // Apenas estudantes podem acessar
        // ===========================================
        [AuthorizeRole(TiposUsuario.Estudante)]
        [HttpGet("top/{estudanteId}")]
        public async Task<IActionResult> TopMatchesEstudante(Guid estudanteId)
        {
            try
            {
                var resultado = await _dbContext.Matches
                    .Where(m => m.EstudanteId == estudanteId)
                    .OrderByDescending(m => m.ScoreTotal)
                    .Take(5)
                    .ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
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
        // Apenas estudantes podem acessar
        // ===========================================
        [AuthorizeRole(TiposUsuario.Estudante)]
        [HttpGet("excelentes")]
        public async Task<IActionResult> ListarExcelentes()
        {
            try
            {
                var resultado = await _dbContext.Matches
                    .Where(m => m.ScoreTotal >= 80)
                    .OrderByDescending(m => m.ScoreTotal)
                    .ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // POST /api/matches
        // Endpoint usado pela IA para gravar resultados de análise
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

                await _dbContext.Matches.AddAsync(novo);
                await _dbContext.SaveChangesAsync();

                var criado = MapearParaDto(novo);
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
                var existente = await _dbContext.Matches
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (existente == null)
                    return NotFound(new { mensagem = "Match não encontrado" });

                _dbContext.Matches.Remove(existente);
                await _dbContext.SaveChangesAsync();

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