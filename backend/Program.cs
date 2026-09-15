using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// CONFIGURAÇÕES DOS SERVIÇOS
// ============================================

// Adiciona suporte a Controllers (as classes que vão receber as requisições HTTP)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Suporte ao Swagger (documentação automática da API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Habilita CORS para o frontend conseguir acessar essa API depois
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrWhiteSpace(origin))
                return false;

            return origin.Equals("https://opendoorsconexa.netlify.app", StringComparison.OrdinalIgnoreCase)
                || origin.StartsWith("http://localhost:", StringComparison.OrdinalIgnoreCase)
                || origin.StartsWith("http://127.0.0.1:", StringComparison.OrdinalIgnoreCase);
        })
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============================================
// CONFIGURAÇÃO DO SUPABASE
// ============================================

// Lê as configurações do appsettings.json + appsettings.Development.json
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];

// Valida se as configurações foram carregadas
if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
{
    throw new Exception("Configurações do Supabase não foram encontradas! Verifique appsettings.json e appsettings.Development.json");
}

// Cria a instância do cliente Supabase e registra como Singleton
// (Singleton = uma única instância compartilhada na aplicação inteira, padrão de injeção de dependência)
builder.Services.AddSingleton<Supabase.Client>(_ =>
{
    var options = new Supabase.SupabaseOptions
    {
        AutoConnectRealtime = true
    };
    var client = new Supabase.Client(supabaseUrl, supabaseKey, options);
    client.InitializeAsync().Wait();
    return client;
});

// ============================================
// AUTENTICAÇÃO JWT (Supabase)
// ============================================

// Busca as chaves públicas do Supabase (ES256) e faz cache em memória
var jwksUri = $"{supabaseUrl}/auth/v1/.well-known/jwks.json";
IList<SecurityKey>? cachedJwksKeys = null;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKeyResolver = (_, _, _, _) =>
            {
                if (cachedJwksKeys == null)
                {
                    using var http = new System.Net.Http.HttpClient();
                    var jwksJson = http.GetStringAsync(jwksUri).Result;
                    cachedJwksKeys = new JsonWebKeySet(jwksJson).GetSigningKeys();
                }
                return cachedJwksKeys;
            }
        };
    });

// ============================================
// CONFIGURAÇÃO DA IA (Groq + Services)
// ============================================

// Registro correto do GroqService — permite injeção automática em outros services
builder.Services.AddHttpClient<OpenDoors.Api.Services.GroqService>();
builder.Services.AddScoped<OpenDoors.Api.Services.GroqService>();

builder.Services.AddScoped<OpenDoors.Api.Services.AnalisarCurriculoService>();
builder.Services.AddScoped<OpenDoors.Api.Services.AnalisarTesteService>();
builder.Services.AddScoped<OpenDoors.Api.Services.GerarScoreService>();
builder.Services.AddScoped<OpenDoors.Api.Services.GerarPerguntasMensaisService>();

builder.Services.AddHttpClient<OpenDoors.Api.Services.JoobleService>();
builder.Services.AddScoped<OpenDoors.Api.Services.JoobleService>();

// ============================================
// CONSTRUÇÃO E EXECUÇÃO DO APP
// ============================================

var app = builder.Build();

// Em ambiente de desenvolvimento, mostra o Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PermitirFrontend");
app.UseAuthentication();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
