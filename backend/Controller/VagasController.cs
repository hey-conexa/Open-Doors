using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Infrastructure.Database;
using OpenDoors.Api.Middleware;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VagasController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public VagasController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ===========================================
        // GET /api/vagas — Lista TODAS as vagas
        // ===========================================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ListarTodas()
        {
            try
            {
                var empresasExemplo = await ObterEmpresasDeExemploAsync();

                var resultado = await _dbContext.Vagas.ToListAsync();

                var vagasDto = OrdenarPorPrioridade(resultado, empresasExemplo)
                    .Select(v => MapearParaDto(v, empresasExemplo))
                    .ToList();
                await PreencherNomesEmpresasAsync(vagasDto);

                return Ok(vagasDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // GET /api/vagas/abertas — Lista só as vagas ABERTAS
        // ===========================================
        [AllowAnonymous]
        [HttpGet("abertas")]
        public async Task<IActionResult> ListarAbertas()
        {
            try
            {
                var empresasExemplo = await ObterEmpresasDeExemploAsync();

                var resultado = await _dbContext.Vagas
                    .Where(v => v.Status == "aberta")
                    .ToListAsync();

                // Vagas de empresas cadastradas pelo site vêm primeiro
                var vagasDto = OrdenarPorPrioridade(resultado, empresasExemplo)
                    .Select(v => MapearParaDto(v, empresasExemplo))
                    .ToList();
                await PreencherNomesEmpresasAsync(vagasDto);

                return Ok(vagasDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // GET /api/vagas/empresa/{empresaId} — Vagas de UMA empresa
        // ===========================================
        [AllowAnonymous]
        [HttpGet("empresa/{empresaId}")]
        public async Task<IActionResult> ListarPorEmpresa(Guid empresaId)
        {
            try
            {
                var resultado = await _dbContext.Vagas
                    .Where(v => v.EmpresaId == empresaId)
                    .OrderByDescending(v => v.CriadoEm)
                    .ToListAsync();

                var vagasDto = resultado.Select(MapearParaDto).ToList();
                await PreencherNomesEmpresasAsync(vagasDto);
                return Ok(vagasDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // GET /api/vagas/{id} — Busca UMA vaga específica
        // ===========================================
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            try
            {
                var resultado = await _dbContext.Vagas
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (resultado == null)
                    return NotFound(new { mensagem = "Vaga não encontrada" });

                var dto = MapearParaDto(resultado);
                await PreencherNomesEmpresasAsync(new[] { dto });
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // POST /api/vagas — Cria uma nova vaga
        // ===========================================
        [Authorize]
        [AuthorizeRole(TiposUsuario.Empresa)]
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CreateVagaDto dto)
        {
            try
            {
                // Validação básica
                if (string.IsNullOrWhiteSpace(dto.Titulo))
                    return BadRequest(new { erro = "O título da vaga é obrigatório" });

                // Cria a Model a partir do DTO de entrada
                var novaVaga = new Vaga
                {
                    // A vaga sempre pertence à empresa do token, não ao valor enviado no corpo
                    EmpresaId = EmpresaLogadaId(),
                    Titulo = dto.Titulo,
                    Descricao = dto.Descricao,
                    Area = dto.Area,
                    Nivel = dto.Nivel,
                    CursosAceitos = dto.CursosAceitos,
                    SemestreMinimo = dto.SemestreMinimo,
                    HabilidadesRequeridas = dto.HabilidadesRequeridas,
                    HabilidadesDiferenciais = dto.HabilidadesDiferenciais,
                    CargaHoraria = dto.CargaHoraria,
                    Modalidade = dto.Modalidade,
                    Cidade = dto.Cidade,
                    Estado = dto.Estado,
                    Bolsa = dto.Bolsa,
                    Beneficios = dto.Beneficios,
                    VagasDisponiveis = dto.VagasDisponiveis,
                    CandidaturasRecebidas = 0,
                    Status = dto.Status ?? "aberta",
                    ExpiraEm = dto.ExpiraEm
                };

                await _dbContext.Vagas.AddAsync(novaVaga);
                await _dbContext.SaveChangesAsync();

                var vagaCriada = MapearParaDto(novaVaga);
                return CreatedAtAction(nameof(BuscarPorId), new { id = vagaCriada.Id }, vagaCriada);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // PUT /api/vagas/{id} — Atualiza uma vaga existente
        // ===========================================
        [Authorize]
        [AuthorizeRole(TiposUsuario.Empresa)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] CreateVagaDto dto)
        {
            try
            {
                var existente = await _dbContext.Vagas
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (existente == null)
                    return NotFound(new { mensagem = "Vaga não encontrada" });

                // Só a empresa dona da vaga pode alterá-la
                if (existente.EmpresaId != EmpresaLogadaId())
                    return Forbid();

                existente.Titulo = dto.Titulo;
                existente.Descricao = dto.Descricao;
                existente.Area = dto.Area;
                existente.Nivel = dto.Nivel;
                existente.CursosAceitos = dto.CursosAceitos;
                existente.SemestreMinimo = dto.SemestreMinimo;
                existente.HabilidadesRequeridas = dto.HabilidadesRequeridas;
                existente.HabilidadesDiferenciais = dto.HabilidadesDiferenciais;
                existente.CargaHoraria = dto.CargaHoraria;
                existente.Modalidade = dto.Modalidade;
                existente.Cidade = dto.Cidade;
                existente.Estado = dto.Estado;
                existente.Bolsa = dto.Bolsa;
                existente.Beneficios = dto.Beneficios;
                existente.VagasDisponiveis = dto.VagasDisponiveis;
                existente.Status = dto.Status;
                existente.ExpiraEm = dto.ExpiraEm;

                _dbContext.Vagas.Update(existente);
                await _dbContext.SaveChangesAsync();

                return Ok(MapearParaDto(existente));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // PATCH /api/vagas/{id}/status — Atualiza só o status
        // ===========================================
        [Authorize]
        [AuthorizeRole(TiposUsuario.Empresa)]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> AtualizarStatus(int id, [FromBody] string novoStatus)
        {
            try
            {
                var existente = await _dbContext.Vagas
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (existente == null)
                    return NotFound(new { mensagem = "Vaga não encontrada" });

                // Só a empresa dona da vaga pode alterá-la
                if (existente.EmpresaId != EmpresaLogadaId())
                    return Forbid();

                existente.Status = novoStatus;
                _dbContext.Vagas.Update(existente);
                await _dbContext.SaveChangesAsync();
                return Ok(MapearParaDto(existente));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // DELETE /api/vagas/{id} — Deleta uma vaga
        // ===========================================
        [Authorize]
        [AuthorizeRole(TiposUsuario.Empresa)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            try
            {
                var existente = await _dbContext.Vagas
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (existente == null)
                    return NotFound(new { mensagem = "Vaga não encontrada" });

                // Só a empresa dona da vaga pode alterá-la
                if (existente.EmpresaId != EmpresaLogadaId())
                    return Forbid();

                _dbContext.Vagas.Remove(existente);
                await _dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // Id da empresa autenticada (claim conta_id do token)
        // ===========================================
        private Guid EmpresaLogadaId() =>
            Guid.TryParse(User.FindFirst("conta_id")?.Value, out var id) ? id : Guid.Empty;

        // ===========================================
        // MÉTODO PRIVADO — Converte Model em DTO
        // ===========================================
        // ===========================================
        // PRIORIDADE: vagas de empresas cadastradas pelo site
        // ===========================================
        // Empresas com criado_por_admin = true são as de exemplo (sem login).
        // As vagas delas ficam depois das vagas das empresas que se cadastraram.
        private async Task<HashSet<Guid>> ObterEmpresasDeExemploAsync()
        {
            var ids = await _dbContext.Empresas
                .Where(e => e.CriadoPorAdmin == true)
                .Select(e => e.Id)
                .ToListAsync();

            return ids.ToHashSet();
        }

        // ===========================================
        // NOME DA EMPRESA — o estudante vê quem anuncia a vaga (e não o id)
        // ===========================================
        private async Task<Dictionary<Guid, string>> ObterNomesEmpresasAsync(IEnumerable<Guid> empresaIds)
        {
            var ids = empresaIds.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<Guid, string>();

            var empresas = await _dbContext.Empresas
                .Where(e => ids.Contains(e.Id))
                .Select(e => new { e.Id, e.NomeFantasia, e.RazaoSocial })
                .ToListAsync();

            return empresas.ToDictionary(
                e => e.Id,
                e => string.IsNullOrWhiteSpace(e.NomeFantasia) ? e.RazaoSocial : e.NomeFantasia!);
        }

        private async Task PreencherNomesEmpresasAsync(IEnumerable<VagaDto> vagas)
        {
            var lista = vagas.ToList();
            var nomes = await ObterNomesEmpresasAsync(lista.Select(v => v.EmpresaId));
            foreach (var v in lista)
                v.EmpresaNome = nomes.GetValueOrDefault(v.EmpresaId);
        }

        private static IEnumerable<Vaga> OrdenarPorPrioridade(IEnumerable<Vaga> vagas, HashSet<Guid> empresasExemplo)
        {
            return vagas
                .OrderBy(v => empresasExemplo.Contains(v.EmpresaId)) // false (empresa cadastrada) primeiro
                .ThenByDescending(v => v.CriadoEm);
        }

        private static VagaDto MapearParaDto(Vaga vaga, HashSet<Guid> empresasExemplo)
        {
            var dto = MapearParaDto(vaga);
            dto.EmpresaCadastrada = !empresasExemplo.Contains(vaga.EmpresaId);
            return dto;
        }

        private static VagaDto MapearParaDto(Vaga vaga)
        {
            return new VagaDto
            {
                Id = vaga.Id,
                EmpresaId = vaga.EmpresaId,
                Titulo = vaga.Titulo,
                Descricao = vaga.Descricao,
                Area = vaga.Area,
                Nivel = vaga.Nivel,
                CursosAceitos = vaga.CursosAceitos,
                SemestreMinimo = vaga.SemestreMinimo,
                HabilidadesRequeridas = vaga.HabilidadesRequeridas,
                HabilidadesDiferenciais = vaga.HabilidadesDiferenciais,
                CargaHoraria = vaga.CargaHoraria,
                Modalidade = vaga.Modalidade,
                Cidade = vaga.Cidade,
                Estado = vaga.Estado,
                Bolsa = vaga.Bolsa,
                Beneficios = vaga.Beneficios,
                VagasDisponiveis = vaga.VagasDisponiveis,
                CandidaturasRecebidas = vaga.CandidaturasRecebidas,
                Status = vaga.Status,
                CriadoEm = vaga.CriadoEm,
                ExpiraEm = vaga.ExpiraEm,
                AtualizadoEm = vaga.AtualizadoEm
            };
        }

         // ===========================================
         // ENDPOINT DE RECOMENDAÇÃO
         // ===========================================
         [HttpGet("recomendadas/{estudanteId}")]
         public async Task<IActionResult> ObterRecomendadas(Guid estudanteId)
         {
             try
             {
                 var matches = await _dbContext.Matches
                     .Where(m => m.EstudanteId == estudanteId)
                     .OrderByDescending(m => m.ScoreTotal)
                     .Take(60)
                     .ToListAsync();

                 var vagasCache = new Dictionary<int, Vaga>();
                 var candidatos = new List<(Match match, Vaga vaga)>();

                 foreach (var match in matches)
                 {
                     if (!vagasCache.TryGetValue(match.VagaId, out var vaga))
                     {
                         vaga = await _dbContext.Vagas
                             .FirstOrDefaultAsync(v => v.Id == match.VagaId);

                         if (vaga != null)
                             vagasCache[match.VagaId] = vaga;
                     }

                     if (vaga != null)
                         candidatos.Add((match, vaga));
                 }

                 // Vagas de empresas cadastradas pelo site vêm antes das de exemplo;
                 // dentro de cada grupo, maior score primeiro
                 var empresasExemplo = await ObterEmpresasDeExemploAsync();
                 candidatos = candidatos
                     .OrderBy(c => empresasExemplo.Contains(c.vaga.EmpresaId))
                     .ThenByDescending(c => c.match.ScoreTotal)
                     .ToList();

                 var maxPorArea = 2;
                 var limiteFinal = 5;
                 var contagemArea = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                 var selecionados = new List<(Match match, Vaga vaga)>();

                 var porArea = candidatos
                     .GroupBy(c => string.IsNullOrWhiteSpace(c.vaga.Area) ? "Outras" : c.vaga.Area.Trim(), StringComparer.OrdinalIgnoreCase)
                     .Select(g => g.First())
                     .OrderBy(c => empresasExemplo.Contains(c.vaga.EmpresaId))
                     .ThenByDescending(c => c.match.ScoreTotal)
                     .ToList();

                 foreach (var item in porArea)
                 {
                     if (selecionados.Count >= 3) break;
                     selecionados.Add(item);
                     var area = string.IsNullOrWhiteSpace(item.vaga.Area) ? "Outras" : item.vaga.Area.Trim();
                     contagemArea[area] = 1;
                 }

                 foreach (var item in candidatos)
                 {
                     if (selecionados.Count >= limiteFinal) break;
                     if (selecionados.Any(s => s.match.VagaId == item.match.VagaId)) continue;

                     var area = string.IsNullOrWhiteSpace(item.vaga.Area) ? "Outras" : item.vaga.Area.Trim();
                     contagemArea.TryGetValue(area, out var atual);
                     if (atual >= maxPorArea) continue;

                     selecionados.Add(item);
                     contagemArea[area] = atual + 1;
                 }

                 if (selecionados.Count < limiteFinal)
                 {
                     foreach (var item in candidatos)
                     {
                         if (selecionados.Count >= limiteFinal) break;
                         if (selecionados.Any(s => s.match.VagaId == item.match.VagaId)) continue;
                         selecionados.Add(item);
                     }
                 }

                 var nomesEmpresas = await ObterNomesEmpresasAsync(selecionados.Select(s => s.vaga.EmpresaId));

                 var recomendadas = selecionados.Select(item => new
                     {
                         // Vaga data
                         Id = item.vaga.Id,
                         EmpresaId = item.vaga.EmpresaId,
                         Titulo = item.vaga.Titulo,
                         Descricao = item.vaga.Descricao,
                         Area = item.vaga.Area,
                         Nivel = item.vaga.Nivel,
                         CursosAceitos = item.vaga.CursosAceitos,
                         SemestreMinimo = item.vaga.SemestreMinimo,
                         HabilidadesRequeridas = item.vaga.HabilidadesRequeridas,
                         HabilidadesDiferenciais = item.vaga.HabilidadesDiferenciais,
                         CargaHoraria = item.vaga.CargaHoraria,
                         Modalidade = item.vaga.Modalidade,
                         Cidade = item.vaga.Cidade,
                         Estado = item.vaga.Estado,
                         Bolsa = item.vaga.Bolsa,
                         Beneficios = item.vaga.Beneficios,
                         VagasDisponiveis = item.vaga.VagasDisponiveis,
                         Status = item.vaga.Status,
                         EmpresaCadastrada = !empresasExemplo.Contains(item.vaga.EmpresaId),
                         EmpresaNome = nomesEmpresas.GetValueOrDefault(item.vaga.EmpresaId),
                         // Score data
                         ScoreTotal = item.match.ScoreTotal,
                         ScoreCurriculo = item.match.ScoreCurriculo,
                         ScoreVocacional = item.match.ScoreVocacional,
                         ScoreHabilidades = item.match.ScoreHabilidades,
                         Justificativa = item.match.Justificativa
                     })
                     .ToList();

                 return Ok(recomendadas);
             }
             catch (Exception ex)
             {
                 return StatusCode(500, new { erro = ex.Message });
             }
        }
    }
}
