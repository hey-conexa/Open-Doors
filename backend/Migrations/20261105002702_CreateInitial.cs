using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OpenDoors.Api.Migrations
{
    /// <inheritdoc />
    public partial class CreateInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "empresas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    razao_social = table.Column<string>(type: "text", nullable: false),
                    nome_fantasia = table.Column<string>(type: "text", nullable: true),
                    cnpj = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: true),
                    site = table.Column<string>(type: "text", nullable: true),
                    logo_url = table.Column<string>(type: "text", nullable: true),
                    cidade = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: true),
                    cep = table.Column<string>(type: "text", nullable: true),
                    setor = table.Column<string>(type: "text", nullable: true),
                    porte = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    responsavel_nome = table.Column<string>(type: "text", nullable: true),
                    responsavel_cargo = table.Column<string>(type: "text", nullable: true),
                    responsavel_email = table.Column<string>(type: "text", nullable: true),
                    vagas_ativas = table.Column<int>(type: "integer", nullable: true),
                    total_contratacoes = table.Column<int>(type: "integer", nullable: true),
                    criado_por_admin = table.Column<bool>(type: "boolean", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "estudantes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: true),
                    cpf = table.Column<string>(type: "text", nullable: true),
                    data_nascimento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cidade = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: true),
                    foto_perfil_url = table.Column<string>(type: "text", nullable: true),
                    instituicao = table.Column<string>(type: "text", nullable: true),
                    curso = table.Column<string>(type: "text", nullable: true),
                    semestre = table.Column<int>(type: "integer", nullable: true),
                    turno = table.Column<string>(type: "text", nullable: true),
                    previsao_conclusao = table.Column<string>(type: "text", nullable: true),
                    curriculo_url = table.Column<string>(type: "text", nullable: true),
                    habilidades_extraidas = table.Column<List<string>>(type: "text[]", nullable: true),
                    tem_curriculo = table.Column<bool>(type: "boolean", nullable: true),
                    tem_teste_vocacional = table.Column<bool>(type: "boolean", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estudantes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "logins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    conta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    senha = table.Column<string>(type: "text", nullable: false),
                    tipo_usuario = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "perguntas_teste",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pergunta = table.Column<string>(type: "text", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    ativa = table.Column<bool>(type: "boolean", nullable: false),
                    categoria = table.Column<string>(type: "text", nullable: false),
                    mes_referencia = table.Column<string>(type: "text", nullable: true),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_perguntas_teste", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vagas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    area = table.Column<string>(type: "text", nullable: true),
                    nivel = table.Column<string>(type: "text", nullable: true),
                    cursos_aceitos = table.Column<List<string>>(type: "text[]", nullable: true),
                    semestre_minimo = table.Column<int>(type: "integer", nullable: true),
                    habilidades_requeridas = table.Column<List<string>>(type: "text[]", nullable: true),
                    habilidades_diferenciais = table.Column<List<string>>(type: "text[]", nullable: true),
                    carga_horaria = table.Column<string>(type: "text", nullable: true),
                    modalidade = table.Column<string>(type: "text", nullable: true),
                    cidade = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: true),
                    bolsa = table.Column<decimal>(type: "numeric", nullable: true),
                    beneficios = table.Column<List<string>>(type: "text[]", nullable: true),
                    vagas_disponiveis = table.Column<int>(type: "integer", nullable: true),
                    candidaturas_recebidas = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    expira_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vagas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vagas_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notificacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    destinatario_tipo = table.Column<string>(type: "text", nullable: false),
                    estudante_id = table.Column<Guid>(type: "uuid", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    mensagem = table.Column<string>(type: "text", nullable: false),
                    referencia_tabela = table.Column<string>(type: "text", nullable: true),
                    referencia_id = table.Column<int>(type: "integer", nullable: true),
                    lida = table.Column<bool>(type: "boolean", nullable: true),
                    data_leitura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notificacoes_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notificacoes_estudantes_estudante_id",
                        column: x => x.estudante_id,
                        principalTable: "estudantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "testes_vocacionais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estudante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    perfil_dominante = table.Column<string>(type: "text", nullable: true),
                    areas_sugeridas = table.Column<List<string>>(type: "text[]", nullable: true),
                    pontos_fortes = table.Column<List<string>>(type: "text[]", nullable: true),
                    descricao_perfil = table.Column<string>(type: "text", nullable: true),
                    analisado_ia = table.Column<bool>(type: "boolean", nullable: true),
                    concluido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_testes_vocacionais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_testes_vocacionais_estudantes_estudante_id",
                        column: x => x.estudante_id,
                        principalTable: "estudantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "candidaturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estudante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vaga_id = table.Column<int>(type: "integer", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: true),
                    carta_apresentacao = table.Column<string>(type: "text", nullable: true),
                    score_compatibilidade = table.Column<decimal>(type: "numeric", nullable: true),
                    posicao_ranking = table.Column<int>(type: "integer", nullable: true),
                    visualizado_empresa = table.Column<bool>(type: "boolean", nullable: true),
                    data_visualizacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidaturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_candidaturas_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_candidaturas_estudantes_estudante_id",
                        column: x => x.estudante_id,
                        principalTable: "estudantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_candidaturas_vagas_vaga_id",
                        column: x => x.vaga_id,
                        principalTable: "vagas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estudante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vaga_id = table.Column<int>(type: "integer", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score_total = table.Column<decimal>(type: "numeric", nullable: false),
                    score_curriculo = table.Column<decimal>(type: "numeric", nullable: true),
                    score_vocacional = table.Column<decimal>(type: "numeric", nullable: true),
                    score_habilidades = table.Column<decimal>(type: "numeric", nullable: true),
                    pontos_fortes = table.Column<List<string>>(type: "text[]", nullable: true),
                    pontos_fracos = table.Column<List<string>>(type: "text[]", nullable: true),
                    justificativa = table.Column<string>(type: "text", nullable: true),
                    gerado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone  ", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_matches_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_matches_estudantes_estudante_id",
                        column: x => x.estudante_id,
                        principalTable: "estudantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_matches_vagas_vaga_id",
                        column: x => x.vaga_id,
                        principalTable: "vagas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "testes_respostas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    teste_id = table.Column<int>(type: "integer", nullable: false),
                    pergunta_id = table.Column<int>(type: "integer", nullable: false),
                    pergunta = table.Column<string>(type: "text", nullable: false),
                    resposta = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_testes_respostas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_testes_respostas_perguntas_teste_pergunta_id",
                        column: x => x.pergunta_id,
                        principalTable: "perguntas_teste",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_testes_respostas_testes_vocacionais_teste_id",
                        column: x => x.teste_id,
                        principalTable: "testes_vocacionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "candidaturas_historico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    candidatura_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidaturas_historico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_candidaturas_historico_candidaturas_candidatura_id",
                        column: x => x.candidatura_id,
                        principalTable: "candidaturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_candidaturas_empresa_id",
                table: "candidaturas",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "IX_candidaturas_estudante_id",
                table: "candidaturas",
                column: "estudante_id");

            migrationBuilder.CreateIndex(
                name: "IX_candidaturas_vaga_id",
                table: "candidaturas",
                column: "vaga_id");

            migrationBuilder.CreateIndex(
                name: "IX_candidaturas_historico_candidatura_id",
                table: "candidaturas_historico",
                column: "candidatura_id");

            migrationBuilder.CreateIndex(
                name: "IX_matches_empresa_id",
                table: "matches",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "IX_matches_estudante_id",
                table: "matches",
                column: "estudante_id");

            migrationBuilder.CreateIndex(
                name: "IX_matches_vaga_id",
                table: "matches",
                column: "vaga_id");

            migrationBuilder.CreateIndex(
                name: "IX_notificacoes_empresa_id",
                table: "notificacoes",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "IX_notificacoes_estudante_id",
                table: "notificacoes",
                column: "estudante_id");

            migrationBuilder.CreateIndex(
                name: "IX_testes_respostas_pergunta_id",
                table: "testes_respostas",
                column: "pergunta_id");

            migrationBuilder.CreateIndex(
                name: "IX_testes_respostas_teste_id",
                table: "testes_respostas",
                column: "teste_id");

            migrationBuilder.CreateIndex(
                name: "IX_testes_vocacionais_estudante_id",
                table: "testes_vocacionais",
                column: "estudante_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vagas_empresa_id",
                table: "vagas",
                column: "empresa_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "candidaturas_historico");

            migrationBuilder.DropTable(
                name: "logins");

            migrationBuilder.DropTable(
                name: "matches");

            migrationBuilder.DropTable(
                name: "notificacoes");

            migrationBuilder.DropTable(
                name: "testes_respostas");

            migrationBuilder.DropTable(
                name: "candidaturas");

            migrationBuilder.DropTable(
                name: "perguntas_teste");

            migrationBuilder.DropTable(
                name: "testes_vocacionais");

            migrationBuilder.DropTable(
                name: "vagas");

            migrationBuilder.DropTable(
                name: "estudantes");

            migrationBuilder.DropTable(
                name: "empresas");
        }
    }
}
