using OpenDoors.Api.DTOs;

namespace OpenDoors.Api.Services
{
    /// <summary>
    /// Analisa respostas do teste vocacional via IA.
    /// As respostas chegam como valores numéricos 1-7 (escala Likert).
    /// A persistência fica no Controller (separação de responsabilidades).
    /// </summary>
    public class AnalisarTesteService
    {
        private readonly GroqService _groq;

        public AnalisarTesteService(GroqService groq) => _groq = groq;

        public async Task<PerfilVocacionalDto> AnalisarAsync(List<RespostaVocacionalDto> respostas)
        {
            if (respostas.Count == 0)
                throw new ArgumentException("A lista de respostas não pode estar vazia.");

            // Converte o valor numérico em texto descritivo para o modelo entender melhor
            static string InterpretarLikert(string resposta) => resposta.Trim() switch
            {
                "1" => "Concordo totalmente",
                "2" => "Concordo",
                "3" => "Concordo parcialmente",
                "4" => "Neutro",
                "5" => "Discordo parcialmente",
                "6" => "Discordo",
                "7" => "Discordo totalmente",
                _   => resposta // fallback: usa o valor original se não for numérico
            };

            var respostasFormatadas = string.Join("\n", respostas.Select(r =>
                $"[{r.Categoria ?? "geral"}] {r.Pergunta}\nResposta: {InterpretarLikert(r.Resposta)}"));

            const string system =
                "Você é um especialista em psicologia vocacional e orientação de carreira para jovens universitários brasileiros. " +
                "As respostas foram coletadas via escala Likert de 7 pontos: " +
                "1 = Concordo totalmente, 2 = Concordo, 3 = Concordo parcialmente, " +
                "4 = Neutro, 5 = Discordo parcialmente, 6 = Discordo, 7 = Discordo totalmente. " +
                "As perguntas são afirmações sobre comportamentos, preferências e valores profissionais. " +
                "Respostas próximas de 1 indicam forte identificação com a afirmação; próximas de 7 indicam baixa identificação. " +
                "Use os frameworks RIASEC (Holland Code) e Big Five para embasar sua análise. " +
                "Evite viés para Tecnologia da Informação quando as respostas forem neutras ou amplas; priorize áreas diversas e coerentes com o conjunto completo das respostas. " +
                "Responda APENAS com JSON válido, sem texto adicional, sem markdown, sem ```json.";

            var user = $$"""
                Analise o perfil vocacional deste estudante com base nas respostas abaixo e retorne um JSON com:
                {
                  "perfilDominante": "nome do perfil RIASEC dominante (ex: Investigativo, Social, Artístico)",
                  "areasSugeridas": ["área profissional 1", "área profissional 2", "área profissional 3"],
                  "pontosFortes": ["ponto forte 1", "ponto forte 2", "ponto forte 3"],
                  "descricaoPerfil": "descrição personalizada do perfil em 2-3 frases, mencionando traços dominantes e como isso se reflete em escolhas profissionais"
                }

                Respostas do estudante:
                {{respostasFormatadas}}
                """;

            return await _groq.ChatJsonAsync<PerfilVocacionalDto>(system, user);
        }
    }
}
