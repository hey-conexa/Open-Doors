using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Api.DTOs;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/testes-respostas")]
    public class TestesRespostasController : ControllerBase
    {
        private readonly Supabase.Client _supabase;

        public TestesRespostasController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        // GET /api/testes-respostas
        [HttpGet]
        public async Task<IActionResult> ListarTodas()
        {
            try
            {
                var resultado = await _supabase.From<TesteResposta>().Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
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
                var resultado = await _supabase
                    .From<TesteResposta>()
                    .Where(r => r.TesteId == testeId)
                    .Order(r => r.PerguntaId, Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();
                var dto = resultado.Models.Select(MapearParaDto).ToList();
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
                var testesExistentes = await _supabase
                    .From<TesteVocacional>()
                    .Where(t => t.EstudanteId == body.EstudanteId)
                    .Get();

                var testeExistente = testesExistentes.Models.FirstOrDefault();
                int testeId;

                if (testeExistente != null)
                {
                    // Já existe: limpa as respostas antigas antes de salvar as novas
                    testeId = testeExistente.Id;

                    var respostasAntigas = await _supabase
                        .From<TesteResposta>()
                        .Where(r => r.TesteId == testeId)
                        .Get();

                    foreach (var antiga in respostasAntigas.Models)
                        await antiga.Delete<TesteResposta>();
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

                    var testeInserido = await _supabase
                        .From<TesteVocacional>()
                        .Insert(novoTeste);

                    var testeCriado = testeInserido.Models.FirstOrDefault();
                    if (testeCriado == null)
                        return StatusCode(500, new { erro = "Erro ao criar registro do teste" });

                    testeId = testeCriado.Id;
                }

                // 2. Salva todas as respostas ligadas a esse TesteVocacional
                foreach (var r in body.Respostas)
                {
                    await _supabase.From<TesteResposta>().Insert(new TesteResposta
                    {
                        TesteId = testeId,
                        PerguntaId = r.PerguntaId,
                        Pergunta = r.Pergunta,
                        Resposta = r.Resposta
                    });
                }

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
