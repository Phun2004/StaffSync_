using Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Demo.Controllers;

public class DepartmentController : Controller
{
    private readonly DB db;

    public DepartmentController(DB db) => this.db = db;

    public IActionResult Index()
    {
        ViewBag.Departments = db.Departments.ToList();
        return View(new Department { Id = NextId() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(Department model)
    {
        if (ModelState.IsValid)
        {
            db.Departments.Add(model);
            db.SaveChanges();
            TempData["Info"] = $"Department {model.Id} inserted.";
            return RedirectToAction("Index");
        }
        ViewBag.Departments = db.Departments.ToList();
        return View(model);
    }

    // GET: Department/Details/D001
    public IActionResult Details(string id)
    {
        var department = db.Departments.Find(id);
        if (department == null)
        {
            TempData["Error"] = "Department not found.";
            return RedirectToAction("Index");
        }

        // Get all employees in this department with related data
        var employees = db.Employees
            .Include(e => e.Position)
            .Include(e => e.Projects)
            .Where(e => e.DepartmentId == id)
            .ToList();

        // Calculate salary statistics
        var salaryStats = new Dictionary<string, decimal>();
        if (employees.Any())
        {
            salaryStats["TotalSalary"] = employees.Sum(e => e.BaseSalary);
            salaryStats["AverageSalary"] = employees.Average(e => e.BaseSalary);
            salaryStats["MinSalary"] = employees.Min(e => e.BaseSalary);
            salaryStats["MaxSalary"] = employees.Max(e => e.BaseSalary);
        }
        else
        {
            salaryStats["TotalSalary"] = 0;
            salaryStats["AverageSalary"] = 0;
            salaryStats["MinSalary"] = 0;
            salaryStats["MaxSalary"] = 0;
        }

        // Calculate attendance statistics (last 30 days)
        var thirtyDaysAgo = DateTime.Now.AddDays(-30);
        var attendanceRecords = db.Attendances
            .Where(a => employees.Select(e => e.Id).Contains(a.EmployeeId) && a.Date >= thirtyDaysAgo)
            .ToList();

        var totalPossibleDays = employees.Count * 30; // Simplified calculation
        var presentCount = attendanceRecords.Count(a => a.Status == "Present");
        var absentCount = attendanceRecords.Count(a => a.Status == "Absent");
        var attendanceRate = totalPossibleDays > 0 ? Math.Round((decimal)presentCount / Math.Max(presentCount + absentCount, 1) * 100, 1) : 0;

        var attendanceStats = new Dictionary<string, object>
        {
            ["PresentCount"] = presentCount,
            ["AbsentCount"] = absentCount,
            ["AttendanceRate"] = attendanceRate,
            ["TotalRecords"] = attendanceRecords.Count
        };

        // Calculate individual employee attendance rates
        var employeeAttendanceRates = new Dictionary<string, decimal>();
        foreach (var employee in employees)
        {
            var empAttendance = attendanceRecords.Where(a => a.EmployeeId == employee.Id).ToList();
            var empPresent = empAttendance.Count(a => a.Status == "Present");
            var empTotal = empAttendance.Count;
            var empRate = empTotal > 0 ? Math.Round((decimal)empPresent / empTotal * 100, 1) : 0;
            employeeAttendanceRates[employee.Id] = empRate;
        }

        // Get employee projects
        var employeeProjects = employees.ToDictionary(e => e.Id, e => e.Projects.ToList());

        // Prepare chart data for salary distribution by position
        var positionSalaryData = employees
            .GroupBy(e => e.Position?.Name ?? "No Position")
            .Select(g => new
            {
                Position = g.Key,
                AverageSalary = g.Average(e => e.BaseSalary),
                Count = g.Count()
            })
            .OrderByDescending(x => x.AverageSalary)
            .ToList();

        var chartData = new
        {
            labels = positionSalaryData.Select(p => $"{p.Position} ({p.Count})").ToArray(),
            values = positionSalaryData.Select(p => Math.Round(p.AverageSalary, 2)).ToArray()
        };

        // Set ViewBag data
        ViewBag.Employees = employees;
        ViewBag.SalaryStats = salaryStats;
        ViewBag.AttendanceStats = attendanceStats;
        ViewBag.EmployeeAttendanceRates = employeeAttendanceRates;
        ViewBag.EmployeeProjects = employeeProjects;
        ViewBag.ChartData = chartData;

        return View(department);
    }

    private string NextId()
    {
        string max = db.Departments.Max(d => d.Id) ?? "D000";
        int n = int.Parse(max[1..]);
        return $"D{(n + 1):D3}";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(string id)
    {
        var dept = db.Departments.Find(id);
        if (dept != null)
        {
            if (!db.Employees.Any(e => e.DepartmentId == id))
            {
                db.Departments.Remove(dept);
                db.SaveChanges();
                TempData["Info"] = $"Department {id} deleted.";
            }
            else
            {
                TempData["Info"] = "Cannot delete department with assigned employees.";
            }
        }
        return RedirectToAction("Index");
    }
}