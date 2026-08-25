using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DominoPontaDeQuina.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RenomearJogoParaPartida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParticipacoesJogo");

            migrationBuilder.DropTable(
                name: "Jogos");

            migrationBuilder.CreateTable(
                name: "Partidas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IniciadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FinalizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partidas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParticipacoesPartida",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PartidaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JogadorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Posicao = table.Column<int>(type: "INTEGER", nullable: false),
                    Pontuacao = table.Column<int>(type: "INTEGER", nullable: false),
                    Vencedor = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipacoesPartida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipacoesPartida_Jogadores_JogadorId",
                        column: x => x.JogadorId,
                        principalTable: "Jogadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParticipacoesPartida_Partidas_PartidaId",
                        column: x => x.PartidaId,
                        principalTable: "Partidas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacoesPartida_JogadorId",
                table: "ParticipacoesPartida",
                column: "JogadorId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacoesPartida_PartidaId",
                table: "ParticipacoesPartida",
                column: "PartidaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParticipacoesPartida");

            migrationBuilder.DropTable(
                name: "Partidas");

            migrationBuilder.CreateTable(
                name: "Jogos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FinalizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IniciadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParticipacoesJogo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JogadorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JogoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Pontuacao = table.Column<int>(type: "INTEGER", nullable: false),
                    Posicao = table.Column<int>(type: "INTEGER", nullable: false),
                    Vencedor = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipacoesJogo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipacoesJogo_Jogadores_JogadorId",
                        column: x => x.JogadorId,
                        principalTable: "Jogadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParticipacoesJogo_Jogos_JogoId",
                        column: x => x.JogoId,
                        principalTable: "Jogos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacoesJogo_JogadorId",
                table: "ParticipacoesJogo",
                column: "JogadorId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacoesJogo_JogoId",
                table: "ParticipacoesJogo",
                column: "JogoId");
        }
    }
}
