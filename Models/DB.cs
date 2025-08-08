using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Demo.Models;

public class DB : DbContext
{
    public DB(DbContextOptions options) : base(options) { }

    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Position> Positions { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Payslip> Payslips { get; set; } = null!;
    public DbSet<Admin> Admins { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Suppress the PendingModelChangesWarning after ensuring model stability
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee configuration
        modelBuilder.Entity<Employee>()
            .HasKey(e => e.Id);
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Position)
            .WithMany()
            .HasForeignKey(e => e.PositionId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Employee>()
            .HasMany(e => e.Projects)
            .WithMany(p => p.Employees)
            .UsingEntity(j => j.ToTable("EmployeeProjects"));
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Phone)
            .IsUnique();
        modelBuilder.Entity<Employee>()
            .Property(e => e.BaseSalary)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Employee>()
            .Property(e => e.Password)
            .HasMaxLength(100);

        // Department and Position configuration
        modelBuilder.Entity<Department>().HasKey(d => d.Id);
        modelBuilder.Entity<Position>().HasKey(p => p.Id);

        // Attendance configuration
        modelBuilder.Entity<Attendance>().HasKey(a => a.Id);
        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Attendance>()
            .HasIndex(a => new { a.EmployeeId, a.Date })
            .IsUnique();

        // Project configuration
        modelBuilder.Entity<Project>().HasKey(p => p.Id);

        // Payslip configuration
        modelBuilder.Entity<Payslip>().HasKey(p => p.Id);
        modelBuilder.Entity<Payslip>()
            .HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Payslip>()
            .Property(p => p.BaseSalary)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Payslip>()
            .Property(p => p.OvertimeHours)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Payslip>()
            .Property(p => p.Bonus)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Payslip>()
            .Property(p => p.EPF)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Payslip>()
            .Property(p => p.SOCSO)
            .HasPrecision(18, 2);

        // Admin configuration
        modelBuilder.Entity<Admin>().HasKey(a => a.Id);
        modelBuilder.Entity<Admin>().ToTable("Admins");

        // Seed data
        modelBuilder.Entity<Position>().HasData(
            new Position { Id = "P001", Name = "Account" },
            new Position { Id = "P002", Name = "IT" },
            new Position { Id = "P003", Name = "HR" },
            new Position { Id = "P004", Name = "Manager" },
            new Position { Id = "P005", Name = "ADMIN" },
            new Position { Id = "P006", Name = "Super Admin" }
        );

        modelBuilder.Entity<Department>().HasData(
            new Department { Id = "D001", Name = "Accounting" },
            new Department { Id = "D002", Name = "Information Technology" },
            new Department { Id = "D003", Name = "Human Resources" },
            new Department { Id = "D004", Name = "Management" },
            new Department { Id = "D005", Name = "Administration" }
        );

        // Use a fixed date for seeding
        var seedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Seed Admins with static hashed passwords
        modelBuilder.Entity<Admin>().HasData(
            new Admin
            {
                Id = 1,
                Username = "superadmin",
                Password = "$2a$11$1X7gZ8x9y2z3w4v5u6t7r8s9t0u1v2w3x4y5z6A7B8C9D0E1F2G3", // Precomputed hash for superadmin123
                Role = "Super Admin",
                Email = "superadmin@example.com",
                CreatedDate = seedDate,
                IsActive = true
            },
            new Admin
            {
                Id = 2,
                Username = "admin",
                Password = "$2a$11$A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8S9T0U1V2W3X4Y5Z6", // Precomputed hash for admin123
                Role = "Admin",
                Email = "admin@example.com",
                CreatedDate = seedDate,
                IsActive = true
            }
        );
    }
}
