using CollegeJournal.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CollegeJournal.Web.Data;

public class CollegeDbContext : DbContext
{
    public CollegeDbContext(DbContextOptions<CollegeDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>(entity =>
        {
            entity.ToTable("Groups");
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(g => g.Name).IsUnique();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students", table => table.HasCheckConstraint("CK_Students_Age", "[Age] BETWEEN 14 AND 100"));
            entity.HasKey(s => s.Id);
            entity.Property(s => s.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(s => s.LastName).HasMaxLength(50).IsRequired();
            entity.HasOne(s => s.Group).WithMany().HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Teacher>().Property(t => t.FullName).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<Subject>().Property(s => s.Name).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<Subject>().HasOne<Teacher>().WithMany().HasForeignKey(s => s.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.ToTable("Grades", table => table.HasCheckConstraint("CK_Grades_Value", "[Value] BETWEEN 1 AND 5"));
            entity.Property(g => g.GradeDate).HasColumnType("date");
            entity.HasOne<Student>().WithMany().HasForeignKey(g => g.StudentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Subject>().WithMany().HasForeignKey(g => g.SubjectId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
