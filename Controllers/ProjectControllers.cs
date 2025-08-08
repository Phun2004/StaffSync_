using Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Demo.Controllers
{
    [AuthorizeAdmin]
    public class ProjectController : Controller
    {
        private readonly DB _context; // Renamed from _db to _context

        public ProjectController(DB context) // Renamed parameter from db to context
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: Project
        public IActionResult Index()
        {
            var projects = _context.Projects
                .Include(p => p.Employees)
                    .ThenInclude(e => e.Department)
                .Include(p => p.Employees)
                    .ThenInclude(e => e.Position)
                .ToList();

            ViewBag.Projects = projects;
            return View(new Project { Id = NextId() });
        }

        // POST: Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Project model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                model.Status = "Active";
                _context.Projects.Add(model);
                _context.SaveChanges();
                TempData["Info"] = $"Project {model.Id} - {model.Name} created successfully.";
                return RedirectToAction("Index");
            }

            var projects = _context.Projects
                .Include(p => p.Employees)
                .ToList();
            ViewBag.Projects = projects;
            return View(model);
        }

        // GET: Project/Details/P001
        public IActionResult Details(string id)
        {
            var project = _context.Projects
                .Include(p => p.Employees)
                    .ThenInclude(e => e.Department)
                .Include(p => p.Employees)
                    .ThenInclude(e => e.Position)
                .Include(p => p.Employees)
                    .ThenInclude(e => e.Projects)
                .FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                TempData["Error"] = "Project not found.";
                return RedirectToAction("Index");
            }

            var employees = project.Employees.ToList();

            // Calculate project statistics
            var projectStats = new Dictionary<string, object>
            {
                ["CreatedDate"] = project.CreatedDate,
                ["DaysActive"] = (DateTime.Now - project.CreatedDate).Days,
                ["TotalSalaryCost"] = employees.Sum(e => e.BaseSalary),
                ["AverageSalary"] = employees.Any() ? employees.Average(e => e.BaseSalary) : 0,
                ["TeamSize"] = employees.Count,
                ["Status"] = project.Status
            };

            // Calculate individual employee attendance rates (last 30 days)
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var employeeAttendanceRates = new Dictionary<string, decimal>();

            foreach (var employee in employees)
            {
                var attendanceRecords = _context.Attendances
                    .Where(a => a.EmployeeId == employee.Id && a.Date >= thirtyDaysAgo)
                    .ToList();

                var presentCount = attendanceRecords.Count(a => a.Status == "Present");
                var totalRecords = attendanceRecords.Count;
                var attendanceRate = totalRecords > 0 ? Math.Round((decimal)presentCount / totalRecords * 100, 1) : 0;

                employeeAttendanceRates[employee.Id] = attendanceRate;
            }

            // Get other projects for each employee (excluding current project)
            var employeeOtherProjects = new Dictionary<string, List<Project>>();
            foreach (var employee in employees)
            {
                var otherProjects = employee.Projects
                    .Where(p => p.Id != project.Id)
                    .ToList();
                employeeOtherProjects[employee.Id] = otherProjects;
            }

            // Prepare chart data for department distribution
            var departmentData = employees
                .GroupBy(e => e.Department?.Name ?? "No Department")
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            var departmentChartData = new
            {
                labels = departmentData.Select(d => d.Department).ToArray(),
                values = departmentData.Select(d => d.Count).ToArray()
            };

            // Prepare chart data for position distribution
            var positionData = employees
                .GroupBy(e => e.Position?.Name ?? "No Position")
                .Select(g => new { Position = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            var positionChartData = new
            {
                labels = positionData.Select(p => p.Position).ToArray(),
                values = positionData.Select(p => p.Count).ToArray()
            };

            // Set ViewBag data
            ViewBag.Employees = employees;
            ViewBag.ProjectStats = projectStats;
            ViewBag.EmployeeAttendanceRates = employeeAttendanceRates;
            ViewBag.EmployeeOtherProjects = employeeOtherProjects;
            ViewBag.DepartmentChartData = departmentChartData;
            ViewBag.PositionChartData = positionChartData;

            return View(project);
        }

        // POST: Project/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            var project = _context.Projects
                .Include(p => p.Employees)
                .FirstOrDefault(p => p.Id == id);

            if (project != null)
            {
                if (project.Employees.Any())
                {
                    TempData["Error"] = $"Cannot delete project '{project.Name}' because it has assigned employees. Please remove all employees first.";
                }
                else
                {
                    _context.Projects.Remove(project);
                    _context.SaveChanges();
                    TempData["Info"] = $"Project {project.Id} - {project.Name} deleted successfully.";
                }
            }
            else
            {
                TempData["Error"] = "Project not found.";
            }

            return RedirectToAction("Index");
        }

        // GET: Project/Edit
        public IActionResult Edit(string id)
        {
            var project = _context.Projects.Find(id);
            if (project == null)
            {
                TempData["Error"] = "Project not found.";
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // POST: Project/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Project model)
        {
            if (ModelState.IsValid)
            {
                var existingProject = _context.Projects.Find(model.Id);
                if (existingProject != null)
                {
                    existingProject.Name = model.Name;
                    existingProject.Description = model.Description;
                    existingProject.Status = model.Status;
                    existingProject.CreatedDate = model.CreatedDate; // Now allows updating CreatedDate

                    _context.SaveChanges();
                    TempData["Info"] = $"Project {model.Id} - {model.Name} updated successfully.";
                    return RedirectToAction("Details", new { id = model.Id });
                }
                else
                {
                    TempData["Error"] = "Project not found.";
                }
            }
            return View(model);
        }


        // Helper method to generate next Project ID
        private string NextId()
        {
            // Get all project IDs that follow the "P###" format
            var projectIds = _context.Projects
                .Where(p => p.Id.StartsWith("P") && p.Id.Length == 4)
                .Select(p => p.Id)
                .ToList();

            if (!projectIds.Any())
            {
                return "P001"; // First project ID
            }

            // Extract numeric parts and find the maximum
            var maxNumber = projectIds
                .Where(id => int.TryParse(id.Substring(1), out _)) // Only valid numeric parts
                .Select(id => int.Parse(id.Substring(1)))
                .DefaultIfEmpty(0)
                .Max();

            return $"P{(maxNumber + 1):D3}";
        }
    }
}
