using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificacoesController : ControllerBase
    {
        private readonly Supabase.Client _supabase;

        public NotificacoesController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        // GET /api/notificacoes
        [HttpGet]
        public async Task<IActionResult> ListarTodas()
        {
            try
            {
                var resultado = await _supabase.From<Notificacao>().Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/notificacoes/estudante/{id}
        [HttpGet("estudante/{estudanteId}")]
        public async Task<IActionResult> ListarPorEstudante(Guid estudanteId)
        {
            try
            {
                var resultado = await _supabase
                    .From<Notificacao>()
                    .Where(n => n.EstudanteId == estudanteId)
                    .Order(n => n.CriadoEm!, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/notificacoes/empresa/{id}
        [HttpGet("empresa/{empresaId}")]
        public async Task<IActionResult> ListarPorEmpresa(Guid empresaId)
        {
            try
            {
                var resultado = await _supabase
                    .From<Notificacao>()
                    .Where(n => n.EmpresaId == empresaId)
                    .Order(n => n.CriadoEm!, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/notificacoes/nao-lidas/estudante/{id}
        // Útil pro badge "você tem X notificações novas"
        [HttpGet("nao-lidas/estudante/{estudanteId}")]
        public async Task<IActionResult> NaoLidasEstudante(Guid estudanteId)
        {
            try
            {
                var resultado = await _supabase
                    .From<Notificacao>()
                    .Where(n => n.EstudanteId == estudanteId && n.Lida == false)
                    .Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
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