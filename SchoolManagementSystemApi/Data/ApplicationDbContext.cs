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
        public DbSet<Test> Tests { get; set; }
        public DbSet<StudentTest> StudentTests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Attendance>()
            //    .Property(a => a.Metadata)
            //    .HasColumnType("jsonb"); // PostgreSQL JSONB column

            // Optional: Seed initial data
            modelBuilder.Entity<Class>().HasData(
                new Class { Id = 1, ClassName = "PG", Section = "A", Teacher = "Miss Esha" },
                new Class { Id = 2, ClassName = "Nursery", Section = "A", Teacher = "Miss Sania" },
                new Class { Id = 3, ClassName = "Prep", Section = "A", Teacher = "Miss Khadija" },
                new Class { Id = 4, ClassName = "Grad 1", Section = "A", Teacher = "Miss Hajra" },
                new Class { Id = 5, ClassName = "Grad 2", Section = "A", Teacher = "Miss Hajra" },
                new Class { Id = 6, ClassName = "Grad 3", Section = "A", Teacher = "Miss Muskan" },
                new Class { Id = 7, ClassName = "Grad 4", Section = "A", Teacher = "Miss Khadija" },
                new Class { Id = 13, ClassName = "Grad 5", Section = "A", Teacher = "Miss Maryam" },
                new Class { Id = 8, ClassName = "Grad 6", Section = "A", Teacher = "Miss Sakeena" },
                new Class { Id = 9, ClassName = "Grad 7", Section = "A", Teacher = "Miss Ambreen" },
                new Class { Id = 10, ClassName = "Grad 8", Section = "A", Teacher = "Sir Shahryar " },
                new Class { Id = 11, ClassName = "Grad 9", Section = "A", Teacher = "Sir Saqib" },
                new Class { Id = 12, ClassName = "Grad 10", Section = "A", Teacher = "Miss Aroosa" }
            );

            // Configure Test entity
                    modelBuilder.Entity<Test>()
              .HasMany(t => t.StudentTests)
              .WithOne(st => st.Test)
              .HasForeignKey(st => st.TestId)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasMany(s => s.StudentTests)
                .WithOne(st => st.Student)
                .HasForeignKey(st => st.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
