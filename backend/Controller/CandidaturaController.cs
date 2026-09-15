using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CandidaturasController : ControllerBase
    {
        private readonly Supabase.Client _supabase;

        public CandidaturasController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        // ===========================================
        // GET /api/candidaturas — Lista TODAS
        // ===========================================
        [HttpGet]
        public async Task<IActionResult> ListarTodas()
        {
            try
            {
                var resultado = await _supabase.From<Candidatura>().Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // GET /api/candidaturas/{id} — Busca uma específica
        // ===========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            try
            {
                var resultado = await _supabase
                    .From<Candidatura>()
                    .Where(c => c.Id == id)
                    .Single();

                if (resultado == null)
                    return NotFound(new { mensagem = "Candidatura não encontrada" });

                return Ok(MapearParaDto(resultado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // GET /api/candidaturas/estudante/{estudanteId}
        // Lista todas as candidaturas de UM estudante específico
        // ===========================================
        [HttpGet("estudante/{estudanteId}")]
        public async Task<IActionResult> ListarPorEstudante(Guid estudanteId)
        {
            try
            {
                var resultado = await _supabase
                    .From<Candidatura>()
                    .Where(c => c.EstudanteId == estudanteId)
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
        // GET /api/candidaturas/vaga/{vagaId}
        // Lista todas as candidaturas de UMA vaga (empresa usa pra ver candidatos)
        // ===========================================
        [HttpGet("vaga/{vagaId}")]
        public async Task<IActionResult> ListarPorVaga(int vagaId)
        {
            try
            {
                var resultado = await _supabase
                    .From<Candidatura>()
                    .Where(c => c.VagaId == vagaId)
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
        // GET /api/candidaturas/empresa/{empresaId}
        // Todas as candidaturas que uma empresa recebeu (em todas as suas vagas)
        // ===========================================
        [HttpGet("empresa/{empresaId}")]
        public async Task<IActionResult> ListarPorEmpresa(Guid empresaId)
        {
            try
            {
                var resultado = await _supabase
                    .From<Candidatura>()
                    .Where(c => c.EmpresaId == empresaId)
                    .Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // POST /api/candidaturas
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CreateCandidaturaDto dto)
        {
            try
            {
                if (dto.EstudanteId == Guid.Empty)
                    return BadRequest(new { erro = "EstudanteId é obrigatório" });

                if (dto.VagaId <= 0)
                    return BadRequest(new { erro = "VagaId é obrigatório" });

                if (dto.EmpresaId == Guid.Empty)
                    return BadRequest(new { erro = "EmpresaId é obrigatório" });

                var nova = new Candidatura
                {
                    EstudanteId = dto.EstudanteId,
                    VagaId = dto.VagaId,
                    EmpresaId = dto.EmpresaId,
                    Status = dto.Status ?? "pendente",
                    CartaApresentacao = dto.CartaApresentacao,
                    VisualizadoEmpresa = false
                };

                var resultado = await _supabase.From<Candidatura>().Insert(nova);

                if (resultado.Models == null || resultado.Models.Count == 0)
                    return StatusCode(500, new { erro = "Falha ao criar candidatura" });

                var criada = MapearParaDto(resultado.Models[0]);
                return CreatedAtAction(nameof(BuscarPorId), new { id = criada.Id }, criada);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // PUT /api/candidaturas/{id}/status — atualiza só o status (caso de uso comum)
        [HttpPut("{id}/status")]
        public async Task<IActionResult> AtualizarStatus(int id, [FromBody] string novoStatus)
        {
            try
            {
                var existente = await _supabase
                    .From<Candidatura>()
                    .Where(c => c.Id == id)
                    .Single();

                if (existente == null)
                    return NotFound(new { mensagem = "Candidatura não encontrada" });

                existente.Status = novoStatus;
                var resultado = await existente.Update<Candidatura>();
                return Ok(MapearParaDto(resultado.Models[0]));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // DELETE /api/candidaturas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            try
            {
                var existente = await _supabase
                    .From<Candidatura>()
                    .Where(c => c.Id == id)
                    .Single();

                if (existente == null)
                    return NotFound(new { mensagem = "Candidatura não encontrada" });

                await _supabase
                    .From<Candidatura>()
                    .Where(c => c.Id == id)
                    .Delete();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        private static CandidaturaDto MapearParaDto(Candidatura c)
        {
            return new CandidaturaDto
            {
                Id = c.Id,
                EstudanteId = c.EstudanteId,
                VagaId = c.VagaId,
                EmpresaId = c.EmpresaId,
                Status = c.Status,
                CartaApresentacao = c.CartaApresentacao,
                ScoreCompatibilidade = c.ScoreCompatibilidade,
                PosicaoRanking = c.PosicaoRanking,
                VisualizadoEmpresa = c.VisualizadoEmpresa,
                DataVisualizacao = c.DataVisualizacao,
                CriadoEm = c.CriadoEm,
                AtualizadoEm = c.AtualizadoEm
            };
        }
    }
}