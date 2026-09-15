using Newtonsoft.Json;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Services
{
    /// <summary>
    /// Gera perguntas de teste vocacional mensais usando o Groq (Llama 3.3).
    /// Cada mês tem um tema diferente (liderança, criatividade, etc.).
    /// As perguntas são salvas na tabela perguntas_teste com tipo = "mensal"
    /// e mes_referencia = "YYYY-MM".
    /// </summary>
    public class GerarPerguntasMensaisService
    {
        private readonly GroqService _groq;
        private readonly Supabase.Client _supabase;

        // Temas rotativos — um por mês, na ordem do ano
        private static readonly string[] TemasPorMes = new[]
        {
            "autoconhecimento e valores pessoais",           // Janeiro
            "inteligência emocional no trabalho",            // Fevereiro
            "liderança e tomada de decisão",                 // Março
            "criatividade e resolução de problemas",         // Abril
            "trabalho em equipe e colaboração",              // Maio
            "comunicação e expressão profissional",          // Junho
            "adaptabilidade e resiliência",                  // Julho
            "ética e responsabilidade profissional",         // Agosto
            "planejamento de carreira e metas",              // Setembro
            "inovação e mentalidade empreendedora",          // Outubro
            "diversidade, inclusão e empatia",               // Novembro
            "propósito e impacto social do trabalho"         // Dezembro
        };

        public GerarPerguntasMensaisService(GroqService groq, Supabase.Client supabase)
        {
            _groq     = groq;
            _supabase = supabase;
        }

        /// <summary>
        /// Gera e salva as perguntas do mês atual.
        /// Se já existirem perguntas para o mês corrente, retorna sem duplicar.
        /// </summary>
        public async Task<List<PerguntaTeste>> GerarParaMesAtualAsync()
        {
            var mesAtual = DateTime.UtcNow.ToString("yyyy-MM");
            return await GerarParaMesAsync(mesAtual);
        }

        /// <summary>
        /// Gera e salva as perguntas para um mês específico (formato "yyyy-MM").
        /// </summary>
        public async Task<List<PerguntaTeste>> GerarParaMesAsync(string mesReferencia)
        {
            // Verifica se já foram geradas perguntas para este mês
            var existentes = await _supabase
                .From<PerguntaTeste>()
                .Where(p => p.MesReferencia == mesReferencia)
                .Get();

            if (existentes.Models.Any())
                return existentes.Models;

            // Descobre o tema do mês baseado no número do mês
            var mes   = int.Parse(mesReferencia.Split('-')[1]);
            var tema  = TemasPorMes[mes - 1];

            var systemPrompt = @"
Você é um especialista em psicologia vocacional e orientação de carreira para jovens universitários brasileiros.
Seu objetivo é gerar perguntas reflexivas de autoconhecimento profissional.
Responda APENAS com um array JSON válido, sem texto adicional, sem markdown, sem explicações.
Formato exato:
[
  { ""pergunta"": ""Texto da pergunta aqui?"", ""ordem"": 1 },
  { ""pergunta"": ""Texto da pergunta aqui?"", ""ordem"": 2 }
]
";

            var userPrompt = $@"
Gere exatamente 8 perguntas abertas e reflexivas sobre o tema: ""{tema}"".

Contexto: as perguntas serão respondidas por estudantes universitários brasileiros (18-25 anos)
que estão descobrindo seu perfil profissional. As respostas serão analisadas por IA para
identificar habilidades, valores e tendências de carreira.

Requisitos:
- Perguntas em português brasileiro, tom acolhedor e direto
- Abertas (não sim/não) — devem gerar respostas ricas e pessoais
- Específicas o suficiente para revelar características reais de personalidade
- Variadas: misture situações passadas, preferências atuais e projeções futuras
- Evite jargões corporativos ou linguagem muito formal
";

            // Chama o Groq e desserializa o JSON retornado
            var itens = await _groq.ChatJsonAsync<List<PerguntaGeradaDto>>(systemPrompt, userPrompt);

            if (itens == null || itens.Count == 0)
                throw new InvalidOperationException("O Groq não retornou perguntas válidas.");

            // Descobre a maior ordem atual para não colidir com as perguntas fixas
            var todasPerguntas = await _supabase.From<PerguntaTeste>().Get();
            var maiorOrdem     = todasPerguntas.Models.Any()
                ? todasPerguntas.Models.Max(p => p.Ordem)
                : 200;

            // Salva cada pergunta no banco
            var salvas = new List<PerguntaTeste>();
            var baseOrdem = maiorOrdem + 10;

            foreach (var item in itens)
            {
                var pergunta = new PerguntaTeste
                {
                    Pergunta       = item.Pergunta,
                    Ordem          = baseOrdem + item.Ordem,
                    Ativa          = true,
                    Categoria      = $"mensal_{tema.Replace(" ", "_")}",
                    MesReferencia  = mesReferencia,
                    Tipo           = "mensal",
                    CriadoEm       = DateTime.UtcNow
                };

                var resultado = await _supabase.From<PerguntaTeste>().Insert(pergunta);
                var criada    = resultado.Models.FirstOrDefault();
                if (criada != null)
                    salvas.Add(criada);
            }

            return salvas;
        }

        // DTO interno para deserializar a resposta do Groq
        private class PerguntaGeradaDto
        {
            [JsonProperty("pergunta")]
            public string Pergunta { get; set; } = string.Empty;

            [JsonProperty("ordem")]
            public int Ordem { get; set; }
        }
    }
}
