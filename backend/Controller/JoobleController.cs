using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Infrastructure.Database;
using OpenDoors.Api.Models;
using OpenDoors.Api.Services;

namespace OpenDoors.Api.Controllers
{
    [ApiController]
    [Route("api/jooble")]
    public class JoobleController : ControllerBase
    {
        private readonly JoobleService _jooble;
        private readonly AppDbContext _dbContext;

        public JoobleController(JoobleService jooble, AppDbContext dbContext)
        {
            _jooble     = jooble;
            _dbContext  = dbContext;
        }

        // GET /api/jooble/buscar?keywords=Engenharia&location=São Paulo&page=1
        // Busca vagas na Jooble e retorna direto pro frontend — SEM salvar no banco.
        // Útil para exibir vagas externas ao vivo na home do estudante.
        [AllowAnonymous]
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
        [AllowAnonymous]
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

                    var existe = await _dbContext.Vagas
                        .AnyAsync(v => v.Titulo == vaga.Titulo && v.Cidade == vaga.Cidade);

                    if (existe)
                    {
                        ignoradas++;
                        continue;
                    }

                    await _dbContext.Vagas.AddAsync(vaga);
                    salvas++;
                }

                await _dbContext.SaveChangesAsync();

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
