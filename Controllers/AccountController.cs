using Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Demo.Controllers;

public class AccountController : Controller
{
    private readonly DB _db;

    public AccountController(DB db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("UserRole") != null)
        {
            return RedirectToAppropriateHome();
        }

        ViewBag.Title = "StaffSync | Login";
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        ViewBag.Title = "StaffSync | Login";

        try
        {
            // Check Admins table first
            var admin = _db.Admins
                .FirstOrDefault(a => a.Username == username && a.IsActive);

            if (admin != null && BCrypt.Net.BCrypt.Verify(password, admin.Password))
            {
                HttpContext.Session.SetString("UserRole", admin.Role);
                HttpContext.Session.SetString("Username", admin.Username);
                TempData["Info"] = $"Welcome back, {admin.Username}!";

                // Redirect based on role
                if (admin.Role == "Super Admin" || admin.Role == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            // Check Employees table
            var employee = _db.Employees
                .FirstOrDefault(e => e.Phone == username);

            if (employee != null && BCrypt.Net.BCrypt.Verify(password, employee.Password))
            {
                HttpContext.Session.SetString("UserRole", "Employee");
                HttpContext.Session.SetString("Username", employee.Name);
                HttpContext.Session.SetString("EmployeeId", employee.Id);
                TempData["Info"] = $"Welcome back, {employee.Name}!";
                return RedirectToAction("Profile", "Employee");
            }

            ViewBag.Error = "Invalid username or password!";
            return View();
        }
        catch (Exception)
        {
            ViewBag.Error = "An error occurred while processing your request.";
            return View();
        }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Info"] = "You have been logged out successfully.";
        return RedirectToAction("Login");
    }

    private IActionResult RedirectToAppropriateHome()
    {
        var userRole = HttpContext.Session.GetString("UserRole");

        return userRole switch
        {
            "Super Admin" or "Admin" => RedirectToAction("Index", "Admin"),
            "Employee" => RedirectToAction("Profile", "Employee"),
            _ => RedirectToAction("Index", "Home")
        };
    }
}