using System.ComponentModel.DataAnnotations;

namespace Demo.Models;

public class Employee
{
    [Required, StringLength(50)]
    public string Id { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string DepartmentId { get; set; } = string.Empty;
    public Department? Department { get; set; } = null!;

    [Required]
    public string PositionId { get; set; } = string.Empty;
    public Position Position { get; set; } = null!;

    [Range(1900, 9999)]
    public int YearOfJoining { get; set; }

    public bool IsPartTime { get; set; }

    [Range(0, 1000000)]
    public decimal BaseSalary { get; set; }

    [StringLength(200)]
    public string? Picture { get; set; }

    [Required, StringLength(100)]
    public string Password { get; set; } = string.Empty;

    public List<Project> Projects { get; set; } = new();
    public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
}