using Demo.Models;
using Demo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Demo.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly DB _db;
        private readonly IPayslipPdfService _payslipPdfService;
        private readonly IWebHostEnvironment _environment;

        public EmployeeController(DB db, IPayslipPdfService payslipPdfService, IWebHostEnvironment environment)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _payslipPdfService = payslipPdfService ?? throw new ArgumentNullException(nameof(payslipPdfService));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        // GET: Employee - List all employees (Admin/SuperAdmin only)
        [AuthorizeAdmin]
        public IActionResult Index()
        {
            var employees = _db.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Projects)
                .ToList();
            ViewBag.Employees = employees;
            return View(new Employee());
        }

        // GET: Employee/Create (Admin/SuperAdmin only)
        [AuthorizeAdmin]
        public IActionResult Create()
        {
            ViewBag.Departments = _db.Departments.ToList();
            ViewBag.Positions = _db.Positions.ToList();
            var vm = new EmployeeVM
            {
                Id = NextId(),
                YearOfJoining = DateTime.Now.Year
            };
            return View(vm);
        }

        // POST: Employee/Create (Admin/SuperAdmin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAdmin]
        public async Task<IActionResult> Create(EmployeeVM vm, string password, IFormFile? Picture)
        {
            // Validate phone number uniqueness
            if (_db.Employees.Any(e => e.Phone == vm.Phone && e.Id != vm.Id))
            {
                ModelState.AddModelError("Phone", "This phone number is already in use.");
            }

            // Validate password
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                ModelState.AddModelError("Password", "Password must be at least 6 characters long.");
            }

            // Validate picture file
            if (Picture != null)
            {
                if (Picture.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    ModelState.AddModelError("Picture", "File size must be less than 5MB.");
                }
                if (!new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(Path.GetExtension(Picture.FileName).ToLower()))
                {
                    ModelState.AddModelError("Picture", "Only JPG, PNG, or GIF files are allowed.");
                }
            }

            if (ModelState.IsValid)
            {
                string picturePath = string.Empty;
                if (Picture != null && Picture.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(Picture.FileName)}";
                    var path = Path.Combine(_environment.WebRootPath, "images", fileName);
                    var directory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await Picture.CopyToAsync(stream);
                    }
                    picturePath = $"/images/{fileName}";
                }

                var employee = new Employee
                {
                    Id = vm.Id ?? NextId(),
                    Name = vm.Name ?? string.Empty,
                    Phone = vm.Phone ?? string.Empty,
                    DepartmentId = vm.DepartmentId ?? string.Empty,
                    PositionId = vm.PositionId ?? string.Empty,
                    YearOfJoining = vm.YearOfJoining,
                    IsPartTime = vm.IsPartTime,
                    BaseSalary = vm.BaseSalary,
                    Picture = picturePath,
                    Password = BCrypt.Net.BCrypt.HashPassword(password)
                };

                _db.Employees.Add(employee);
                await _db.SaveChangesAsync();
                TempData["Info"] = $"Employee {employee.Name} created successfully.";
                return RedirectToAction("Index");
            }

            ViewBag.Departments = _db.Departments.ToList();
            ViewBag.Positions = _db.Positions.ToList();
            return View(vm);
        }

        // GET: Employee/Profile (Employee access)
        public IActionResult Profile()
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            if (string.IsNullOrEmpty(userRole))
            {
                TempData["Error"] = "You must be logged in to view your profile.";
                return RedirectToAction("Login", "Account");
            }

            var employeeId = HttpContext.Session.GetString("EmployeeId");
            if (string.IsNullOrEmpty(employeeId) && userRole == "Employee")
            {
                TempData["Error"] = "Invalid session. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            var employee = _db.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Projects)
                .Include(e => e.Payslips)
                .FirstOrDefault(e => e.Id == employeeId);

            if (employee == null)
            {
                TempData["Error"] = "Profile not found.";
                return RedirectToAction("Login", "Account");
            }

            var model = new EmployeeProfileVM
            {
                Id = employee.Id,
                Name = employee.Name,
                Phone = employee.Phone,
                Picture = employee.Picture,
                Department = employee.Department,
                Position = employee.Position,
                YearOfJoining = employee.YearOfJoining,
                IsPartTime = employee.IsPartTime,
                BaseSalary = employee.BaseSalary,
                Projects = employee.Projects,
                RecentPayslips = employee.Payslips.OrderByDescending(p => p.Period).Take(5).ToList()
            };

            return View(model);
        }

        // GET: Employee/ChangePassword (Employee access)
        public IActionResult ChangePassword()
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            if (userRole != "Employee")
            {
                TempData["Error"] = "Access denied. Only employees can change their password.";
                return RedirectToAction("Profile");
            }

            return View();
        }

        // POST: Employee/ChangePassword (Employee access)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            if (userRole != "Employee")
            {
                TempData["Error"] = "Access denied. Only employees can change their password.";
                return RedirectToAction("Profile");
            }

            var employeeId = HttpContext.Session.GetString("EmployeeId");
            var employee = _db.Employees.FirstOrDefault(e => e.Id == employeeId);

            if (employee == null)
            {
                TempData["Error"] = "Employee not found.";
                return RedirectToAction("Profile");
            }

            // Validate current password
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, employee.Password))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
            }

            // Validate new password
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                ModelState.AddModelError("NewPassword", "New password must be at least 6 characters long.");
            }

            if (newPassword != confirmNewPassword)
            {
                ModelState.AddModelError("ConfirmNewPassword", "New password and confirmation do not match.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    employee.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    _db.SaveChanges();
                    TempData["Info"] = "Password changed successfully.";
                    return RedirectToAction("Profile");
                }
                catch (Exception)
                {
                    TempData["Error"] = "An error occurred while changing the password.";
                }
            }

            return View();
        }

        // GET: Employee/PayslipPdf (Employee access for own payslips, Admin/SuperAdmin for all)
        public IActionResult PayslipPdf(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            var employeeId = HttpContext.Session.GetString("EmployeeId");

            if (string.IsNullOrEmpty(userRole))
            {
                TempData["Error"] = "You must be logged in to download payslips.";
                return RedirectToAction("Login", "Account");
            }

            var payslip = _db.Payslips
                .Include(p => p.Employee)
                    .ThenInclude(e => e.Department)
                .Include(p => p.Employee)
                    .ThenInclude(e => e.Position)
                .FirstOrDefault(p => p.Id == id && (userRole != "Employee" || p.EmployeeId == employeeId));

            if (payslip == null)
            {
                TempData["Error"] = "Payslip not found or access denied.";
                return RedirectToAction(userRole == "Employee" ? "Profile" : "Index");
            }

            try
            {
                var pdfBytes = _payslipPdfService.GeneratePayslipPdf(payslip);
                var fileName = $"Payslip_{payslip.Employee?.Name?.Replace(" ", "_")}_{payslip.Period:yyyy_MM}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating payslip PDF: {ex.Message}";
                return RedirectToAction(userRole == "Employee" ? "Profile" : "Index");
            }
        }

        // POST: Employee/UploadPicture (Employee access)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(IFormFile picture)
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            if (userRole != "Employee")
            {
                TempData["Error"] = "Access denied. Only employees can upload profile pictures.";
                return RedirectToAction("Profile");
            }

            var employeeId = HttpContext.Session.GetString("EmployeeId");
            var employee = _db.Employees.FirstOrDefault(e => e.Id == employeeId);
            if (employee == null)
            {
                TempData["Error"] = "Profile not found.";
                return RedirectToAction("Profile");
            }

            if (picture != null && picture.Length > 0)
            {
                if (picture.Length > 5 * 1024 * 1024)
                {
                    TempData["Error"] = "File size must be less than 5MB.";
                    return RedirectToAction("Profile");
                }

                var fileExtension = Path.GetExtension(picture.FileName).ToLower();
                if (!new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(fileExtension))
                {
                    TempData["Error"] = "Only JPG, PNG, or GIF files are allowed.";
                    return RedirectToAction("Profile");
                }

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_environment.WebRootPath, "images", fileName);
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await picture.CopyToAsync(stream);
                }
                employee.Picture = $"/images/{fileName}";
                await _db.SaveChangesAsync();
                TempData["Info"] = "Profile picture updated successfully.";
            }
            else
            {
                TempData["Error"] = "No file selected or invalid file.";
            }
            return RedirectToAction("Profile");
        }

        // GET: Employee/Payslip (Admin/SuperAdmin only)
        [AuthorizeAdmin]
        public IActionResult Payslip(string id)
        {
            var employee = _db.Employees
                .Include(e => e.Payslips)
                .FirstOrDefault(e => e.Id == id);
            if (employee == null)
            {
                TempData["Error"] = "Employee not found.";
                return RedirectToAction("Index");
            }

            var model = new EmployeeVM
            {
                Id = employee.Id,
                Name = employee.Name,
                Payslips = employee.Payslips.ToList()
            };
            return View(model);
        }

        // POST: Employee/GeneratePayslip (Admin/SuperAdmin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAdmin]
        public async Task<IActionResult> GeneratePayslip(string EmployeeId, decimal OvertimeHours, decimal Bonus)
        {
            if (string.IsNullOrEmpty(EmployeeId))
            {
                TempData["Error"] = "Employee ID is required.";
                return RedirectToAction("Index");
            }

            var emp = await _db.Employees.FirstOrDefaultAsync(e => e.Id == EmployeeId);
            if (emp == null)
            {
                TempData["Error"] = "Employee not found.";
                return RedirectToAction("Index");
            }

            var currentMonth = DateTime.Now.Date.AddDays(1 - DateTime.Now.Day);
            var existingPayslip = await _db.Payslips
                .FirstOrDefaultAsync(p => p.EmployeeId == EmployeeId && p.Period.Date == currentMonth);
            if (existingPayslip != null)
            {
                TempData["Error"] = "Payslip for current month already exists.";
                return RedirectToAction("Payslip", new { id = EmployeeId });
            }

            try
            {
                var overtimeRate = emp.IsPartTime ? 15m : 20m; // Example rates: $15/hr part-time, $20/hr full-time
                var overtimePay = OvertimeHours * overtimeRate;
                var epf = emp.BaseSalary * 0.11m;
                var socso = emp.BaseSalary * 0.005m;
                var totalPay = emp.BaseSalary + overtimePay + Bonus - epf - socso;

                var payslip = new Payslip
                {
                    EmployeeId = emp.Id,
                    Period = currentMonth,
                    BaseSalary = emp.BaseSalary,
                    OvertimeHours = OvertimeHours,
                    OvertimePay = overtimePay,
                    Bonus = Bonus,
                    EPF = epf,
                    SOCSO = socso,
                    TotalPay = totalPay
                };

                _db.Payslips.Add(payslip);
                await _db.SaveChangesAsync();
                TempData["Info"] = "Payslip generated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating payslip: {ex.Message}";
            }

            return RedirectToAction("Payslip", new { id = EmployeeId });
        }

        // Helper method to generate next Employee ID
        private string NextId()
        {
            string max = _db.Employees.Max(e => e.Id) ?? "E000";
            if (int.TryParse(max[1..], out int n))
            {
                return $"E{(n + 1):D3}";
            }
            return "E001";
        }

        // Helper method to check if employee exists
        private bool EmployeeExists(string id)
        {
            return _db.Employees.Any(e => e.Id == id);
        }
    }

    public class AuthorizeAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userRole = context.HttpContext.Session.GetString("UserRole");

            // If not logged in or not Super Admin/Admin, redirect to login
            if (string.IsNullOrEmpty(userRole) || (userRole != "Super Admin" && userRole != "Admin"))
            {
                ((Controller)context.Controller).TempData["Error"] = "Unauthorized access.";
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}