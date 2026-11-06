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
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresasController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly LoginService _loginService;

        public EmpresasController(AppDbContext dbContext, LoginService loginService)
        {
            _dbContext = dbContext;
            _loginService = loginService;
        }

        // GET /api/empresas
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ListarTodas()
        {
            try
            {
                var resultado = await _dbContext.Empresas.ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/empresas/ativas
        [AllowAnonymous]
        [HttpGet("ativas")]
        public async Task<IActionResult> ListarAtivas()
        {
            try
            {
                var resultado = await _dbContext.Empresas
                    .Where(e => e.Status == "ativa")
                    .ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/empresas/{id}
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(Guid id)
        {
            try
            {
                var resultado = await _dbContext.Empresas
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (resultado == null)
                    return NotFound(new { mensagem = "Empresa não encontrada" });

                return Ok(MapearParaDto(resultado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // POST /api/empresas
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CreateEmpresaDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.RazaoSocial))
                    return BadRequest(new { erro = "Razão social é obrigatória" });

                if (string.IsNullOrWhiteSpace(dto.Cnpj))
                    return BadRequest(new { erro = "CNPJ é obrigatório" });

                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest(new { erro = "Email é obrigatório" });

                if (string.IsNullOrWhiteSpace(dto.Senha) || dto.Senha.Length < LoginService.TamanhoMinimoSenha)
                    return BadRequest(new { erro = $"Senha é obrigatória (mínimo {LoginService.TamanhoMinimoSenha} caracteres)" });

                // Verifica se o email já está cadastrado
                if (await _loginService.EmailJaCadastradoAsync(dto.Email))
                    return Conflict(new { erro = "Email já cadastrado no sistema" });

                var nova = new Empresa
                {
                    // O Id da conta é gerado pelo backend e vira o conta_id do login/token
                    Id = Guid.NewGuid(),
                    RazaoSocial = dto.RazaoSocial,
                    NomeFantasia = dto.NomeFantasia,
                    Cnpj = dto.Cnpj,
                    Email = LoginService.NormalizarEmail(dto.Email),
                    Telefone = dto.Telefone,
                    Site = dto.Site,
                    LogoUrl = dto.LogoUrl,
                    Cidade = dto.Cidade,
                    Estado = dto.Estado,
                    Cep = dto.Cep,
                    Setor = dto.Setor,
                    Porte = dto.Porte,
                    Descricao = dto.Descricao,
                    ResponsavelNome = dto.ResponsavelNome,
                    ResponsavelCargo = dto.ResponsavelCargo,
                    ResponsavelEmail = dto.ResponsavelEmail,
                    VagasAtivas = 0,
                    TotalContratacoes = 0,
                    Status = dto.Status ?? "ativa"
                };

                await _dbContext.Empresas.AddAsync(nova);

                // Perfil e login são gravados juntos: se um falhar, nenhum é salvo
                _loginService.AdicionarLogin(new CreateLoginDto
                {
                    ContaId = nova.Id,
                    Email = dto.Email,
                    Senha = dto.Senha,
                    TipoUsuario = TiposUsuario.Empresa
                });

                await _dbContext.SaveChangesAsync();

                var criada = MapearParaDto(nova);
                return CreatedAtAction(nameof(BuscarPorId), new { id = criada.Id }, criada);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // PUT /api/empresas/{id}
        [Authorize]
        [AuthorizeRole(TiposUsuario.Empresa)]
        [AuthorizeOwner("id")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] CreateEmpresaDto dto)
        {
            try
            {
                var existente = await _dbContext.Empresas
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (existente == null)
                    return NotFound(new { mensagem = "Empresa não encontrada" });

                // Mantém o email do login igual ao do perfil
                if (!string.IsNullOrWhiteSpace(dto.Email) &&
                    LoginService.NormalizarEmail(dto.Email) != LoginService.NormalizarEmail(existente.Email))
                {
                    if (!await _loginService.SincronizarEmailAsync(id, dto.Email))
                        return Conflict(new { erro = "Email já cadastrado no sistema" });

                    existente.Email = LoginService.NormalizarEmail(dto.Email);
                }

                existente.RazaoSocial = dto.RazaoSocial;
                existente.NomeFantasia = dto.NomeFantasia;
                existente.Cnpj = dto.Cnpj;
                existente.Telefone = dto.Telefone;
                existente.Site = dto.Site;
                existente.LogoUrl = dto.LogoUrl;
                existente.Cidade = dto.Cidade;
                existente.Estado = dto.Estado;
                existente.Cep = dto.Cep;
                existente.Setor = dto.Setor;
                existente.Porte = dto.Porte;
                existente.Descricao = dto.Descricao;
                existente.ResponsavelNome = dto.ResponsavelNome;
                existente.ResponsavelCargo = dto.ResponsavelCargo;
                existente.ResponsavelEmail = dto.ResponsavelEmail;
                existente.Status = dto.Status;

                _dbContext.Empresas.Update(existente);
                await _dbContext.SaveChangesAsync();
                return Ok(MapearParaDto(existente));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // DELETE /api/empresas/{id}
        [Authorize]
        [AuthorizeRole(TiposUsuario.Empresa)]
        [AuthorizeOwner("id")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(Guid id)
        {
            try
            {
                var existente = await _dbContext.Empresas
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (existente == null)
                    return NotFound(new { mensagem = "Empresa não encontrada" });

                _dbContext.Empresas.Remove(existente);
                await _dbContext.SaveChangesAsync();
            
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        private static EmpresaDto MapearParaDto(Empresa e)
        {
            return new EmpresaDto
            {
                Id = e.Id,
                RazaoSocial = e.RazaoSocial,
                NomeFantasia = e.NomeFantasia,
                Cnpj = e.Cnpj,
                Email = e.Email,
                Telefone = e.Telefone,
                Site = e.Site,
                LogoUrl = e.LogoUrl,
                Cidade = e.Cidade,
                Estado = e.Estado,
                Cep = e.Cep,
                Setor = e.Setor,
                Porte = e.Porte,
                Descricao = e.Descricao,
                ResponsavelNome = e.ResponsavelNome,
                ResponsavelCargo = e.ResponsavelCargo,
                ResponsavelEmail = e.ResponsavelEmail,
                VagasAtivas = e.VagasAtivas,
                TotalContratacoes = e.TotalContratacoes,
                Status = e.Status
            };
        }
    }
}