using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Exceptions;
using OpenDoors.Api.Services;

namespace OpenDoors.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly LoginService _loginService;

        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }

        // POST /api/login
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Autenticar([FromBody] LoginDto dto)
        {
            try
            {
                var response = await _loginService.AutenticarAsync(dto);
                return Ok(response);
            }
            catch (MissingRequiredFieldException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (InvalidCredentialsException ex)
            {
                return Unauthorized(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // PUT /api/login/senha
        [Authorize]
        [HttpPut("senha")]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto dto)
        {
            if (!Guid.TryParse(User.FindFirst("conta_id")?.Value, out var contaId))
                return Unauthorized();

            try
            {
                await _loginService.AlterarSenhaAsync(contaId, dto.SenhaAtual, dto.NovaSenha);
                return NoContent();
            }
            catch (MissingRequiredFieldException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (InvalidCredentialsException)
            {
                return BadRequest(new { erro = "Senha atual incorreta." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }
    }
}
