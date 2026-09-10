using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Refeicao.Core.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FUNCIONARIO",
                columns: table => new
                {
                    ID_FUNCIONARIO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    NM_USUARIO = table.Column<string>(type: "longtext", nullable: false),
                    HASH_SENHA = table.Column<string>(type: "longtext", nullable: false),
                    SN_PRIMEIRO_ACESSO = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DT_CADASTRO = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DT_INATIVACAO = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FUNCIONARIO", x => x.ID_FUNCIONARIO);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    ID_USUARIO = table.Column<string>(type: "varchar(255)", nullable: false),
                    NM_USUARIO = table.Column<string>(type: "longtext", nullable: false),
                    HASH_SENHA = table.Column<string>(type: "longtext", nullable: false),
                    SN_PRIMEIRO_ACESSO = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DT_INATIVACAO = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DT_CADASTRO = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIO", x => x.ID_USUARIO);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ACOMPANHAMENTO",
                columns: table => new
                {
                    ID_ACOMPANHAMENTO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    DS_ACOMPANHAMENTO = table.Column<string>(type: "longtext", nullable: false),
                    ID_USUARIO = table.Column<string>(type: "varchar(255)", nullable: false),
                    ID_USUARIO_DELETOU = table.Column<string>(type: "varchar(255)", nullable: true),
                    SN_DELETADO = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACOMPANHAMENTO", x => x.ID_ACOMPANHAMENTO);
                    table.ForeignKey(
                        name: "FK_ACOMPANHAMENTO_USUARIO_ID_USUARIO",
                        column: x => x.ID_USUARIO,
                        principalTable: "USUARIO",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ACOMPANHAMENTO_USUARIO_ID_USUARIO_DELETOU",
                        column: x => x.ID_USUARIO_DELETOU,
                        principalTable: "USUARIO",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "REFEICAO",
                columns: table => new
                {
                    ID_REFEICAO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    DS_REFEICAO = table.Column<string>(type: "longtext", nullable: false),
                    ID_USUARIO = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REFEICAO", x => x.ID_REFEICAO);
                    table.ForeignKey(
                        name: "FK_REFEICAO_USUARIO_ID_USUARIO",
                        column: x => x.ID_USUARIO,
                        principalTable: "USUARIO",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ACOMPANHAMENTO_REFEICAO",
                columns: table => new
                {
                    ID_ACOMPANHAMENTO = table.Column<int>(type: "int", nullable: false),
                    ID_REFEICAO = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACOMPANHAMENTO_REFEICAO", x => new { x.ID_ACOMPANHAMENTO, x.ID_REFEICAO });
                    table.ForeignKey(
                        name: "FK_ACOMPANHAMENTO_REFEICAO_ACOMPANHAMENTO_ID_ACOMPANHAMENTO",
                        column: x => x.ID_ACOMPANHAMENTO,
                        principalTable: "ACOMPANHAMENTO",
                        principalColumn: "ID_ACOMPANHAMENTO",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ACOMPANHAMENTO_REFEICAO_REFEICAO_ID_REFEICAO",
                        column: x => x.ID_REFEICAO,
                        principalTable: "REFEICAO",
                        principalColumn: "ID_REFEICAO",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CARDAPIO",
                columns: table => new
                {
                    ID_CARDAPIO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    TP_CARDAPIO = table.Column<string>(type: "char(1)", nullable: false),
                    DT_CARDAPIO = table.Column<DateOnly>(type: "date", nullable: false),
                    ID_USUARIO = table.Column<string>(type: "varchar(255)", nullable: false),
                    ID_REFEICAO = table.Column<int>(type: "int", nullable: false),
                    SN_FECHADO = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CARDAPIO", x => x.ID_CARDAPIO);
                    table.ForeignKey(
                        name: "FK_CARDAPIO_REFEICAO_ID_REFEICAO",
                        column: x => x.ID_REFEICAO,
                        principalTable: "REFEICAO",
                        principalColumn: "ID_REFEICAO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CARDAPIO_USUARIO_ID_USUARIO",
                        column: x => x.ID_USUARIO,
                        principalTable: "USUARIO",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FUNCIONARIO_CARDAPIO",
                columns: table => new
                {
                    ID_FUNCIONARIO = table.Column<int>(type: "int", nullable: false),
                    ID_CARDAPIO = table.Column<int>(type: "int", nullable: false),
                    DT_CONFIRMACAO = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FUNCIONARIO_CARDAPIO", x => new { x.ID_FUNCIONARIO, x.ID_CARDAPIO });
                    table.ForeignKey(
                        name: "FK_FUNCIONARIO_CARDAPIO_CARDAPIO_ID_CARDAPIO",
                        column: x => x.ID_CARDAPIO,
                        principalTable: "CARDAPIO",
                        principalColumn: "ID_CARDAPIO",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FUNCIONARIO_CARDAPIO_FUNCIONARIO_ID_FUNCIONARIO",
                        column: x => x.ID_FUNCIONARIO,
                        principalTable: "FUNCIONARIO",
                        principalColumn: "ID_FUNCIONARIO",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ACOMPANHAMENTO_ID_USUARIO",
                table: "ACOMPANHAMENTO",
                column: "ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_ACOMPANHAMENTO_ID_USUARIO_DELETOU",
                table: "ACOMPANHAMENTO",
                column: "ID_USUARIO_DELETOU");

            migrationBuilder.CreateIndex(
                name: "IX_ACOMPANHAMENTO_REFEICAO_ID_REFEICAO",
                table: "ACOMPANHAMENTO_REFEICAO",
                column: "ID_REFEICAO");

            migrationBuilder.CreateIndex(
                name: "IX_CARDAPIO_DT_CARDAPIO_TP_CARDAPIO",
                table: "CARDAPIO",
                columns: new[] { "DT_CARDAPIO", "TP_CARDAPIO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CARDAPIO_ID_REFEICAO",
                table: "CARDAPIO",
                column: "ID_REFEICAO");

            migrationBuilder.CreateIndex(
                name: "IX_CARDAPIO_ID_USUARIO",
                table: "CARDAPIO",
                column: "ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_FUNCIONARIO_CARDAPIO_ID_CARDAPIO",
                table: "FUNCIONARIO_CARDAPIO",
                column: "ID_CARDAPIO");

            migrationBuilder.CreateIndex(
                name: "IX_REFEICAO_ID_USUARIO",
                table: "REFEICAO",
                column: "ID_USUARIO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ACOMPANHAMENTO_REFEICAO");

            migrationBuilder.DropTable(
                name: "FUNCIONARIO_CARDAPIO");

            migrationBuilder.DropTable(
                name: "ACOMPANHAMENTO");

            migrationBuilder.DropTable(
                name: "CARDAPIO");

            migrationBuilder.DropTable(
                name: "FUNCIONARIO");

            migrationBuilder.DropTable(
                name: "REFEICAO");

            migrationBuilder.DropTable(
                name: "USUARIO");
        }
    }
}
