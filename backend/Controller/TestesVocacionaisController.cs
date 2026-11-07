using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Infrastructure.Database;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/testes-vocacionais")]
    public class TestesVocacionaisController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public TestesVocacionaisController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET /api/testes-vocacionais
        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            try
            {
                var resultado = await _dbContext.TestesVocacionais.ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/testes-vocacionais/estudante/{id}
        // Busca o teste de um estudante (só tem 1 por estudante)
        [HttpGet("estudante/{estudanteId}")]
        public async Task<IActionResult> BuscarPorEstudante(Guid estudanteId)
        {
            try
            {
                var resultado = await _dbContext.TestesVocacionais
                    .FirstOrDefaultAsync(t => t.EstudanteId == estudanteId);

                if (resultado == null)
                    return NotFound(new { mensagem = "Estudante ainda não fez o teste vocacional" });

                return Ok(MapearParaDto(resultado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/testes-vocacionais/analisados
        // Testes que já passaram pela análise da IA
        [HttpGet("analisados")]
        public async Task<IActionResult> ListarAnalisados()
        {
            try
            {
                var resultado = await _dbContext.TestesVocacionais
                    .Where(t => t.AnalisadoIa == true)
                    .ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        private static TesteVocacionalDto MapearParaDto(TesteVocacional t)
        {
            return new TesteVocacionalDto
            {
                Id = t.Id,
                EstudanteId = t.EstudanteId,
                PerfilDominante = t.PerfilDominante,
                AreasSugeridas = t.AreasSugeridas,
                PontosFortes = t.PontosFortes,
                DescricaoPerfil = t.DescricaoPerfil,
                AnalisadoIa = t.AnalisadoIa,
                ConcluidoEm = t.ConcluidoEm
            };
        }
    }
}