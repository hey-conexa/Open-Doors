using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenDoors.Api.Infrastructure.Database;
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

// CORS: a API só responde ao frontend publicado no CloudFront.
// (app.UseCors("PermitirFrontend") é chamado lá embaixo, depois do builder.Build())
const string OrigemFrontend = "https://dq9hh10hw71hk.cloudfront.net";

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins(OrigemFrontend)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ============================================
// CONFIGURAÇÃO DO ENTITY FRAMEWORK CORE (PostgreSQL)
// ============================================

// Registra o AppDbContext com PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("Connection string 'DefaultConnection' não foi encontrada! Verifique appsettings.json e appsettings.Development.json");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// ============================================
// AUTENTICAÇÃO JWT (token próprio, gerado pelo JwtService)
// ============================================

// Mesma chave usada pelo JwtService para assinar os tokens (HS256)
var jwtSecret = builder.Configuration["JWT:Secret"];
if (string.IsNullOrEmpty(jwtSecret) || Encoding.ASCII.GetByteCount(jwtSecret) < 32)
{
    throw new Exception("JWT:Secret ausente ou curta demais (mínimo 32 caracteres). Configure em appsettings.Development.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = OpenDoors.Api.Services.JwtService.Issuer,
            ValidateAudience = true,
            ValidAudience = OpenDoors.Api.Services.JwtService.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
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

builder.Services.AddScoped<OpenDoors.Api.Services.JwtService>();
builder.Services.AddScoped<OpenDoors.Api.Services.LoginService>();
builder.Services.AddSingleton<OpenDoors.Api.Services.GoogleAuthService>();
builder.Services.AddScoped<OpenDoors.Api.Services.EmailService>();

// ============================================
// CONSTRUÇÃO E EXECUÇÃO DO APP
// ============================================

var app = builder.Build();

// ============================================
// MIGRATIONS AUTOMÁTICAS
// ============================================
// Ao iniciar, aplica as migrations pendentes (tabelas, triggers e procedures).
// Se o banco indicado na connection string ainda não existir, o EF Core cria antes.
// Assim o banco do RDS (privado, sem acesso de fora da AWS) é montado pela própria API.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var pendentes = db.Database.GetPendingMigrations().ToList();
        if (pendentes.Count > 0)
        {
            logger.LogInformation("Aplicando {Quantidade} migration(s): {Migrations}", pendentes.Count, string.Join(", ", pendentes));
            db.Database.Migrate();
            logger.LogInformation("Migrations aplicadas com sucesso.");
        }
        else
        {
            logger.LogInformation("Banco de dados já está atualizado.");
        }
    }
    catch (Exception ex)
    {
        // Não derruba a API: o erro fica no log (web.stdout.log no Beanstalk) para diagnóstico
        logger.LogError(ex, "Falha ao aplicar as migrations no banco de dados.");
    }
}

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
    //app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
