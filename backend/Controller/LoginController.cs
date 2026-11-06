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
        private readonly GoogleAuthService _googleAuth;

        public LoginController(LoginService loginService, GoogleAuthService googleAuth)
        {
            _loginService = loginService;
            _googleAuth = googleAuth;
        }

        // GET /api/login/config — dados públicos que o frontend precisa (Client ID do Google)
        [AllowAnonymous]
        [HttpGet("config")]
        public IActionResult Config() => Ok(new { googleClientId = _googleAuth.ClientId });

        // POST /api/login/google
        // 200 com token quando a conta existe; 404 com nome/e-mail do Google quando precisa se cadastrar
        [AllowAnonymous]
        [HttpPost("google")]
        public async Task<IActionResult> AutenticarGoogle([FromBody] LoginGoogleDto dto)
        {
            try
            {
                var (resposta, usuario) = await _loginService.AutenticarGoogleAsync(dto.Credential);
                if (resposta == null)
                    return NotFound(new
                    {
                        erro = "Ainda não existe conta com este e-mail. Complete o cadastro.",
                        precisaCadastro = true,
                        email = usuario.Email,
                        nome = usuario.Nome
                    });

                return Ok(resposta);
            }
            catch (MissingRequiredFieldException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (InvalidCredentialsException ex)
            {
                return Unauthorized(new { erro = ex.Message });
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

        // POST /api/login/esqueci-senha — sempre responde 204 (não revela se o e-mail tem conta)
        [AllowAnonymous]
        [HttpPost("esqueci-senha")]
        public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaDto dto)
        {
            try
            {
                await _loginService.SolicitarRedefinicaoAsync(dto.Email);
                return NoContent();
            }
            catch (MissingRequiredFieldException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = "Não foi possível enviar o e-mail agora. Tente novamente.", detalhe = ex.Message });
            }
        }

        // POST /api/login/redefinir-senha
        [AllowAnonymous]
        [HttpPost("redefinir-senha")]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaDto dto)
        {
            try
            {
                await _loginService.RedefinirSenhaAsync(dto.Email, dto.Codigo, dto.NovaSenha);
                return NoContent();
            }
            catch (MissingRequiredFieldException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (InvalidCredentialsException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
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
