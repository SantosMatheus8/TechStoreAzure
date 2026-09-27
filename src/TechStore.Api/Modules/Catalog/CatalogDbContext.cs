using Microsoft.EntityFrameworkCore;
using TechStore.Api.Modules.Catalog.Entities;

namespace TechStore.Api.Modules.Catalog;

public sealed class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Descricao).HasMaxLength(1000);
            entity.Property(e => e.Preco).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DataCriacao).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.CategoriaId);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // IDs de seed conhecidos — coincidem com os IDs das categorias do módulo Categories
        var notebooksId = new Guid("a1b2c3d4-0001-0001-0001-000000000001");
        var perifericosId = new Guid("a1b2c3d4-0001-0001-0001-000000000002");
        var componentesId = new Guid("a1b2c3d4-0001-0001-0001-000000000003");
        var monitoresId = new Guid("a1b2c3d4-0001-0001-0001-000000000004");

        var seedDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = new Guid("b1b2c3d4-0002-0001-0001-000000000001"),
                Nome = "Notebook Dell Inspiron 15",
                Descricao = "Notebook Dell com processador Intel Core i7, 16GB RAM, SSD 512GB",
                Preco = 4299.99m,
                Estoque = 25,
                CategoriaId = notebooksId,
                Ativo = true,
                DataCriacao = seedDate
            },
            new Product
            {
                Id = new Guid("b1b2c3d4-0002-0001-0001-000000000002"),
                Nome = "MacBook Air M2",
                Descricao = "Apple MacBook Air com chip M2, 8GB RAM, SSD 256GB",
                Preco = 7999.00m,
                Estoque = 10,
                CategoriaId = notebooksId,
                Ativo = true,
                DataCriacao = seedDate
            },
            new Product
            {
                Id = new Guid("b1b2c3d4-0002-0001-0001-000000000003"),
                Nome = "Lenovo IdeaPad 3",
                Descricao = "Notebook Lenovo com Ryzen 5, 8GB RAM, SSD 256GB",
                Preco = 2899.00m,
                Estoque = 30,
                CategoriaId = notebooksId,
                Ativo = true,
                DataCriacao = seedDate
            },
            new Product
            {
                Id = new Guid("b1b2c3d4-0002-0001-0001-000000000004"),
                Nome = "Teclado Mecânico Redragon Kumara",
                Descricao = "Teclado mecânico RGB, switches blue, layout ABNT2",
                Preco = 249.90m,
                Estoque = 50,
                CategoriaId = perifericosId,
                Ativo = true,
                DataCriacao = seedDate
            },
            new Product
            {
                Id = new Guid("b1b2c3d4-0002-0001-0001-000000000005"),
                Nome = "Mouse Logitech G305",
                Descricao = "Mouse gamer sem fio, sensor HERO, 12000 DPI",
                Preco = 199.90m,
                Estoque = 40,
                CategoriaId = perifericosId,
                Ativo = true,
                DataCriacao = seedDate
            },
            new Product
            {
                Id = new Guid("b1b2c3d4-0002-0001-0001-000000000006"),
                Nome = "Headset HyperX Cloud Stinger",
                Descricao = "Headset gamer com microfone, drivers 50mm",
                Preco = 299.00m,
                Estoque = 35,
                CategoriaId = perifericosId,
                Ativo = true,
                DataCriacao = seedDate
            },
            new Product
            {
                Id = new Guid("b1b2c3d4-0002-0001-0001-000000000007"),
                Nome = "Placa de Vídeo RTX 4060",
                Descricao = "NVIDIA GeForce RTX 4060 8GB GDDR6",
                Preco = 2199.00m,
                Estoque = 15,
                CategoriaId = componentesId,
                Ativo = true,
                DataCriacao = seedDate
            },
            new Product
            {
                Id = new Guid("b1b2c3d4-0002-0001-0001-000000000008"),
                Nome = "Processador AMD Ryzen 7 5800X",
                Descricao = "Processador AMD Ryzen 7, 8 cores, 16 threads, 4.7GHz",
                Preco = 1599.00m,
                Estoque = 20,
                CategoriaId = componentesId,
                Ativo = true,
                DataCriacao = seedDate
            },
            new Product
            {
                Id = new Guid("b1b2c3d4-0002-0001-0001-000000000009"),
                Nome = "SSD Kingston NV2 1TB",
                Descricao = "SSD NVMe M.2 1TB, leitura 3500MB/s",
                Preco = 399.90m,
                Estoque = 60,
                CategoriaId = componentesId,
                Ativo = true,
                DataCriacao = seedDate
            },
            new Product
            {
                Id = new Guid("b1b2c3d4-0002-0001-0001-000000000010"),
                Nome = "Monitor LG UltraWide 29\"",
                Descricao = "Monitor ultrawide 29 polegadas, IPS, 2560x1080, 75Hz",
                Preco = 1299.00m,
                Estoque = 12,
                CategoriaId = monitoresId,
                Ativo = true,
                DataCriacao = seedDate
            }
        );
    }
}
