using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechStore.Api.Modules.Catalog.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estoque = table.Column<int>(type: "int", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "Products",
                columns: new[] { "Id", "Ativo", "CategoriaId", "DataAtualizacao", "DataCriacao", "Descricao", "Estoque", "Nome", "Preco" },
                values: new object[,]
                {
                    { new Guid("b1b2c3d4-0002-0001-0001-000000000001"), true, new Guid("a1b2c3d4-0001-0001-0001-000000000001"), null, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Notebook Dell com processador Intel Core i7, 16GB RAM, SSD 512GB", 25, "Notebook Dell Inspiron 15", 4299.99m },
                    { new Guid("b1b2c3d4-0002-0001-0001-000000000002"), true, new Guid("a1b2c3d4-0001-0001-0001-000000000001"), null, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Apple MacBook Air com chip M2, 8GB RAM, SSD 256GB", 10, "MacBook Air M2", 7999.00m },
                    { new Guid("b1b2c3d4-0002-0001-0001-000000000003"), true, new Guid("a1b2c3d4-0001-0001-0001-000000000001"), null, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Notebook Lenovo com Ryzen 5, 8GB RAM, SSD 256GB", 30, "Lenovo IdeaPad 3", 2899.00m },
                    { new Guid("b1b2c3d4-0002-0001-0001-000000000004"), true, new Guid("a1b2c3d4-0001-0001-0001-000000000002"), null, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Teclado mecânico RGB, switches blue, layout ABNT2", 50, "Teclado Mecânico Redragon Kumara", 249.90m },
                    { new Guid("b1b2c3d4-0002-0001-0001-000000000005"), true, new Guid("a1b2c3d4-0001-0001-0001-000000000002"), null, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Mouse gamer sem fio, sensor HERO, 12000 DPI", 40, "Mouse Logitech G305", 199.90m },
                    { new Guid("b1b2c3d4-0002-0001-0001-000000000006"), true, new Guid("a1b2c3d4-0001-0001-0001-000000000002"), null, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Headset gamer com microfone, drivers 50mm", 35, "Headset HyperX Cloud Stinger", 299.00m },
                    { new Guid("b1b2c3d4-0002-0001-0001-000000000007"), true, new Guid("a1b2c3d4-0001-0001-0001-000000000003"), null, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "NVIDIA GeForce RTX 4060 8GB GDDR6", 15, "Placa de Vídeo RTX 4060", 2199.00m },
                    { new Guid("b1b2c3d4-0002-0001-0001-000000000008"), true, new Guid("a1b2c3d4-0001-0001-0001-000000000003"), null, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Processador AMD Ryzen 7, 8 cores, 16 threads, 4.7GHz", 20, "Processador AMD Ryzen 7 5800X", 1599.00m },
                    { new Guid("b1b2c3d4-0002-0001-0001-000000000009"), true, new Guid("a1b2c3d4-0001-0001-0001-000000000003"), null, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "SSD NVMe M.2 1TB, leitura 3500MB/s", 60, "SSD Kingston NV2 1TB", 399.90m },
                    { new Guid("b1b2c3d4-0002-0001-0001-000000000010"), true, new Guid("a1b2c3d4-0001-0001-0001-000000000004"), null, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Monitor ultrawide 29 polegadas, IPS, 2560x1080, 75Hz", 12, "Monitor LG UltraWide 29\"", 1299.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoriaId",
                schema: "catalog",
                table: "Products",
                column: "CategoriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products",
                schema: "catalog");
        }
    }
}
