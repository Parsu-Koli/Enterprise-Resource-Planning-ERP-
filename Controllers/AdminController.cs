using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController(ERPDbContext context) : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Help()
        {
            return View();
        }
        public IActionResult Departments()
        {
            return View(context.Departments.ToList());
        }

        public IActionResult CreateDepartment()
        {
            return View();
        }

        public IActionResult Employees()
        {
            return View(context.Employees
                .Include(e=> e.Department)
                .ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateDepartment(Department department)
        {
            department.CreatedDate = DateTime.Now;
            context.Departments.Add(department);
            context.SaveChanges();
            return RedirectToAction(nameof(Departments));
        }

        public IActionResult LeaveRequests()
        {
            return View(context.Leaves.ToList());
        }

        public IActionResult UpdateLeave(int id, string status)
        {
            var leave = context.Leaves.Find(id);
            if (leave != null)
            {
                leave.Status = status;
                context.SaveChanges();
            }
            return RedirectToAction(nameof(LeaveRequests));
        }

        public async Task<IActionResult> DeleteUsers(int id)
        {
            var employee = await context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            context.Employees.Remove(employee);
            await context.SaveChangesAsync();

            return RedirectToAction("Employees");
        }
        
        public IActionResult CreateHr()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateHr(HRModel model, string userName, string password, string email)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1️⃣ Create User
            var user = new User
            {
                UserName = userName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "HR",
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            context.Users.Add(user);
            await context.SaveChangesAsync(); // Generates UserId

            // 2️⃣ Attach UserId to HR
            model.UserId = user.UserId;

            // 3️⃣ Resume Upload
            if (model.ResumeFile != null)
            {
                // ✅ Use absolute path & safe folder
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/resumes");  // match the URL

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.ResumeFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ResumeFile.CopyToAsync(stream);
                }

                model.ResumePath = "/uploads/resumes/" + fileName; 
            }

            // 4️⃣ Save HR Profile
            context.HRModels.Add(model);
            await context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        public IActionResult HRList()
        {
           var result=  context.HRModels
                .Include(e=> e.User)
                .ToList();
            return View(result);
        }

        public IActionResult DeleteHR(int id)
        {
            var result = context.HRModels
                .Include(e=> e.User)
                .FirstOrDefault(e => e.UserId == id);

            if (result == null)
                return NotFound();

            if (result != null)
                context.Users.Remove(result.User);

            context.HRModels.Remove(result);
            context.SaveChanges();
            return RedirectToAction("HRList");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateHr(int id)
        {
            var hr = await context.HRModels
                                  .Include(h => h.User)
                                  .FirstOrDefaultAsync(h => h.Id == id);

            if (hr == null)
                return NotFound();

            return View(hr);
        }



        public async Task<IActionResult> UpdateHr(int id, HRModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var hr = await context.HRModels
                                  .Include(h => h.User)
                                  .FirstOrDefaultAsync(h => h.Id == id);

            if (hr == null)
                return NotFound();

            
            hr.FirstName = model.FirstName;
            hr.LastName = model.LastName;
            hr.Qualification = model.Qualification;
            hr.Experience = model.Experience;

            
            if (hr.User != null)
                hr.User.Email = model.User?.Email;

            await context.SaveChangesAsync();

            return RedirectToAction("HRList");
        }


    }

}
