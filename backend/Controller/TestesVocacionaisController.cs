using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/testes-vocacionais")]
    public class TestesVocacionaisController : ControllerBase
    {
        private readonly Supabase.Client _supabase;

        public TestesVocacionaisController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        // GET /api/testes-vocacionais
        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            try
            {
                var resultado = await _supabase.From<TesteVocacional>().Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
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
                var resultado = await _supabase
                    .From<TesteVocacional>()
                    .Where(t => t.EstudanteId == estudanteId)
                    .Single();

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
                var resultado = await _supabase
                    .From<TesteVocacional>()
                    .Where(t => t.AnalisadoIa == true)
                    .Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
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