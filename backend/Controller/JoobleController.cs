using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Models;
using OpenDoors.Api.Services;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/jooble")]
    public class JoobleController : ControllerBase
    {
        private readonly JoobleService _jooble;
        private readonly Supabase.Client _supabase;

        public JoobleController(JoobleService jooble, Supabase.Client supabase)
        {
            _jooble   = jooble;
            _supabase = supabase;
        }

        // GET /api/jooble/buscar?keywords=Engenharia&location=São Paulo&page=1
        // Busca vagas na Jooble e retorna direto pro frontend — SEM salvar no banco.
        // Útil para exibir vagas externas ao vivo na home do estudante.
        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] JoobleBuscaDto filtros)
        {
            try
            {
                var resultado = await _jooble.BuscarVagasAsync(filtros);
                return Ok(new
                {
                    total  = resultado.TotalCount,
                    vagas  = resultado.Jobs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // POST /api/jooble/sincronizar
        // Busca vagas na Jooble e salva no banco (tabela "vagas").
        // Evita duplicatas pelo título + cidade.
        // Ideal para rodar uma vez ao dia via tarefa agendada (ou manualmente pelo admin).
        //
        // Body esperado:
        // {
        //   "keywords": "Engenharia de Software",
        //   "location": "Brasil",
        //   "page": 1,
        //   "resultsPerPage": 20
        // }
        [HttpPost("sincronizar")]
        public async Task<IActionResult> Sincronizar([FromBody] JoobleBuscaDto filtros)
        {
            try
            {
                var resultado = await _jooble.BuscarVagasAsync(filtros);

                if (resultado.Jobs == null || resultado.Jobs.Count == 0)
                    return Ok(new { mensagem = "Nenhuma vaga retornada pela Jooble.", salvas = 0 });

                int salvas    = 0;
                int ignoradas = 0;

                foreach (var job in resultado.Jobs)
                {
                    // Verifica se já existe uma vaga com o mesmo título e cidade
                    var vaga = JoobleService.ConverterParaVaga(job);

                    var existentes = await _supabase
                        .From<Vaga>()
                        .Where(v => v.Titulo == vaga.Titulo && v.Cidade == vaga.Cidade)
                        .Get();

                    if (existentes.Models.Any())
                    {
                        ignoradas++;
                        continue;
                    }

                    await _supabase.From<Vaga>().Insert(vaga);
                    salvas++;
                }

                return Ok(new
                {
                    mensagem  = "Sincronização concluída.",
                    salvas,
                    ignoradas,
                    total     = resultado.TotalCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }
    }
}
