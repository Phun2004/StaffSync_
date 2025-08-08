using System.ComponentModel.DataAnnotations;

namespace Demo.Models;

public class Project
{
    [Required, StringLength(50)]
    public string Id { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [StringLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; } = "Active";

    public List<Employee> Employees { get; set; } = new();
}