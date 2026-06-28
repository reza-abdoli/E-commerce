using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Brand> Brand { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<ProductColor> ProductColor { get; set; }
        public DbSet<ProductFeature> ProductFeature { get; set; }
        public DbSet<ProductGallery> ProductGallery { get; set; }
        public DbSet<ProductSelectedCategory> ProductSelectedCategory { get; set; }
        public DbSet<ProductSize> ProductSize { get; set; }
        public DbSet<ProductVariant> ProductVariant { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
    .SelectMany(t => t.GetForeignKeys())
    .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .IsRequired(false);

            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Brand>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<ProductCategory>().HasQueryFilter(pc => !pc.IsDeleted);
            modelBuilder.Entity<ProductColor>().HasQueryFilter(pc => !pc.IsDeleted);
            modelBuilder.Entity<ProductFeature>().HasQueryFilter(pf => !pf.IsDeleted);
            modelBuilder.Entity<ProductGallery>().HasQueryFilter(pg => !pg.IsDeleted);
            modelBuilder.Entity<ProductSelectedCategory>().HasQueryFilter(psc => !psc.IsDeleted);
            modelBuilder.Entity<ProductSize>().HasQueryFilter(ps => !ps.IsDeleted);
            modelBuilder.Entity<ProductVariant>().HasQueryFilter(pv => !pv.IsDeleted);


        }
    }
}