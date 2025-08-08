using Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Demo.Controllers
{
    public class AdminController : Controller
    {
        private readonly DB _db;

        public AdminController(DB db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // GET: Admin Dashboard
        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            // Check if user is Admin or Super Admin
            if (userRole != "Admin" && userRole != "Super Admin")
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Login", "Account");
            }

            var dashboardData = new DashboardVM
            {
                TotalEmployees = _db.Employees.Count(),
                TotalDepartments = _db.Departments.Count(),
                ActiveProjects = _db.Projects.Count(p => p.Status == "Active"),
                TotalAdmins = _db.Admins.Count(a => a.IsActive),
                RecentEmployees = _db.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .OrderByDescending(e => e.YearOfJoining)
                    .Take(5)
                    .ToList()
            };

            ViewBag.Title = $"StaffSync | {userRole} Dashboard";
            return View(dashboardData);
        }

        // GET: Admin Management (Super Admin only)
        public IActionResult Management()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Super Admin")
            {
                TempData["Error"] = "Only Super Admin can access admin management.";
                return RedirectToAction("Index");
            }

            var admins = _db.Admins.OrderBy(a => a.Role).ThenBy(a => a.Username).ToList();
            ViewBag.Title = "Admin Management";
            return View(admins);
        }

        // GET: Create Admin (Super Admin only)
        public IActionResult Create()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Super Admin")
            {
                TempData["Error"] = "Only Super Admin can create new admins.";
                return RedirectToAction("Index");
            }

            ViewBag.Title = "Create New Admin";
            return View(new Admin());
        }

        // POST: Create Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Admin admin, string confirmPassword)
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Super Admin")
            {
                TempData["Error"] = "Only Super Admin can create new admins.";
                return RedirectToAction("Index");
            }

            // Validate username uniqueness
            if (_db.Admins.Any(a => a.Username == admin.Username))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
            }

            // Validate email uniqueness
            if (!string.IsNullOrEmpty(admin.Email) && _db.Admins.Any(a => a.Email == admin.Email))
            {
                ModelState.AddModelError("Email", "This email is already in use.");
            }

            // Validate password confirmation
            if (admin.Password != confirmPassword)
            {
                ModelState.AddModelError("Password", "Password and confirmation do not match.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    admin.Password = BCrypt.Net.BCrypt.HashPassword(admin.Password);
                    admin.CreatedDate = DateTime.UtcNow;
                    admin.IsActive = true;

                    _db.Admins.Add(admin);
                    await _db.SaveChangesAsync();

                    TempData["Info"] = $"Admin '{admin.Username}' created successfully.";
                    return RedirectToAction("Management");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating admin: {ex.Message}";
                }
            }

            ViewBag.Title = "Create New Admin";
            return View(admin);
        }

        // GET: Edit Admin
        public IActionResult Edit(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Super Admin")
            {
                TempData["Error"] = "Only Super Admin can edit admins.";
                return RedirectToAction("Index");
            }

            var admin = _db.Admins.Find(id);
            if (admin == null)
            {
                TempData["Error"] = "Admin not found.";
                return RedirectToAction("Management");
            }

            ViewBag.Title = $"Edit Admin - {admin.Username}";
            return View(admin);
        }

        // POST: Edit Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Admin admin, string? newPassword, string? confirmPassword)
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Super Admin")
            {
                TempData["Error"] = "Only Super Admin can edit admins.";
                return RedirectToAction("Index");
            }

            var existingAdmin = await _db.Admins.FindAsync(admin.Id);
            if (existingAdmin == null)
            {
                TempData["Error"] = "Admin not found.";
                return RedirectToAction("Management");
            }

            // Validate username uniqueness
            if (_db.Admins.Any(a => a.Username == admin.Username && a.Id != admin.Id))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
            }

            // Validate email uniqueness
            if (!string.IsNullOrEmpty(admin.Email) && _db.Admins.Any(a => a.Email == admin.Email && a.Id != admin.Id))
            {
                ModelState.AddModelError("Email", "This email is already in use.");
            }

            // Validate new password if provided
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("NewPassword", "New password and confirmation do not match.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingAdmin.Username = admin.Username;
                    existingAdmin.Email = admin.Email;
                    existingAdmin.Role = admin.Role;
                    existingAdmin.IsActive = admin.IsActive;

                    // Update password if new one provided
                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        existingAdmin.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    }

                    await _db.SaveChangesAsync();
                    TempData["Info"] = $"Admin '{existingAdmin.Username}' updated successfully.";
                    return RedirectToAction("Management");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error updating admin: {ex.Message}";
                }
            }

            ViewBag.Title = $"Edit Admin - {admin.Username}";
            return View(admin);
        }

        // POST: Toggle Admin Status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Super Admin")
            {
                return Json(new { success = false, message = "Only Super Admin can toggle admin status." });
            }

            try
            {
                var admin = await _db.Admins.FindAsync(id);
                if (admin == null)
                {
                    return Json(new { success = false, message = "Admin not found." });
                }

                // Don't allow deactivating the current user
                var currentUsername = HttpContext.Session.GetString("Username");
                if (admin.Username == currentUsername)
                {
                    return Json(new { success = false, message = "You cannot deactivate your own account." });
                }

                admin.IsActive = !admin.IsActive;
                await _db.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Admin '{admin.Username}' has been {(admin.IsActive ? "activated" : "deactivated")}.",
                    isActive = admin.IsActive
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: Admin Profile Settings
        public IActionResult Profile()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var username = HttpContext.Session.GetString("Username");

            if (userRole != "Admin" && userRole != "Super Admin")
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Login", "Account");
            }

            var admin = _db.Admins.FirstOrDefault(a => a.Username == username);
            if (admin == null)
            {
                TempData["Error"] = "Admin profile not found.";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Title = "My Profile";
            return View(admin);
        }

        // POST: Update Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(Admin model, string? newPassword, string? confirmPassword)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var username = HttpContext.Session.GetString("Username");

            if (userRole != "Admin" && userRole != "Super Admin")
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Login", "Account");
            }

            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Username == username);
            if (admin == null)
            {
                TempData["Error"] = "Admin profile not found.";
                return RedirectToAction("Login", "Account");
            }

            // Validate email uniqueness
            if (!string.IsNullOrEmpty(model.Email) && _db.Admins.Any(a => a.Email == model.Email && a.Id != admin.Id))
            {
                ModelState.AddModelError("Email", "This email is already in use.");
            }

            // Validate new password if provided
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("NewPassword", "New password and confirmation do not match.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    admin.Email = model.Email;

                    // Update password if new one provided
                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        admin.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    }

                    await _db.SaveChangesAsync();
                    TempData["Info"] = "Profile updated successfully.";
                    return RedirectToAction("Profile");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error updating profile: {ex.Message}";
                }
            }

            ViewBag.Title = "My Profile";
            return View(model);
        }
    }
}