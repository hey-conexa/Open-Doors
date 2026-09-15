using Microsoft.AspNetCore.Mvc;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [ApiController]
    [Route("api/perguntas-teste")]
    public class PerguntasTesteController : ControllerBase
    {
        private readonly Supabase.Client _supabase;

        public PerguntasTesteController(Supabase.Client supabase)
        {
            _supabase = supabase;
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
                var query = _supabase.From<PerguntaTeste>();
                var resultado = await query.Get();

                var perguntas = resultado.Models
                    .Where(p => p.Ativa)
                    .AsEnumerable();

                if (!string.IsNullOrEmpty(tipo))
                    perguntas = perguntas.Where(p => p.Tipo == tipo);

                if (!string.IsNullOrEmpty(mes))
                    perguntas = perguntas.Where(p => p.MesReferencia == mes);
                else if (string.IsNullOrEmpty(tipo))
                    // Sem filtros: retorna fixas + perguntas do mês atual
                    perguntas = perguntas.Where(p =>
                        p.Tipo == "fixa" ||
                        p.MesReferencia == DateTime.UtcNow.ToString("yyyy-MM"));

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
                var resultado = await _supabase.From<PerguntaTeste>().Get();
                var dto = resultado.Models
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

                var resultado = await _supabase.From<PerguntaTeste>().Insert(nova);
                var criada    = resultado.Models.FirstOrDefault();

                if (criada == null)
                    return StatusCode(500, new { erro = "Erro ao salvar a pergunta" });

                return CreatedAtAction(nameof(ListarAtivas), MapearParaDto(criada));
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
                var resultado = await _supabase
                    .From<PerguntaTeste>()
                    .Where(p => p.Id == id)
                    .Single();

                if (resultado == null)
                    return NotFound(new { mensagem = "Pergunta não encontrada" });

                resultado.Pergunta      = body.Pergunta;
                resultado.Ordem         = body.Ordem;
                resultado.Categoria     = body.Categoria ?? resultado.Categoria;
                resultado.MesReferencia = body.MesReferencia;

                await resultado.Update<PerguntaTeste>();
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
                var resultado = await _supabase
                    .From<PerguntaTeste>()
                    .Where(p => p.Id == id)
                    .Single();

                if (resultado == null)
                    return NotFound(new { mensagem = "Pergunta não encontrada" });

                resultado.Ativa = false;
                await resultado.Update<PerguntaTeste>();

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
