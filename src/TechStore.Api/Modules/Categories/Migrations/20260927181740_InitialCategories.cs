using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechStore.Api.Modules.Categories.Migrations
{
    /// <inheritdoc />
    public partial class InitialCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "categories");

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "categories",
                table: "Categories",
                columns: new[] { "Id", "Ativo", "DataCriacao", "Descricao", "Nome" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-0001-0001-0001-000000000001"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Notebooks e laptops para uso pessoal e profissional", "Notebooks" },
                    { new Guid("a1b2c3d4-0001-0001-0001-000000000002"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teclados, mouses, headsets e outros periféricos", "Periféricos" },
                    { new Guid("a1b2c3d4-0001-0001-0001-000000000003"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Placas de vídeo, processadores, memórias e SSDs", "Componentes" },
                    { new Guid("a1b2c3d4-0001-0001-0001-000000000004"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Monitores e telas para desktop", "Monitores" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories",
                schema: "categories");
        }
    }
}
