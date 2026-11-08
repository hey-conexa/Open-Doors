using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Infrastructure.Database;
using OpenDoors.Api.Middleware;
using OpenDoors.Api.Models;
using OpenDoors.Api.Services;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/ia/agente-carreira")]
    public class AgenteCarreiraController : ControllerBase
    {
        private readonly AgenteCarreiraService _agente;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AgenteCarreiraController> _logger;

        public AgenteCarreiraController(
            AgenteCarreiraService agente,
            AppDbContext dbContext,
            ILogger<AgenteCarreiraController> logger)
        {
            _agente = agente;
            _dbContext = dbContext;
            _logger = logger;
        }

        // ===========================================
        // POST /api/ia/agente-carreira
        // Sem body: o estudante é identificado pelo token (claim "conta_id"),
        // então ninguém consegue pedir o relatório de outra pessoa.
        // ===========================================
        [AuthorizeRole(TiposUsuario.Estudante)]
        [HttpPost]
        public async Task<IActionResult> Gerar()
        {
            if (!Guid.TryParse(User.FindFirst("conta_id")?.Value, out var estudanteId))
                return Unauthorized();

            var estudante = await _dbContext.Estudantes
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == estudanteId);

            if (estudante == null)
                return NotFound(new { erro = "Estudante não encontrado." });

            // Seleciona só o texto: não carrega o PDF (bytea) inteiro do banco
            var textoCurriculo = await _dbContext.Curriculos
                .AsNoTracking()
                .Where(c => c.EstudanteId == estudanteId)
                .Select(c => c.Texto)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(textoCurriculo))
                return UnprocessableEntity(new { erro = "Envie seu currículo para receber sugestões." });

            var teste = await _dbContext.TestesVocacionais
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.EstudanteId == estudanteId);

            var perfil = new PerfilParaMelhoriaDto
            {
                Curso = estudante.Curso,
                Semestre = estudante.Semestre,
                Instituicao = estudante.Instituicao,
                PrevisaoConclusao = estudante.PrevisaoConclusao,
                Habilidades = estudante.HabilidadesExtraidas ?? new List<string>(),
                PerfilVocacional = teste?.PerfilDominante,
                AreasSugeridas = teste?.AreasSugeridas ?? new List<string>(),
                TextoCurriculo = textoCurriculo
            };

            try
            {
                var resultado = await _agente.GerarAsync(perfil);
                return Ok(resultado);
            }
            catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException or TaskCanceledException)
            {
                // Groq recusou, ficou fora do ar ou devolveu um JSON inválido.
                // O detalhe técnico vai pro log; o usuário recebe só uma mensagem amigável.
                _logger.LogError(ex, "Falha ao gerar sugestões do agente de carreira para o estudante {EstudanteId}", estudanteId);
                return StatusCode(502, new { erro = "A IA não conseguiu gerar as sugestões agora. Tente novamente em instantes." });
            }
        }
    }
}