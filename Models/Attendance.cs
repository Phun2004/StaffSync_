using System;
using System.ComponentModel.DataAnnotations;

namespace Demo.Models;

public class Attendance
{
    [Required, StringLength(50)]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string EmployeeId { get; set; } = string.Empty;

    public Employee Employee { get; set; } = null!;

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Present"; // Add this line
}