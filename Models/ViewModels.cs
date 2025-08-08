using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Demo.Models
{
    public class EmployeeVM
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public string DepartmentId { get; set; } = string.Empty;
        public Department? Department { get; set; }

        [Required(ErrorMessage = "Position is required")]
        public string PositionId { get; set; } = string.Empty;
        public Position? Position { get; set; }

        [Required(ErrorMessage = "Year of joining is required")]
        [Range(1900, 3000, ErrorMessage = "Please enter a valid year")]
        public int YearOfJoining { get; set; }

        public bool IsPartTime { get; set; }

        [Required(ErrorMessage = "Base salary is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Base salary must be greater than 0")]
        [DataType(DataType.Currency)]
        public decimal BaseSalary { get; set; }

        public string? Picture { get; set; }
        public List<Project> Projects { get; set; } = new();
        public List<Payslip> Payslips { get; set; } = new();
    }

    public class AttendanceVM
    {
        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Status is required")]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Present";

        // Additional properties for better display
        public Employee? Employee { get; set; }
        public string? EmployeeName { get; set; }
    }

    public class PayslipVM
    {
        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Period { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Base salary must be non-negative")]
        [DataType(DataType.Currency)]
        public decimal BaseSalary { get; set; }

        [Range(0, 200, ErrorMessage = "Overtime hours must be between 0 and 200")]
        public decimal OvertimeHours { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Bonus must be non-negative")]
        [DataType(DataType.Currency)]
        public decimal Bonus { get; set; }

        [DataType(DataType.Currency)]
        public decimal EPF { get; set; }

        [DataType(DataType.Currency)]
        public decimal SOCSO { get; set; }

        // Calculated properties
        public decimal OvertimePay => OvertimeHours * (IsPartTime ? 15m : 20m);
        public decimal TotalDeductions => EPF + SOCSO;
        public decimal NetSalary => BaseSalary + OvertimePay + Bonus - TotalDeductions;

        // Additional properties
        public Employee? Employee { get; set; }
        public bool IsPartTime { get; set; }
    }

    public class DashboardVM
    {
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public int ActiveProjects { get; set; }
        public int TotalAdmins { get; set; }
        public List<Employee> RecentEmployees { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
        public List<Employee> Employees { get; set; } = new();
        public List<Attendance> RecentAttendances { get; set; } = new();

        // Additional dashboard statistics
        public int InactiveEmployees { get; set; }
        public int CompletedProjects { get; set; }
        public decimal TotalPayroll { get; set; }
        public Dictionary<string, int> EmployeesByDepartment { get; set; } = new();
        public Dictionary<string, int> AttendanceStats { get; set; } = new();
    }

    // New ViewModel for Admin management
    public class AdminVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [StringLength(50)]
        public string Role { get; set; } = "Admin";

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100)]
        public string? Email { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // For editing - optional new password
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match")]
        public string? ConfirmNewPassword { get; set; }

        // Display properties
        public bool IsCurrentUser { get; set; }
        public string StatusText => IsActive ? "Active" : "Inactive";
        public string RoleBadgeClass => Role == "Super Admin" ? "bg-danger" : "bg-warning";
    }

    // New ViewModel for Department details
    public class DepartmentDetailsVM
    {
        public Department Department { get; set; } = new();
        public List<Employee> Employees { get; set; } = new();
        public DepartmentStatsVM Stats { get; set; } = new();
        public List<Project> DepartmentProjects { get; set; } = new();
    }

    public class DepartmentStatsVM
    {
        public decimal TotalSalary { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public decimal AttendanceRate { get; set; }
        public Dictionary<string, decimal> EmployeeAttendanceRates { get; set; } = new();
        public Dictionary<string, List<Project>> EmployeeProjects { get; set; } = new();
    }

    // New ViewModel for Employee Profile (self-service)
    public class EmployeeProfileVM
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Picture { get; set; }
        public Department? Department { get; set; }
        public Position? Position { get; set; }
        public int YearOfJoining { get; set; }
        public bool IsPartTime { get; set; }
        public decimal BaseSalary { get; set; }
        public List<Project> Projects { get; set; } = new();
        public List<Payslip> RecentPayslips { get; set; } = new();
        public List<Attendance> RecentAttendances { get; set; } = new();

        // Profile stats
        public int YearsOfService => DateTime.Now.Year - YearOfJoining;
        public int ProjectCount => Projects.Count;
        public decimal AttendanceRate { get; set; }
    }

    // New ViewModel for Login
    public class LoginVM
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }

    // New ViewModel for Project assignment
    public class ProjectAssignmentVM
    {
        public string EmployeeId { get; set; } = string.Empty;
        public Employee? Employee { get; set; }
        public List<Project> AvailableProjects { get; set; } = new();
        public List<Project> AssignedProjects { get; set; } = new();
        public string[] SelectedProjectIds { get; set; } = Array.Empty<string>();
    }

    // New ViewModel for Attendance management
    public class AttendanceManagementVM
    {
        public DateTime Date { get; set; } = DateTime.Today;
        public List<Employee> Employees { get; set; } = new();
        public Dictionary<string, Attendance?> AttendanceRecords { get; set; } = new();
        public Dictionary<string, string> AttendanceStatus { get; set; } = new();
    }
}