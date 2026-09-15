using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/candidaturas-historico")]
    public class CandidaturasHistoricoController : ControllerBase
    {
        private readonly Supabase.Client _supabase;

        public CandidaturasHistoricoController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        // GET /api/candidaturas-historico
        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            try
            {
                var resultado = await _supabase.From<CandidaturaHistorico>().Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/candidaturas-historico/candidatura/{id}
        // Timeline de uma candidatura específica (pra mostrar evolução)
        [HttpGet("candidatura/{candidaturaId}")]
        public async Task<IActionResult> ListarPorCandidatura(int candidaturaId)
        {
            try
            {
                var resultado = await _supabase
                    .From<CandidaturaHistorico>()
                    .Where(h => h.CandidaturaId == candidaturaId)
                    .Order(h => h.CriadoEm!, Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        private static CandidaturaHistoricoDto MapearParaDto(CandidaturaHistorico h)
        {
            return new CandidaturaHistoricoDto
            {
                Id = h.Id,
                CandidaturaId = h.CandidaturaId,
                Status = h.Status,
                Observacao = h.Observacao,
                CriadoEm = h.CriadoEm
            };
        }
    }
}