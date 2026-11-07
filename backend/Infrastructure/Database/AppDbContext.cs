using Microsoft.EntityFrameworkCore;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Infrastructure.Database
{
    /// <summary>
    /// DbContext para Entity Framework Core
    /// Mapeia todas as entidades do OpenDoors com suas respectivas tabelas no banco
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets para cada entidade
        public DbSet<Empresa> Empresas { get; set; } = null!;
        public DbSet<Estudante> Estudantes { get; set; } = null!;
        public DbSet<Vaga> Vagas { get; set; } = null!;
        public DbSet<Candidatura> Candidaturas { get; set; } = null!;
        public DbSet<CandidaturaHistorico> CandidaturasHistorico { get; set; } = null!;
        public DbSet<Match> Matches { get; set; } = null!;
        public DbSet<Login> Logins { get; set; } = null!;
        public DbSet<CodigoRedefinicaoSenha> CodigosRedefinicaoSenha { get; set; } = null!;
        public DbSet<CurriculoArquivo> Curriculos { get; set; } = null!;
        public DbSet<Notificacao> Notificacoes { get; set; } = null!;
        public DbSet<TesteVocacional> TestesVocacionais { get; set; } = null!;
        public DbSet<PerguntaTeste> PerguntasTeste { get; set; } = null!;
        public DbSet<TesteResposta> TestesRespostas { get; set; } = null!;

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp with time zone");
            configurationBuilder.Properties<DateTime?>().HaveColumnType("timestamp with time zone");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore(typeof(DbContextOptions<>));
            modelBuilder.Ignore(typeof(DbContextOptions));

            // ============================================
            // EMPRESA
            // ============================================
            modelBuilder.Entity<Empresa>(entity =>
            {
                entity.ToTable("empresas");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .ValueGeneratedNever(); // UUID enviada pela aplicação

                entity.Property(e => e.RazaoSocial).HasColumnName("razao_social").HasColumnType("text").IsRequired();
                entity.Property(e => e.NomeFantasia).HasColumnName("nome_fantasia").HasColumnType("text");
                entity.Property(e => e.Cnpj).HasColumnName("cnpj").HasColumnType("text").IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").HasColumnType("text").IsRequired();
                entity.Property(e => e.Telefone).HasColumnName("telefone").HasColumnType("text");
                entity.Property(e => e.Site).HasColumnName("site").HasColumnType("text");
                entity.Property(e => e.LogoUrl).HasColumnName("logo_url").HasColumnType("text");
                entity.Property(e => e.Cidade).HasColumnName("cidade").HasColumnType("text");
                entity.Property(e => e.Estado).HasColumnName("estado").HasColumnType("text");
                entity.Property(e => e.Cep).HasColumnName("cep").HasColumnType("text");
                entity.Property(e => e.Setor).HasColumnName("setor").HasColumnType("text");
                entity.Property(e => e.Porte).HasColumnName("porte").HasColumnType("text");
                entity.Property(e => e.Descricao).HasColumnName("descricao").HasColumnType("text");
                entity.Property(e => e.ResponsavelNome).HasColumnName("responsavel_nome").HasColumnType("text");
                entity.Property(e => e.ResponsavelCargo).HasColumnName("responsavel_cargo").HasColumnType("text");
                entity.Property(e => e.ResponsavelEmail).HasColumnName("responsavel_email").HasColumnType("text");
                entity.Property(e => e.VagasAtivas).HasColumnName("vagas_ativas").HasColumnType("integer");
                entity.Property(e => e.TotalContratacoes).HasColumnName("total_contratacoes").HasColumnType("integer");
                entity.Property(e => e.CriadoPorAdmin).HasColumnName("criado_por_admin").HasColumnType("boolean");
                entity.Property(e => e.Status).HasColumnName("status").HasColumnType("text");
                entity.Property(e => e.CriadoEm).HasColumnName("criado_em");
                entity.Property(e => e.AtualizadoEm).HasColumnName("atualizado_em");
            });

            // ============================================
            // ESTUDANTE
            // ============================================
            modelBuilder.Entity<Estudante>(entity =>
            {
                entity.ToTable("estudantes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .ValueGeneratedNever(); // UUID enviada pela aplicação

                entity.Property(e => e.Nome).HasColumnName("nome").HasColumnType("text").IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").HasColumnType("text").IsRequired();
                entity.Property(e => e.Telefone).HasColumnName("telefone").HasColumnType("text");
                entity.Property(e => e.Cpf).HasColumnName("cpf").HasColumnType("text");
                entity.Property(e => e.DataNascimento).HasColumnName("data_nascimento");
                entity.Property(e => e.Cidade).HasColumnName("cidade").HasColumnType("text");
                entity.Property(e => e.Estado).HasColumnName("estado").HasColumnType("text");
                entity.Property(e => e.FotoPerfilUrl).HasColumnName("foto_perfil_url").HasColumnType("text");
                entity.Property(e => e.Instituicao).HasColumnName("instituicao").HasColumnType("text");
                entity.Property(e => e.Curso).HasColumnName("curso").HasColumnType("text");
                entity.Property(e => e.Semestre).HasColumnName("semestre").HasColumnType("integer");
                entity.Property(e => e.Turno).HasColumnName("turno").HasColumnType("text");
                entity.Property(e => e.PrevisaoConclusao).HasColumnName("previsao_conclusao").HasColumnType("text");
                entity.Property(e => e.CurriculoUrl).HasColumnName("curriculo_url").HasColumnType("text");
                entity.Property(e => e.HabilidadesExtraidas).HasColumnName("habilidades_extraidas").HasColumnType("text[]");
                entity.Property(e => e.TemCurriculo).HasColumnName("tem_curriculo").HasColumnType("boolean");
                entity.Property(e => e.TemTesteVocacional).HasColumnName("tem_teste_vocacional").HasColumnType("boolean");
                entity.Property(e => e.Status).HasColumnName("status").HasColumnType("text");
                entity.Property(e => e.CriadoEm).HasColumnName("criado_em");
                entity.Property(e => e.AtualizadoEm).HasColumnName("atualizado_em");
            });

            // ============================================
            // VAGA
            // ============================================
            modelBuilder.Entity<Vaga>(entity =>
            {
                entity.ToTable("vagas");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedOnAdd(); // Auto-incremento (SERIAL)

                entity.Property(e => e.EmpresaId).HasColumnName("empresa_id").HasColumnType("uuid").IsRequired();
                entity.Property(e => e.Titulo).HasColumnName("titulo").HasColumnType("text").IsRequired();
                entity.Property(e => e.Descricao).HasColumnName("descricao").HasColumnType("text");
                entity.Property(e => e.Area).HasColumnName("area").HasColumnType("text");
                entity.Property(e => e.Nivel).HasColumnName("nivel").HasColumnType("text");
                entity.Property(e => e.CursosAceitos).HasColumnName("cursos_aceitos").HasColumnType("text[]");
                entity.Property(e => e.SemestreMinimo).HasColumnName("semestre_minimo").HasColumnType("integer");
                entity.Property(e => e.HabilidadesRequeridas).HasColumnName("habilidades_requeridas").HasColumnType("text[]");
                entity.Property(e => e.HabilidadesDiferenciais).HasColumnName("habilidades_diferenciais").HasColumnType("text[]");
                entity.Property(e => e.CargaHoraria).HasColumnName("carga_horaria").HasColumnType("text");
                entity.Property(e => e.Modalidade).HasColumnName("modalidade").HasColumnType("text");
                entity.Property(e => e.Cidade).HasColumnName("cidade").HasColumnType("text");
                entity.Property(e => e.Estado).HasColumnName("estado").HasColumnType("text");
                entity.Property(e => e.Bolsa).HasColumnName("bolsa").HasColumnType("numeric");
                entity.Property(e => e.Beneficios).HasColumnName("beneficios").HasColumnType("text[]");
                entity.Property(e => e.VagasDisponiveis).HasColumnName("vagas_disponiveis").HasColumnType("integer");
                entity.Property(e => e.CandidaturasRecebidas).HasColumnName("candidaturas_recebidas").HasColumnType("integer");
                entity.Property(e => e.Status).HasColumnName("status").HasColumnType("text");
                entity.Property(e => e.CriadoEm).HasColumnName("criado_em");
                entity.Property(e => e.ExpiraEm).HasColumnName("expira_em");
                entity.Property(e => e.AtualizadoEm).HasColumnName("atualizado_em");

                // FK para Empresa
                entity.HasOne<Empresa>()
                    .WithMany()
                    .HasForeignKey(e => e.EmpresaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================
            // CANDIDATURA
            // ============================================
            modelBuilder.Entity<Candidatura>(entity =>
            {
                entity.ToTable("candidaturas");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedOnAdd(); // Auto-incremento

                entity.Property(e => e.EstudanteId).HasColumnName("estudante_id").HasColumnType("uuid").IsRequired();
                entity.Property(e => e.VagaId).HasColumnName("vaga_id").HasColumnType("integer").IsRequired();
                entity.Property(e => e.EmpresaId).HasColumnName("empresa_id").HasColumnType("uuid").IsRequired();
                entity.Property(e => e.Status).HasColumnName("status").HasColumnType("text");
                entity.Property(e => e.CartaApresentacao).HasColumnName("carta_apresentacao").HasColumnType("text");
                entity.Property(e => e.ScoreCompatibilidade).HasColumnName("score_compatibilidade").HasColumnType("numeric");
                entity.Property(e => e.PosicaoRanking).HasColumnName("posicao_ranking").HasColumnType("integer");
                entity.Property(e => e.VisualizadoEmpresa).HasColumnName("visualizado_empresa").HasColumnType("boolean");
                entity.Property(e => e.DataVisualizacao).HasColumnName("data_visualizacao");
                entity.Property(e => e.CriadoEm).HasColumnName("criado_em");
                entity.Property(e => e.AtualizadoEm).HasColumnName("atualizado_em");

                // FKs
                entity.HasOne<Estudante>()
                    .WithMany()
                    .HasForeignKey(e => e.EstudanteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Vaga>()
                    .WithMany()
                    .HasForeignKey(e => e.VagaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Empresa>()
                    .WithMany()
                    .HasForeignKey(e => e.EmpresaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================
            // CANDIDATURA_HISTORICO
            // ============================================
            modelBuilder.Entity<CandidaturaHistorico>(entity =>
            {
                entity.ToTable("candidaturas_historico");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedOnAdd(); // Auto-incremento

                entity.Property(e => e.CandidaturaId).HasColumnName("candidatura_id").HasColumnType("integer").IsRequired();
                entity.Property(e => e.Status).HasColumnName("status").HasColumnType("text").IsRequired();
                entity.Property(e => e.Observacao).HasColumnName("observacao").HasColumnType("text");
                entity.Property(e => e.CriadoEm).HasColumnName("criado_em");

                // FK para Candidatura
                entity.HasOne<Candidatura>()
                    .WithMany()
                    .HasForeignKey(e => e.CandidaturaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================================
            // MATCH
            // ============================================
            modelBuilder.Entity<Match>(entity =>
            {
                entity.ToTable("matches");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedOnAdd(); // Auto-incremento

                entity.Property(e => e.EstudanteId).HasColumnName("estudante_id").HasColumnType("uuid").IsRequired();
                entity.Property(e => e.VagaId).HasColumnName("vaga_id").HasColumnType("integer").IsRequired();
                entity.Property(e => e.EmpresaId).HasColumnName("empresa_id").HasColumnType("uuid").IsRequired();
                entity.Property(e => e.ScoreTotal).HasColumnName("score_total").HasColumnType("numeric").IsRequired();
                entity.Property(e => e.ScoreCurriculo).HasColumnName("score_curriculo").HasColumnType("numeric");
                entity.Property(e => e.ScoreVocacional).HasColumnName("score_vocacional").HasColumnType("numeric");
                entity.Property(e => e.ScoreHabilidades).HasColumnName("score_habilidades").HasColumnType("numeric");
                entity.Property(e => e.PontosFortes).HasColumnName("pontos_fortes").HasColumnType("text[]");
                entity.Property(e => e.PontosFracos).HasColumnName("pontos_fracos").HasColumnType("text[]");
                entity.Property(e => e.Justificativa).HasColumnName("justificativa").HasColumnType("text");
                entity.Property(e => e.GeradoEm).HasColumnName("gerado_em");
                entity.Property(e => e.AtualizadoEm).HasColumnName("atualizado_em");

                // FKs
                entity.HasOne<Estudante>()
                    .WithMany()
                    .HasForeignKey(e => e.EstudanteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Vaga>()
                    .WithMany()
                    .HasForeignKey(e => e.VagaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Empresa>()
                    .WithMany()
                    .HasForeignKey(e => e.EmpresaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================
            // LOGIN
            // ============================================
            modelBuilder.Entity<Login>(entity =>
            {
                entity.ToTable("logins");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // Auto-incremento

                entity.Property(e => e.ContaId).HasColumnName("conta_id").IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").IsRequired();
                entity.Property(e => e.Senha).HasColumnName("senha").IsRequired();
                entity.Property(e => e.TipoUsuario).HasColumnName("tipo_usuario").IsRequired();
            });

            // ============================================
            // CODIGO DE REDEFINICAO DE SENHA
            // ============================================
            modelBuilder.Entity<CodigoRedefinicaoSenha>(entity =>
            {
                entity.ToTable("codigos_redefinicao_senha");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // Auto-incremento

                entity.Property(e => e.ContaId).HasColumnName("conta_id").IsRequired();
                entity.Property(e => e.CodigoHash).HasColumnName("codigo_hash").IsRequired();
                entity.Property(e => e.CriadoEm).HasColumnName("criado_em").IsRequired();
                entity.Property(e => e.ExpiraEm).HasColumnName("expira_em").IsRequired();
                entity.Property(e => e.Tentativas).HasColumnName("tentativas").IsRequired();
                entity.Property(e => e.Usado).HasColumnName("usado").IsRequired();

                entity.HasIndex(e => e.ContaId);
            });

            // ============================================
            // CURRICULO (PDF guardado no banco)
            // ============================================
            modelBuilder.Entity<CurriculoArquivo>(entity =>
            {
                entity.ToTable("curriculos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // Auto-incremento

                entity.Property(e => e.EstudanteId).HasColumnName("estudante_id").IsRequired();
                entity.Property(e => e.NomeArquivo).HasColumnName("nome_arquivo").IsRequired();
                entity.Property(e => e.ContentType).HasColumnName("content_type").IsRequired();
                entity.Property(e => e.Conteudo).HasColumnName("conteudo").HasColumnType("bytea").IsRequired();
                entity.Property(e => e.Texto).HasColumnName("texto").IsRequired();
                entity.Property(e => e.EnviadoEm).HasColumnName("enviado_em").IsRequired();

                entity.HasIndex(e => e.EstudanteId).IsUnique();
                entity.HasOne<Estudante>().WithMany().HasForeignKey(e => e.EstudanteId).OnDelete(DeleteBehavior.Cascade);
            });

            // ============================================
            // NOTIFICACAO
            // ============================================
            modelBuilder.Entity<Notificacao>(entity =>
            {
                entity.ToTable("notificacoes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // Auto-incremento

                entity.Property(e => e.DestinatarioTipo).HasColumnName("destinatario_tipo").IsRequired();
                entity.Property(e => e.EstudanteId).HasColumnName("estudante_id");
                entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
                entity.Property(e => e.Tipo).HasColumnName("tipo").IsRequired();
                entity.Property(e => e.Titulo).HasColumnName("titulo").IsRequired();
                entity.Property(e => e.Mensagem).HasColumnName("mensagem").IsRequired();
                entity.Property(e => e.ReferenciaTabela).HasColumnName("referencia_tabela");
                entity.Property(e => e.ReferenciaId).HasColumnName("referencia_id");
                entity.Property(e => e.Lida).HasColumnName("lida");
                entity.Property(e => e.DataLeitura).HasColumnName("data_leitura");
                entity.Property(e => e.CriadoEm).HasColumnName("criado_em");

                // FKs opcionais (mutuamente exclusivas)
                entity.HasOne<Estudante>()
                    .WithMany()
                    .HasForeignKey(e => e.EstudanteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Empresa>()
                    .WithMany()
                    .HasForeignKey(e => e.EmpresaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================
            // TESTE_VOCACIONAL
            // ============================================
            modelBuilder.Entity<TesteVocacional>(entity =>
            {
                entity.ToTable("testes_vocacionais");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // Auto-incremento

                entity.Property(e => e.EstudanteId).HasColumnName("estudante_id").IsRequired();
                entity.Property(e => e.PerfilDominante).HasColumnName("perfil_dominante");
                entity.Property(e => e.AreasSugeridas).HasColumnName("areas_sugeridas");
                entity.Property(e => e.PontosFortes).HasColumnName("pontos_fortes");
                entity.Property(e => e.DescricaoPerfil).HasColumnName("descricao_perfil");
                entity.Property(e => e.AnalisadoIa).HasColumnName("analisado_ia");
                entity.Property(e => e.ConcluidoEm).HasColumnName("concluido_em");

                // UNIQUE constraint para estudante_id
                entity.HasIndex(e => e.EstudanteId).IsUnique();

                // FK para Estudante
                entity.HasOne<Estudante>()
                    .WithMany()
                    .HasForeignKey(e => e.EstudanteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================================
            // PERGUNTA_TESTE
            // ============================================
            modelBuilder.Entity<PerguntaTeste>(entity =>
            {
                entity.ToTable("perguntas_teste");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // Auto-incremento

                entity.Property(e => e.Pergunta).HasColumnName("pergunta").IsRequired();
                entity.Property(e => e.Ordem).HasColumnName("ordem").IsRequired();
                entity.Property(e => e.Ativa).HasColumnName("ativa").IsRequired();
                entity.Property(e => e.Categoria).HasColumnName("categoria").IsRequired();
                entity.Property(e => e.MesReferencia).HasColumnName("mes_referencia");
                entity.Property(e => e.Tipo).HasColumnName("tipo").IsRequired();
                entity.Property(e => e.CriadoEm).HasColumnName("criado_em");
            });

            // ============================================
            // TESTE_RESPOSTA
            // ============================================
            modelBuilder.Entity<TesteResposta>(entity =>
            {
                entity.ToTable("testes_respostas");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // Auto-incremento

                entity.Property(e => e.TesteId).HasColumnName("teste_id").IsRequired();
                entity.Property(e => e.PerguntaId).HasColumnName("pergunta_id").IsRequired();
                entity.Property(e => e.Pergunta).HasColumnName("pergunta").IsRequired();
                entity.Property(e => e.Resposta).HasColumnName("resposta").IsRequired();
                entity.Property(e => e.CriadoEm).HasColumnName("criado_em");
                entity.Property(e => e.AtualizadoEm).HasColumnName("atualizado_em");

                // FKs
                entity.HasOne<TesteVocacional>()
                    .WithMany()
                    .HasForeignKey(e => e.TesteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<PerguntaTeste>()
                    .WithMany()
                    .HasForeignKey(e => e.PerguntaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}