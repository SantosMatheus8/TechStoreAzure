using Microsoft.EntityFrameworkCore;
using TechStore.Api.Modules.Categories.Entities;

namespace TechStore.Api.Modules.Categories;

public sealed class CategoriesDbContext : DbContext
{
    public CategoriesDbContext(DbContextOptions<CategoriesDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("categories");

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.DataCriacao).HasDefaultValueSql("GETUTCDATE()");
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = new Guid("a1b2c3d4-0001-0001-0001-000000000001"),
                Nome = "Notebooks",
                Descricao = "Notebooks e laptops para uso pessoal e profissional",
                Ativo = true,
                DataCriacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = new Guid("a1b2c3d4-0001-0001-0001-000000000002"),
                Nome = "Periféricos",
                Descricao = "Teclados, mouses, headsets e outros periféricos",
                Ativo = true,
                DataCriacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = new Guid("a1b2c3d4-0001-0001-0001-000000000003"),
                Nome = "Componentes",
                Descricao = "Placas de vídeo, processadores, memórias e SSDs",
                Ativo = true,
                DataCriacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category
            {
                Id = new Guid("a1b2c3d4-0001-0001-0001-000000000004"),
                Nome = "Monitores",
                Descricao = "Monitores e telas para desktop",
                Ativo = true,
                DataCriacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
