using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenDoors.Api.Migrations
{
    /// <summary>
    /// Regras de banco no PostgreSQL (triggers e procedures).
    ///
    /// Triggers:
    ///   trg_*_atualizado_em        — preenche atualizado_em em toda alteração (empresas, estudantes, vagas, candidaturas, matches)
    ///   trg_candidaturas_contador  — mantém vagas.candidaturas_recebidas igual ao número de candidaturas da vaga
    ///   trg_candidaturas_historico — grava em candidaturas_historico cada mudança de status de uma candidatura
    ///   trg_vagas_ativas           — mantém empresas.vagas_ativas atualizado quando vagas são criadas, alteradas ou removidas
    ///
    /// Procedures:
    ///   sp_recalcular_vagas_ativas(empresa) — recalcula empresas.vagas_ativas (usada pela trigger acima)
    ///   sp_encerrar_vagas_expiradas()       — encerra vagas abertas cuja data de expiração já passou
    /// </summary>
    public partial class AdicionaProceduresETriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------------------------------------------------------------
            // 1. atualizado_em preenchido pelo banco em toda alteração
            // ---------------------------------------------------------------
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION fn_definir_atualizado_em()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    NEW.atualizado_em := now();
                    RETURN NEW;
                END;
                $$;
                """);

            foreach (var tabela in TabelasComAtualizadoEm)
            {
                migrationBuilder.Sql($"""
                    CREATE TRIGGER trg_{tabela}_atualizado_em
                    BEFORE UPDATE ON {tabela}
                    FOR EACH ROW
                    EXECUTE FUNCTION fn_definir_atualizado_em();
                    """);
            }

            // ---------------------------------------------------------------
            // 2. vagas.candidaturas_recebidas = número de candidaturas da vaga
            // ---------------------------------------------------------------
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION fn_atualizar_contador_candidaturas()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    v_vaga_id integer := COALESCE(NEW.vaga_id, OLD.vaga_id);
                BEGIN
                    UPDATE vagas
                       SET candidaturas_recebidas = (SELECT count(*) FROM candidaturas c WHERE c.vaga_id = v_vaga_id)
                     WHERE "Id" = v_vaga_id;
                    RETURN NULL;
                END;
                $$;

                CREATE TRIGGER trg_candidaturas_contador
                AFTER INSERT OR DELETE ON candidaturas
                FOR EACH ROW
                EXECUTE FUNCTION fn_atualizar_contador_candidaturas();
                """);

            // ---------------------------------------------------------------
            // 3. Histórico: cada mudança de status de uma candidatura
            // ---------------------------------------------------------------
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION fn_registrar_historico_candidatura()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    IF TG_OP = 'INSERT' OR NEW.status IS DISTINCT FROM OLD.status THEN
                        INSERT INTO candidaturas_historico (candidatura_id, status, observacao, criado_em)
                        VALUES (
                            NEW."Id",
                            COALESCE(NEW.status, 'pendente'),
                            CASE WHEN TG_OP = 'INSERT'
                                 THEN 'Candidatura criada'
                                 ELSE 'Status alterado de ' || COALESCE(OLD.status, '(vazio)') || ' para ' || COALESCE(NEW.status, '(vazio)')
                            END,
                            now()
                        );
                    END IF;
                    RETURN NULL;
                END;
                $$;

                CREATE TRIGGER trg_candidaturas_historico
                AFTER INSERT OR UPDATE OF status ON candidaturas
                FOR EACH ROW
                EXECUTE FUNCTION fn_registrar_historico_candidatura();
                """);

            // ---------------------------------------------------------------
            // 4. Procedure: recalcula as vagas ativas (abertas) de uma empresa
            // ---------------------------------------------------------------
            migrationBuilder.Sql("""
                CREATE OR REPLACE PROCEDURE sp_recalcular_vagas_ativas(p_empresa_id uuid)
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    UPDATE empresas
                       SET vagas_ativas = (
                               SELECT count(*) FROM vagas v
                                WHERE v.empresa_id = p_empresa_id
                                  AND v.status = 'aberta')
                     WHERE "Id" = p_empresa_id;
                END;
                $$;

                CREATE OR REPLACE FUNCTION fn_vagas_ativas_da_empresa()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    IF TG_OP IN ('UPDATE', 'DELETE') THEN
                        CALL sp_recalcular_vagas_ativas(OLD.empresa_id);
                    END IF;
                    IF TG_OP IN ('INSERT', 'UPDATE') AND (TG_OP = 'INSERT' OR NEW.empresa_id IS DISTINCT FROM OLD.empresa_id OR NEW.status IS DISTINCT FROM OLD.status) THEN
                        CALL sp_recalcular_vagas_ativas(NEW.empresa_id);
                    END IF;
                    RETURN NULL;
                END;
                $$;

                CREATE TRIGGER trg_vagas_ativas
                AFTER INSERT OR UPDATE OF status, empresa_id OR DELETE ON vagas
                FOR EACH ROW
                EXECUTE FUNCTION fn_vagas_ativas_da_empresa();
                """);

            // ---------------------------------------------------------------
            // 5. Procedure: encerra vagas abertas que já expiraram
            //    Uso: CALL sp_encerrar_vagas_expiradas();
            // ---------------------------------------------------------------
            migrationBuilder.Sql("""
                CREATE OR REPLACE PROCEDURE sp_encerrar_vagas_expiradas()
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    UPDATE vagas
                       SET status = 'encerrada'
                     WHERE status = 'aberta'
                       AND expira_em IS NOT NULL
                       AND expira_em < now();
                END;
                $$;
                """);

            // Acerta os contadores das linhas que já existiam antes das triggers
            migrationBuilder.Sql("""
                UPDATE vagas v
                   SET candidaturas_recebidas = (SELECT count(*) FROM candidaturas c WHERE c.vaga_id = v."Id");

                UPDATE empresas e
                   SET vagas_ativas = (SELECT count(*) FROM vagas v WHERE v.empresa_id = e."Id" AND v.status = 'aberta');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS trg_vagas_ativas ON vagas;
                DROP TRIGGER IF EXISTS trg_candidaturas_historico ON candidaturas;
                DROP TRIGGER IF EXISTS trg_candidaturas_contador ON candidaturas;
                DROP FUNCTION IF EXISTS fn_vagas_ativas_da_empresa();
                DROP FUNCTION IF EXISTS fn_registrar_historico_candidatura();
                DROP FUNCTION IF EXISTS fn_atualizar_contador_candidaturas();
                DROP PROCEDURE IF EXISTS sp_encerrar_vagas_expiradas();
                DROP PROCEDURE IF EXISTS sp_recalcular_vagas_ativas(uuid);
                """);

            foreach (var tabela in TabelasComAtualizadoEm)
                migrationBuilder.Sql($"DROP TRIGGER IF EXISTS trg_{tabela}_atualizado_em ON {tabela};");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_definir_atualizado_em();");
        }

        private static readonly string[] TabelasComAtualizadoEm =
            { "empresas", "estudantes", "vagas", "candidaturas", "matches" };
    }
}
