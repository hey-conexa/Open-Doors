using Microsoft.AspNetCore.Mvc;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VagasController : ControllerBase
    {
        private readonly Supabase.Client _supabase;

        public VagasController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        // ===========================================
        // GET /api/vagas — Lista TODAS as vagas
        // ===========================================
        [HttpGet]
        public async Task<IActionResult> ListarTodas()
        {
            try
            {
                var resultado = await _supabase
                    .From<Vaga>()
                    .Get();

                // Converte cada Model em DTO (sem os campos internos do Supabase)
                var vagasDto = resultado.Models.Select(MapearParaDto).ToList();

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
        [HttpGet("abertas")]
        public async Task<IActionResult> ListarAbertas()
        {
            try
            {
                var resultado = await _supabase
                    .From<Vaga>()
                    .Where(v => v.Status == "aberta")
                    .Get();

                var vagasDto = resultado.Models.Select(MapearParaDto).ToList();

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
        [HttpGet("empresa/{empresaId}")]
        public async Task<IActionResult> ListarPorEmpresa(Guid empresaId)
        {
            try
            {
                var resultado = await _supabase
                    .From<Vaga>()
                    .Where(v => v.EmpresaId == empresaId)
                    .Order("criado_em", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                var vagasDto = resultado.Models.Select(MapearParaDto).ToList();
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
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            try
            {
                var resultado = await _supabase
                    .From<Vaga>()
                    .Where(v => v.Id == id)
                    .Single();

                if (resultado == null)
                    return NotFound(new { mensagem = "Vaga não encontrada" });

                return Ok(MapearParaDto(resultado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // POST /api/vagas — Cria uma nova vaga
        // ===========================================
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CreateVagaDto dto)
        {
            try
            {
                // Validação básica
                if (string.IsNullOrWhiteSpace(dto.Titulo))
                    return BadRequest(new { erro = "O título da vaga é obrigatório" });

                if (dto.EmpresaId == Guid.Empty)
                    return BadRequest(new { erro = "A empresa é obrigatória" });

                // Cria a Model a partir do DTO de entrada
                var novaVaga = new Vaga
                {
                    EmpresaId = dto.EmpresaId,
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
                    CandidaturasRecebidas = 0, // Sempre começa em zero
                    Status = dto.Status ?? "aberta", // Default "aberta"
                    ExpiraEm = dto.ExpiraEm
                };

                // Insere no Supabase
                var resultado = await _supabase.From<Vaga>().Insert(novaVaga);

                if (resultado.Models == null || resultado.Models.Count == 0)
                    return StatusCode(500, new { erro = "Falha ao criar vaga" });

                // Retorna 201 Created com a vaga criada
                var vagaCriada = MapearParaDto(resultado.Models[0]);
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
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] CreateVagaDto dto)
        {
            try
            {
                // Verifica se a vaga existe
                var existente = await _supabase
                    .From<Vaga>()
                    .Where(v => v.Id == id)
                    .Single();

                if (existente == null)
                    return NotFound(new { mensagem = "Vaga não encontrada" });

                // Atualiza os campos
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

                var resultado = await existente.Update<Vaga>();

                return Ok(MapearParaDto(resultado.Models[0]));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // PATCH /api/vagas/{id}/status — Atualiza só o status
        // ===========================================
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> AtualizarStatus(int id, [FromBody] string novoStatus)
        {
            try
            {
                var existente = await _supabase
                    .From<Vaga>()
                    .Where(v => v.Id == id)
                    .Single();

                if (existente == null)
                    return NotFound(new { mensagem = "Vaga não encontrada" });

                existente.Status = novoStatus;
                var resultado = await existente.Update<Vaga>();
                return Ok(MapearParaDto(resultado.Models[0]));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // DELETE /api/vagas/{id} — Deleta uma vaga
        // ===========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            try
            {
                var existente = await _supabase
                    .From<Vaga>()
                    .Where(v => v.Id == id)
                    .Single();

                if (existente == null)
                    return NotFound(new { mensagem = "Vaga não encontrada" });

                await _supabase
                    .From<Vaga>()
                    .Where(v => v.Id == id)
                    .Delete();

                return NoContent(); // HTTP 204
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // ===========================================
        // MÉTODO PRIVADO — Converte Model em DTO
        // ===========================================
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
                 var matches = await _supabase
                     .From<Match>()
                     .Where(m => m.EstudanteId == estudanteId)
                     .Order("score_total", Supabase.Postgrest.Constants.Ordering.Descending)
                     .Limit(60)
                     .Get();

                 var vagasCache = new Dictionary<int, Vaga>();
                 var candidatos = new List<(Match match, Vaga vaga)>();

                 foreach (var match in matches.Models)
                 {
                     if (!vagasCache.TryGetValue(match.VagaId, out var vaga))
                     {
                         vaga = await _supabase
                             .From<Vaga>()
                             .Where(v => v.Id == match.VagaId)
                             .Single();

                         if (vaga != null)
                             vagasCache[match.VagaId] = vaga;
                     }

                     if (vaga != null)
                         candidatos.Add((match, vaga));
                 }

                 var maxPorArea = 2;
                 var limiteFinal = 5;
                 var contagemArea = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                 var selecionados = new List<(Match match, Vaga vaga)>();

                 var porArea = candidatos
                     .GroupBy(c => string.IsNullOrWhiteSpace(c.vaga.Area) ? "Outras" : c.vaga.Area.Trim(), StringComparer.OrdinalIgnoreCase)
                     .Select(g => g.First())
                     .OrderByDescending(c => c.match.ScoreTotal)
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
