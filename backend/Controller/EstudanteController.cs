using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EstudantesController : ControllerBase
    {
        private readonly Supabase.Client _supabase;

        public EstudantesController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        // GET /api/estudantes
        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            try
            {
                var resultado = await _supabase.From<Estudante>().Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/estudantes/ativos
        [HttpGet("ativos")]
        public async Task<IActionResult> ListarAtivos()
        {
            try
            {
                var resultado = await _supabase
                    .From<Estudante>()
                    .Where(e => e.Status == "ativo")
                    .Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/estudantes/{id}
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(Guid id)
        {
            try
            {
                var resultado = await _supabase
                    .From<Estudante>()
                    .Where(e => e.Id == id)
                    .Single();

                if (resultado == null)
                    return NotFound(new { mensagem = "Estudante não encontrado" });

                return Ok(MapearParaDto(resultado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/estudantes/por-email?email=teste@x.com
        [AllowAnonymous]
        [HttpGet("por-email")]
        public async Task<IActionResult> BuscarPorEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { erro = "E-mail é obrigatório" });

            try
            {
                var resultado = await _supabase
                    .From<Estudante>()
                    .Where(e => e.Email == email)
                    .Single();

                if (resultado == null)
                    return NotFound(new { mensagem = "Estudante não encontrado" });

                return Ok(MapearParaDto(resultado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // POST /api/estudantes
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CreateEstudanteDto dto)
        {
            try
            {
                if (dto.Id == Guid.Empty)
                    return BadRequest(new { erro = "O Id (UUID do Supabase Auth) é obrigatório" });

                if (string.IsNullOrWhiteSpace(dto.Nome))
                    return BadRequest(new { erro = "Nome é obrigatório" });

                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest(new { erro = "Email é obrigatório" });

                var novo = new Estudante
                {
                    Id = dto.Id,
                    Nome = dto.Nome,
                    Email = dto.Email,
                    Telefone = dto.Telefone,
                    Cpf = dto.Cpf,
                    DataNascimento = dto.DataNascimento,
                    Cidade = dto.Cidade,
                    Estado = dto.Estado,
                    FotoPerfilUrl = dto.FotoPerfilUrl,
                    Instituicao = dto.Instituicao,
                    Curso = dto.Curso,
                    Semestre = dto.Semestre,
                    Turno = dto.Turno,
                    PrevisaoConclusao = dto.PrevisaoConclusao,
                    CurriculoUrl = dto.CurriculoUrl,
                    HabilidadesExtraidas = dto.HabilidadesExtraidas,
                    TemCurriculo = dto.TemCurriculo ?? false,
                    TemTesteVocacional = dto.TemTesteVocacional ?? false,
                    Status = dto.Status ?? "ativo"
                };

                var resultado = await _supabase.From<Estudante>().Insert(novo);

                if (resultado.Models == null || resultado.Models.Count == 0)
                    return StatusCode(500, new { erro = "Falha ao criar estudante" });

                var criado = MapearParaDto(resultado.Models[0]);
                return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, criado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // PUT /api/estudantes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] CreateEstudanteDto dto)
        {
            try
            {
                var existente = await _supabase
                    .From<Estudante>()
                    .Where(e => e.Id == id)
                    .Single();

                if (existente == null)
                    return NotFound(new { mensagem = "Estudante não encontrado" });

                existente.Nome = dto.Nome;
                existente.Email = dto.Email;
                existente.Telefone = dto.Telefone;
                existente.Cpf = dto.Cpf;
                existente.DataNascimento = dto.DataNascimento;
                existente.Cidade = dto.Cidade;
                existente.Estado = dto.Estado;
                existente.FotoPerfilUrl = dto.FotoPerfilUrl;
                existente.Instituicao = dto.Instituicao;
                existente.Curso = dto.Curso;
                existente.Semestre = dto.Semestre;
                existente.Turno = dto.Turno;
                existente.PrevisaoConclusao = dto.PrevisaoConclusao;
                existente.CurriculoUrl = dto.CurriculoUrl;
                existente.HabilidadesExtraidas = dto.HabilidadesExtraidas;
                existente.TemCurriculo = dto.TemCurriculo;
                existente.TemTesteVocacional = dto.TemTesteVocacional;
                existente.Status = dto.Status;

                var resultado = await existente.Update<Estudante>();
                return Ok(MapearParaDto(resultado.Models[0]));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // DELETE /api/estudantes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(Guid id)
        {
            try
            {
                var existente = await _supabase
                    .From<Estudante>()
                    .Where(e => e.Id == id)
                    .Single();

                if (existente == null)
                    return NotFound(new { mensagem = "Estudante não encontrado" });

                await _supabase
                    .From<Estudante>()
                    .Where(e => e.Id == id)
                    .Delete();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        private static EstudanteDto MapearParaDto(Estudante e)
        {
            return new EstudanteDto
            {
                Id = e.Id,
                Nome = e.Nome,
                Email = e.Email,
                Telefone = e.Telefone,
                Cpf = e.Cpf,
                DataNascimento = e.DataNascimento,
                Cidade = e.Cidade,
                Estado = e.Estado,
                FotoPerfilUrl = e.FotoPerfilUrl,
                Instituicao = e.Instituicao,
                Curso = e.Curso,
                Semestre = e.Semestre,
                Turno = e.Turno,
                PrevisaoConclusao = e.PrevisaoConclusao,
                CurriculoUrl = e.CurriculoUrl,
                HabilidadesExtraidas = e.HabilidadesExtraidas,
                TemCurriculo = e.TemCurriculo,
                TemTesteVocacional = e.TemTesteVocacional,
                Status = e.Status,
                CriadoEm = e.CriadoEm,
                AtualizadoEm = e.AtualizadoEm
            };
        }
    }
}