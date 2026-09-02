using Microsoft.EntityFrameworkCore;
using StudentApi.Api.Models;

namespace StudentApi.Api.Data;

public class StudentDbContext(DbContextOptions<StudentDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(s => s.Id);

            entity.Property(s => s.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(s => s.LastName).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Email).HasMaxLength(256).IsRequired();
            entity.Property(s => s.Department).HasMaxLength(100);
            entity.Property(s => s.Gpa).HasColumnType("decimal(4,2)");

            entity.HasIndex(s => s.Email).IsUnique();
            entity.Ignore(s => s.FullName);
        });
    }
}
