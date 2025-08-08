using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace Demo.Models
{
    public class Payslip
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string EmployeeId { get; set; } = string.Empty;
        public Employee? Employee { get; set; }

        [Required]
        public DateTime Period { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public decimal BaseSalary { get; set; }
        [Required]
        public decimal OvertimeHours { get; set; }
        public decimal Bonus { get; set; }
        public decimal EPF { get; set; }
        public decimal SOCSO { get; set; }

        // 8hour at weekday and 4hour at saturday , total 176 h
        private const decimal StandardWorkingHoursPerMonth = 176m; 

        // Calculate hourly rate based on basic salary
        public decimal HourlyRate => BaseSalary / StandardWorkingHoursPerMonth;

        // Calculate overtime rate (1.5x hourly rate)
        public decimal OvertimeRate => HourlyRate * 1.5m;

        // Calculate overtime pay
        [Precision(18, 2)]
        public decimal OvertimePay { get; set; }

        [Precision(18, 2)]
        public decimal TotalPay { get; set; }
    }
}