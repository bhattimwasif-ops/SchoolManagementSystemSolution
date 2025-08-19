using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SchoolManagementSystemApi.Models;

namespace SchoolManagementSystemApi.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }       

        public DbSet<Class> Classes { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Attendance>()
                .Property(a => a.Metadata)
                .HasColumnType("jsonb"); // PostgreSQL JSONB column

            // Optional: Seed initial data
            modelBuilder.Entity<Class>().HasData(
                new Class { Id = 1, ClassName = "PG", Section = "A", Teacher = "Miss Smith" },
                new Class { Id = 2, ClassName = "Nursery", Section = "A", Teacher = "Miss Smith" },
                new Class { Id = 3, ClassName = "Prep", Section = "A", Teacher = "Miss Smith" },
                new Class { Id = 4, ClassName = "Grad 1", Section = "A", Teacher = "Miss Smith" },
                new Class { Id = 5, ClassName = "Grad 2", Section = "A", Teacher = "Miss Smith" },
                new Class { Id = 6, ClassName = "Grad 3", Section = "A", Teacher = "Miss Smith" },
                new Class { Id = 7, ClassName = "Grad 4", Section = "A", Teacher = "Miss Smith" }
            );
        }

    }
}
