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
    public class NotificacoesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public NotificacoesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET /api/notificacoes
        // Ambos os tipos podem acessar
        [HttpGet]
        public async Task<IActionResult> ListarTodas()
        {
            try
            {
                var resultado = await _dbContext.Notificacoes.ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/notificacoes/estudante/{id}
        // Apenas estudantes podem acessar
        [AuthorizeRole(TiposUsuario.Estudante)]
        [HttpGet("estudante/{estudanteId}")]
        public async Task<IActionResult> ListarPorEstudante(Guid estudanteId)
        {
            try
            {
                var resultado = await _dbContext.Notificacoes
                    .Where(n => n.EstudanteId == estudanteId)
                    .OrderByDescending(n => n.CriadoEm)
                    .ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/notificacoes/empresa/{id}
        // Apenas empresas podem acessar
        [AuthorizeRole(TiposUsuario.Empresa)]
        [HttpGet("empresa/{empresaId}")]
        public async Task<IActionResult> ListarPorEmpresa(Guid empresaId)
        {
            try
            {
                var resultado = await _dbContext.Notificacoes
                    .Where(n => n.EmpresaId == empresaId)
                    .OrderByDescending(n => n.CriadoEm)
                    .ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/notificacoes/nao-lidas/estudante/{id}
        // Útil pro badge "você tem X notificações novas"
        // Apenas estudantes podem acessar
        [AuthorizeRole(TiposUsuario.Estudante)]
        [HttpGet("nao-lidas/estudante/{estudanteId}")]
        public async Task<IActionResult> NaoLidasEstudante(Guid estudanteId)
        {
            try
            {
                var resultado = await _dbContext.Notificacoes
                    .Where(n => n.EstudanteId == estudanteId && n.Lida == false)
                    .ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        private static NotificacaoDto MapearParaDto(Notificacao n)
        {
            return new NotificacaoDto
            {
                Id = n.Id,
                DestinatarioTipo = n.DestinatarioTipo,
                EstudanteId = n.EstudanteId,
                EmpresaId = n.EmpresaId,
                Tipo = n.Tipo,
                Titulo = n.Titulo,
                Mensagem = n.Mensagem,
                ReferenciaTabela = n.ReferenciaTabela,
                ReferenciaId = n.ReferenciaId,
                Lida = n.Lida,
                DataLeitura = n.DataLeitura,
                CriadoEm = n.CriadoEm
            };
        }
    }
}