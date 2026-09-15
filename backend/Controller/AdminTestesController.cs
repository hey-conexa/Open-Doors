using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Api.Models;
using OpenDoors.Api.Services;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/admin/testes")]
    public class AdminTestesController : ControllerBase
    {
        private readonly GerarPerguntasMensaisService _gerador;
        private readonly Supabase.Client _supabase;

        public AdminTestesController(GerarPerguntasMensaisService gerador, Supabase.Client supabase)
        {
            _gerador  = gerador;
            _supabase = supabase;
        }

        // POST /api/admin/testes/gerar-mes
        // Gera as perguntas do mês atual via Groq e salva no banco.
        // Idempotente: se já existirem perguntas pro mês, retorna as existentes sem duplicar.
        [HttpPost("gerar-mes")]
        public async Task<IActionResult> GerarMesAtual()
        {
            try
            {
                var perguntas = await _gerador.GerarParaMesAtualAsync();
                return Ok(new
                {
                    mes       = DateTime.UtcNow.ToString("yyyy-MM"),
                    geradas   = perguntas.Count,
                    perguntas = perguntas.Select(p => new { p.Id, p.Pergunta, p.Categoria, p.Ordem })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // POST /api/admin/testes/gerar-mes/{mesReferencia}
        // Gera perguntas para um mês específico (ex: "2025-08").
        // Útil para pré-gerar perguntas com antecedência.
        [HttpPost("gerar-mes/{mesReferencia}")]
        public async Task<IActionResult> GerarMesEspecifico(string mesReferencia)
        {
            // Valida formato yyyy-MM
            if (!System.Text.RegularExpressions.Regex.IsMatch(mesReferencia, @"^\d{4}-\d{2}$"))
                return BadRequest(new { erro = "Formato inválido. Use yyyy-MM, ex: 2025-08" });

            try
            {
                var perguntas = await _gerador.GerarParaMesAsync(mesReferencia);
                return Ok(new
                {
                    mes       = mesReferencia,
                    geradas   = perguntas.Count,
                    perguntas = perguntas.Select(p => new { p.Id, p.Pergunta, p.Categoria, p.Ordem })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/admin/testes/por-mes
        // Lista quantas perguntas existem por mês — visão geral do banco
        [HttpGet("por-mes")]
        public async Task<IActionResult> ListarPorMes()
        {
            try
            {
                var todas = await _supabase.From<PerguntaTeste>().Get();

                var agrupado = todas.Models
                    .GroupBy(p => p.MesReferencia ?? "fixa")
                    .Select(g => new
                    {
                        mes       = g.Key,
                        tipo      = g.Key == "fixa" ? "fixa" : "mensal",
                        total     = g.Count(),
                        ativas    = g.Count(p => p.Ativa),
                        categorias = g.Select(p => p.Categoria).Distinct().ToList()
                    })
                    .OrderBy(g => g.mes)
                    .ToList();

                return Ok(agrupado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }
    }
}
