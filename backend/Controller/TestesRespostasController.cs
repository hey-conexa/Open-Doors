using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Infrastructure.Database;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/testes-respostas")]
    public class TestesRespostasController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public TestesRespostasController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET /api/testes-respostas
        [HttpGet]
        public async Task<IActionResult> ListarTodas()
        {
            try
            {
                var resultado = await _dbContext.TestesRespostas.ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/testes-respostas/teste/{testeId}
        // Todas as respostas de um teste específico
        [HttpGet("teste/{testeId}")]
        public async Task<IActionResult> ListarPorTeste(int testeId)
        {
            try
            {
                var resultado = await _dbContext.TestesRespostas
                    .Where(r => r.TesteId == testeId)
                    .OrderBy(r => r.PerguntaId)
                    .ToListAsync();
                var dto = resultado.Select(MapearParaDto).ToList();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // POST /api/testes-respostas
        // Estudante envia suas respostas — salva no banco e dispara análise da IA
        //
        // Body esperado:
        // {
        //   "estudanteId": "guid-do-estudante",
        //   "respostas": [
        //     { "perguntaId": 1, "pergunta": "Texto da pergunta", "resposta": "Resposta do estudante" },
        //     ...
        //   ]
        // }
        [HttpPost]
        public async Task<IActionResult> ResponderTeste([FromBody] ResponderTesteDto body)
        {
            if (body.EstudanteId == Guid.Empty || body.Respostas == null || body.Respostas.Count == 0)
                return BadRequest(new { erro = "estudanteId e respostas são obrigatórios" });

            try
            {
                // 1. Verifica se o estudante já tem um registro de TesteVocacional
                //    (o resultado da IA fica aqui — pode ser que já exista de uma tentativa anterior)
                var testeExistente = await _dbContext.TestesVocacionais
                    .FirstOrDefaultAsync(t => t.EstudanteId == body.EstudanteId);

                int testeId;

                if (testeExistente != null)
                {
                    // Já existe: limpa as respostas antigas antes de salvar as novas
                    testeId = testeExistente.Id;

                    var respostasAntigas = await _dbContext.TestesRespostas
                        .Where(r => r.TesteId == testeId)
                        .ToListAsync();

                    foreach (var antiga in respostasAntigas)
                        _dbContext.TestesRespostas.Remove(antiga);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    // Ainda não existe: cria um registro de TesteVocacional vazio
                    // (a IA vai preencher depois via /api/ia/analisar-teste)
                    var novoTeste = new TesteVocacional
                    {
                        EstudanteId = body.EstudanteId,
                        AnalisadoIa = false
                    };

                    await _dbContext.TestesVocacionais.AddAsync(novoTeste);
                    await _dbContext.SaveChangesAsync();
                    testeId = novoTeste.Id;
                }

                // 2. Salva todas as respostas ligadas a esse TesteVocacional
                foreach (var r in body.Respostas)
                {
                    await _dbContext.TestesRespostas.AddAsync(new TesteResposta
                    {
                        TesteId = testeId,
                        PerguntaId = r.PerguntaId,
                        Pergunta = r.Pergunta,
                        Resposta = r.Resposta
                    });
                }

                await _dbContext.SaveChangesAsync();

                // 3. Retorna as respostas salvas + testeId para o frontend
                //    O frontend deve chamar POST /api/ia/analisar-teste com essas respostas
                //    para gerar o perfil vocacional via IA
                return Ok(new
                {
                    testeId,
                    mensagem = "Respostas salvas. Chame /api/ia/analisar-teste para gerar o perfil.",
                    respostasSalvas = body.Respostas.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        private static TesteRespostaDto MapearParaDto(TesteResposta r)
        {
            return new TesteRespostaDto
            {
                Id = r.Id,
                TesteId = r.TesteId,
                PerguntaId = r.PerguntaId,
                Pergunta = r.Pergunta,
                Resposta = r.Resposta
            };
        }
    }
}
