using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BancoDigital.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clientes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contas", x => x.id);
                    table.ForeignKey(
                        name: "fk_contas_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transferencias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_origem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_destino_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    realizada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transferencias", x => x.id);
                    table.ForeignKey(
                        name: "fk_transferencias_contas_conta_destino_id",
                        column: x => x.conta_destino_id,
                        principalTable: "contas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transferencias_contas_conta_origem_id",
                        column: x => x.conta_origem_id,
                        principalTable: "contas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_contas_cliente_id",
                table: "contas",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_contas_numero",
                table: "contas",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transferencias_conta_destino_id",
                table: "transferencias",
                column: "conta_destino_id");

            migrationBuilder.CreateIndex(
                name: "ix_transferencias_conta_origem_id",
                table: "transferencias",
                column: "conta_origem_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transferencias");

            migrationBuilder.DropTable(
                name: "contas");

            migrationBuilder.DropTable(
                name: "clientes");
        }
    }
}
