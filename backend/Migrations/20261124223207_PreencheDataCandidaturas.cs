using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenDoors.Api.Migrations
{
    /// <summary>
    /// Preenche candidaturas.criado_em nas candidaturas antigas.
    ///
    /// Até esta versão o POST /api/candidaturas não gravava a data e a coluna não
    /// tem valor padrão, então as candidaturas ficaram com criado_em vazio e sumiam
    /// dos filtros de 7, 30 e 90 dias do painel da empresa. O CandidaturaController
    /// agora grava a data; esta migration corrige as linhas que já existiam.
    ///
    /// Usa atualizado_em quando existe (a candidatura teve o status alterado) e,
    /// sem ele, a data em que a migration roda — a data real de envio não foi
    /// gravada e não há como recuperá-la. Gravado em UTC, como a API grava.
    /// </summary>
    public partial class PreencheDataCandidaturas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE candidaturas
                   SET criado_em = COALESCE(atualizado_em, now() AT TIME ZONE 'UTC')
                 WHERE criado_em IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sem volta: depois da migration não dá para saber quais datas foram
            // preenchidas por ela e quais a API gravou — e voltar a deixá-las vazias
            // só traria o problema de volta.
        }
    }
}
