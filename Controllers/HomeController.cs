using Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo.Controllers;

public class HomeController : Controller
{
    private readonly DB _db;

    public HomeController(DB db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public IActionResult Index()
    {
        // Check if user is logged in
        var userRole = HttpContext.Session.GetString("UserRole");
        var isLoggedIn = !string.IsNullOrEmpty(userRole) && userRole == "Super Admin";

        if (!isLoggedIn)
        {
            // Redirect to login if not logged in or not a Super Admin
            return RedirectToAction("Login", "Account");
        }

        // Set ViewBag properties for the view
        ViewBag.Title = "StaffSync | Home";
        ViewBag.IsLoggedIn = true;
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.CurrentDate = DateTime.Now;

        // Prepare dashboard data
        var dashboardData = new DashboardVM
        {
            TotalEmployees = _db.Employees.Count(),
            TotalDepartments = _db.Departments.Count(),
            ActiveProjects = _db.Projects.Count(p => p.Status == "Active"),
            TotalAdmins = _db.Admins.Count(),
            RecentEmployees = _db.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .OrderByDescending(e => e.Id)
                .Take(5)
                .ToList(),
            Departments = _db.Departments.ToList(),
            Employees = _db.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .ToList(),
            RecentAttendances = _db.Attendances
                .Include(a => a.Employee)
                .OrderByDescending(a => a.Date)
                .Take(5)
                .ToList()
        };

        return View(dashboardData);
    }
}