using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TodoApi
{
    public partial class to_do_tasksContext : DbContext
    {
        public to_do_tasksContext()
        {
        }

        public to_do_tasksContext(DbContextOptions<to_do_tasksContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<User> Users { get; set; }

        // עדכון הפונקציה כך שתשתמש בהגדרות בקובץ appsettings.json
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // מקבל את ה-Connection String מתוך appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("to_do_tasks");

            // אם לא מצאנו את ה-Connection String בקובץ, נזרוק חריגה
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("The connection string is not configured.");
            }

            // מחבר למסד הנתונים ב-Cloud באמצעות ה-Connection String
            optionsBuilder.UseMySql(connectionString, Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.40-mysql"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .UseCollation("utf8mb4_unicode_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("Items");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("Users");
                entity.Property(e => e.NameUser).HasMaxLength(100).IsRequired();
                entity.Property(e => e.password).HasMaxLength(20).IsRequired();
            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
