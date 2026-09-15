using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresasController : ControllerBase
    {
        private readonly Supabase.Client _supabase;

        public EmpresasController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        // GET /api/empresas
        [HttpGet]
        public async Task<IActionResult> ListarTodas()
        {
            try
            {
                var resultado = await _supabase.From<Empresa>().Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/empresas/ativas
        [HttpGet("ativas")]
        public async Task<IActionResult> ListarAtivas()
        {
            try
            {
                var resultado = await _supabase
                    .From<Empresa>()
                    .Where(e => e.Status == "ativa")
                    .Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/empresas/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(Guid id)
        {
            try
            {
                var resultado = await _supabase
                    .From<Empresa>()
                    .Where(e => e.Id == id)
                    .Single();

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
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CreateEmpresaDto dto)
        {
            try
            {
                if (dto.Id == Guid.Empty)
                    return BadRequest(new { erro = "O Id (UUID do Supabase Auth) é obrigatório" });

                if (string.IsNullOrWhiteSpace(dto.RazaoSocial))
                    return BadRequest(new { erro = "Razão social é obrigatória" });

                if (string.IsNullOrWhiteSpace(dto.Cnpj))
                    return BadRequest(new { erro = "CNPJ é obrigatório" });

                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest(new { erro = "Email é obrigatório" });

                var nova = new Empresa
                {
                    Id = dto.Id,
                    RazaoSocial = dto.RazaoSocial,
                    NomeFantasia = dto.NomeFantasia,
                    Cnpj = dto.Cnpj,
                    Email = dto.Email,
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

                var resultado = await _supabase.From<Empresa>().Insert(nova);

                if (resultado.Models == null || resultado.Models.Count == 0)
                    return StatusCode(500, new { erro = "Falha ao criar empresa" });

                var criada = MapearParaDto(resultado.Models[0]);
                return CreatedAtAction(nameof(BuscarPorId), new { id = criada.Id }, criada);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // PUT /api/empresas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] CreateEmpresaDto dto)
        {
            try
            {
                var existente = await _supabase
                    .From<Empresa>()
                    .Where(e => e.Id == id)
                    .Single();

                if (existente == null)
                    return NotFound(new { mensagem = "Empresa não encontrada" });

                existente.RazaoSocial = dto.RazaoSocial;
                existente.NomeFantasia = dto.NomeFantasia;
                existente.Cnpj = dto.Cnpj;
                existente.Email = dto.Email;
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

                var resultado = await existente.Update<Empresa>();
                return Ok(MapearParaDto(resultado.Models[0]));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // DELETE /api/empresas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(Guid id)
        {
            try
            {
                var existente = await _supabase
                    .From<Empresa>()
                    .Where(e => e.Id == id)
                    .Single();

                if (existente == null)
                    return NotFound(new { mensagem = "Empresa não encontrada" });

                await _supabase
                    .From<Empresa>()
                    .Where(e => e.Id == id)
                    .Delete();

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
                Status = e.Status,
                CriadoEm = e.CriadoEm,
                AtualizadoEm = e.AtualizadoEm
            };
        }
    }
}