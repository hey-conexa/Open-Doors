using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Infrastructure.Database;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [ApiController]
    [Route("api/perguntas-teste")]
    public class PerguntasTesteController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public PerguntasTesteController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET /api/perguntas-teste
        // Retorna perguntas ativas em ordem.
        // Parâmetros opcionais:
        //   ?mes=2025-06        → só perguntas mensais daquele mês
        //   ?tipo=fixa          → só perguntas fixas (RIASEC / Big Five)
        //   ?tipo=mensal        → só perguntas mensais
        //   (sem parâmetros)    → todas as ativas (fixas + mês atual)
        [HttpGet]
        public async Task<IActionResult> ListarAtivas([FromQuery] string? mes, [FromQuery] string? tipo)
        {
            try
            {
                var perguntas = await _dbContext.PerguntasTeste
                    .Where(p => p.Ativa)
                    .ToListAsync();

                if (!string.IsNullOrEmpty(tipo))
                    perguntas = perguntas.Where(p => p.Tipo == tipo).ToList();

                if (!string.IsNullOrEmpty(mes))
                    perguntas = perguntas.Where(p => p.MesReferencia == mes).ToList();
                else if (string.IsNullOrEmpty(tipo))
                    // Sem filtros: retorna fixas + perguntas do mês atual
                    perguntas = perguntas.Where(p =>
                        p.Tipo == "fixa" ||
                        p.MesReferencia == DateTime.UtcNow.ToString("yyyy-MM")).ToList();

                var dto = perguntas
                    .OrderBy(p => p.Ordem)
                    .Select(MapearParaDto)
                    .ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/perguntas-teste/todas
        // Todas as perguntas (admin)
        [HttpGet("todas")]
        public async Task<IActionResult> ListarTodas()
        {
            try
            {
                var resultado = await _dbContext.PerguntasTeste
                    .OrderBy(p => p.Ordem)
                    .ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // POST /api/perguntas-teste
        // Cria nova pergunta manualmente
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CreatePerguntaTesteDto body)
        {
            if (string.IsNullOrWhiteSpace(body.Pergunta))
                return BadRequest(new { erro = "O campo 'pergunta' é obrigatório" });

            try
            {
                var nova = new PerguntaTeste
                {
                    Pergunta      = body.Pergunta,
                    Ordem         = body.Ordem,
                    Ativa         = true,
                    Categoria     = body.Categoria ?? "geral",
                    MesReferencia = body.MesReferencia,
                    Tipo          = string.IsNullOrEmpty(body.MesReferencia) ? "fixa" : "mensal",
                    CriadoEm     = DateTime.UtcNow
                };

                await _dbContext.PerguntasTeste.AddAsync(nova);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(ListarAtivas), MapearParaDto(nova));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // PUT /api/perguntas-teste/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] CreatePerguntaTesteDto body)
        {
            if (string.IsNullOrWhiteSpace(body.Pergunta))
                return BadRequest(new { erro = "O campo 'pergunta' é obrigatório" });

            try
            {
                var resultado = await _dbContext.PerguntasTeste
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (resultado == null)
                    return NotFound(new { mensagem = "Pergunta não encontrada" });

                resultado.Pergunta      = body.Pergunta;
                resultado.Ordem         = body.Ordem;
                resultado.Categoria     = body.Categoria ?? resultado.Categoria;
                resultado.MesReferencia = body.MesReferencia;

                _dbContext.PerguntasTeste.Update(resultado);
                await _dbContext.SaveChangesAsync();
                return Ok(MapearParaDto(resultado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // DELETE /api/perguntas-teste/{id}
        // Soft delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> Desativar(int id)
        {
            try
            {
                var resultado = await _dbContext.PerguntasTeste
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (resultado == null)
                    return NotFound(new { mensagem = "Pergunta não encontrada" });

                resultado.Ativa = false;
                _dbContext.PerguntasTeste.Update(resultado);
                await _dbContext.SaveChangesAsync();

                return Ok(new { mensagem = "Pergunta desativada com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        private static PerguntaTesteDto MapearParaDto(PerguntaTeste p) => new()
        {
            Id            = p.Id,
            Pergunta      = p.Pergunta,
            Ordem         = p.Ordem,
            Categoria     = p.Categoria,
            MesReferencia = p.MesReferencia,
            Tipo          = p.Tipo
        };
    }
}
