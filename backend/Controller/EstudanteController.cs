using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Exceptions;
using OpenDoors.Api.Infrastructure.Database;
using OpenDoors.Api.Middleware;
using OpenDoors.Api.Models;
using OpenDoors.Api.Services;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EstudantesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly LoginService _loginService;
        private readonly GoogleAuthService _googleAuth;

        public EstudantesController(AppDbContext dbContext, LoginService loginService, GoogleAuthService googleAuth)
        {
            _dbContext = dbContext;
            _loginService = loginService;
            _googleAuth = googleAuth;
        }

        // GET /api/estudantes
        [AuthorizeRole(TiposUsuario.Estudante, TiposUsuario.Empresa)]
        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            try
            {
                var resultado = await _dbContext.Estudantes.ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/estudantes/ativos
        [AuthorizeRole(TiposUsuario.Estudante, TiposUsuario.Empresa)]
        [HttpGet("ativos")]
        public async Task<IActionResult> ListarAtivos()
        {
            try
            {
                var resultado = await _dbContext.Estudantes
                    .Where(e => e.Status == "ativo")
                    .ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
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
                var resultado = await _dbContext.Estudantes
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (resultado == null)
                    return NotFound(new { mensagem = "Estudante não encontrado" });

                return Ok(MapearParaDto(resultado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/estudantes/{id}/curriculo — baixa o PDF guardado no banco
        // Pode baixar: o próprio estudante ou qualquer empresa (para avaliar candidatos)
        [AuthorizeRole(TiposUsuario.Estudante, TiposUsuario.Empresa)]
        [HttpGet("{id}/curriculo")]
        public async Task<IActionResult> BaixarCurriculo(Guid id)
        {
            var ehEmpresa = User.FindFirst("tipo_usuario")?.Value == nameof(TiposUsuario.Empresa);
            var ehDono = Guid.TryParse(User.FindFirst("conta_id")?.Value, out var contaId) && contaId == id;
            if (!ehEmpresa && !ehDono)
                return Forbid();

            var arquivo = await _dbContext.Curriculos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.EstudanteId == id);

            if (arquivo == null)
                return NotFound(new { mensagem = "Este estudante ainda não enviou currículo" });

            return File(arquivo.Conteudo, arquivo.ContentType, arquivo.NomeArquivo);
        }

        // POST /api/estudantes
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CreateEstudanteDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nome))
                    return BadRequest(new { erro = "Nome é obrigatório" });

                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest(new { erro = "Email é obrigatório" });

                // Cadastro pelo Google: o e-mail vem do token verificado e a senha não é necessária
                if (!string.IsNullOrWhiteSpace(dto.GoogleCredential))
                {
                    var google = await _googleAuth.ValidarAsync(dto.GoogleCredential);
                    dto.Email = google.Email;
                    dto.Senha = LoginService.SenhaAleatoria();
                }

                if (string.IsNullOrWhiteSpace(dto.Senha) || dto.Senha.Length < LoginService.TamanhoMinimoSenha)
                    return BadRequest(new { erro = $"Senha é obrigatória (mínimo {LoginService.TamanhoMinimoSenha} caracteres)" });

                // Verifica se o email já está cadastrado
                if (await _loginService.EmailJaCadastradoAsync(dto.Email))
                    return Conflict(new { erro = "Email já cadastrado no sistema" });

                var novo = new Estudante
                {
                    // O Id da conta é gerado pelo backend e vira o conta_id do login/token
                    Id = Guid.NewGuid(),
                    Nome = dto.Nome,
                    Email = LoginService.NormalizarEmail(dto.Email),
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

                await _dbContext.Estudantes.AddAsync(novo);

                // Perfil e login são gravados juntos: se um falhar, nenhum é salvo
                _loginService.AdicionarLogin(new CreateLoginDto
                {
                    ContaId = novo.Id,
                    Email = dto.Email,
                    Senha = dto.Senha,
                    TipoUsuario = TiposUsuario.Estudante
                });

                await _dbContext.SaveChangesAsync();

                var criado = MapearParaDto(novo);
                return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, criado);
            }
            catch (AuthenticationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // PUT /api/estudantes/{id}
        [AuthorizeRole(TiposUsuario.Estudante)]
        [AuthorizeOwner("id")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] CreateEstudanteDto dto)
        {
            try
            {
                var existente = await _dbContext.Estudantes
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (existente == null)
                    return NotFound(new { mensagem = "Estudante não encontrado" });

                // Mantém o email do login igual ao do perfil
                if (!string.IsNullOrWhiteSpace(dto.Email) &&
                    LoginService.NormalizarEmail(dto.Email) != LoginService.NormalizarEmail(existente.Email))
                {
                    if (!await _loginService.SincronizarEmailAsync(id, dto.Email))
                        return Conflict(new { erro = "Email já cadastrado no sistema" });

                    existente.Email = LoginService.NormalizarEmail(dto.Email);
                }

                existente.Nome = dto.Nome;
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

                _dbContext.Estudantes.Update(existente);
                await _dbContext.SaveChangesAsync();
                return Ok(MapearParaDto(existente));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // DELETE /api/estudantes/{id}
        [AuthorizeRole(TiposUsuario.Estudante)]
        [AuthorizeOwner("id")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(Guid id)
        {
            try
            {
                var existente = await _dbContext.Estudantes
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (existente == null)
                    return NotFound(new { mensagem = "Estudante não encontrado" });

                _dbContext.Estudantes.Remove(existente);
                await _dbContext.SaveChangesAsync();

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