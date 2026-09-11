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
    
        #region Dashboard

        // GET: /Admin/Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        #endregion


        #region Help

        // GET: /Admin/Help
        public IActionResult Help()
        {
            return View();
        }

        #endregion


        #region Department Management

        // GET: /Admin/Departments
        public IActionResult Departments()
        {
            return View(
                context.Departments.ToList()
            );
        }


        // GET: /Admin/CreateDepartment
        public IActionResult CreateDepartment()
        {
            return View();
        }


        // POST: /Admin/CreateDepartment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateDepartment(
            Department department)
        {
            department.CreatedDate = DateTime.UtcNow;

            context.Departments.Add(department);
            context.SaveChanges();

            return RedirectToAction(
                nameof(Departments)
            );
        }

        #endregion


        #region Employee Management

        // GET: /Admin/Employees
        public IActionResult Employees()
        {
            var employees = context.Employees
                .Include(e => e.Department)
                .ToList();

            return View(employees);
        }


        // GET: /Admin/DeleteUsers
        public async Task<IActionResult> DeleteUsers(int id)
        {
            var employee = await context.Employees
                .FindAsync(id);

            if (employee == null)
                return NotFound();

            context.Employees.Remove(employee);

            await context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Employees)
            );
        }

        #endregion


        #region Leave Management

        // GET: /Admin/LeaveRequests
        public IActionResult LeaveRequests()
        {
            return View(
                context.Leaves.ToList()
            );
        }


        // GET: /Admin/UpdateLeave
        public IActionResult UpdateLeave(
            int id,
            string status)
        {
            var leave = context.Leaves.Find(id);

            if (leave != null)
            {
                leave.Status = status;

                context.SaveChanges();
            }

            return RedirectToAction(
                nameof(LeaveRequests)
            );
        }

        #endregion


        #region HR Management

        // ---------------------------------------------------------
        // CREATE HR
        // ---------------------------------------------------------

        // GET: /Admin/CreateHr
        [HttpGet]
        public IActionResult CreateHr()
        {
            return View();
        }


        // POST: /Admin/CreateHr
        [HttpPost]
        public async Task<IActionResult> CreateHr(
            HRModel model,
            string userName,
            string password,
            string email)
        {
            if (!ModelState.IsValid)
                return View(model);


            // -----------------------------------------------------
            // Create User Account
            // -----------------------------------------------------

            var user = new User
            {
                UserName = userName,
                Email = email,

                PasswordHash = password,    

                Role = "HR",

                IsActive = true,

                CreatedDate = DateTime.UtcNow
            };


            context.Users.Add(user);

            // Save first so UserId is generated
            await context.SaveChangesAsync();


            // -----------------------------------------------------
            // Attach UserId to HR Profile
            // -----------------------------------------------------

            model.UserId = user.UserId;


            // -----------------------------------------------------
            // Resume Upload
            // -----------------------------------------------------

            if (model.ResumeFile != null)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/resumes"
                );


                // Create folder if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(
                        uploadsFolder
                    );
                }


                // Generate unique file name
                var fileName =
                    Guid.NewGuid()
                    + Path.GetExtension(
                        model.ResumeFile.FileName
                    );


                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName
                );


                // Save resume
                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await model.ResumeFile
                        .CopyToAsync(stream);
                }


                // Save URL path in database
                model.ResumePath =
                    "/uploads/resumes/" + fileName;
            }


            // -----------------------------------------------------
            // Save HR Profile
            // -----------------------------------------------------

            context.HRModels.Add(model);

            await context.SaveChangesAsync();


            return RedirectToAction(
                nameof(Dashboard)
            );
        }


        // ---------------------------------------------------------
        // HR LIST
        // ---------------------------------------------------------

        // GET: /Admin/HRList
        public IActionResult HRList()
        {
            var result = context.HRModels
                .Include(e => e.User)
                .ToList();

            return View(result);
        }


        // ---------------------------------------------------------
        // DELETE HR
        // ---------------------------------------------------------

        // GET: /Admin/DeleteHR
        public IActionResult DeleteHR(int id)
        {
            var result = context.HRModels
                .Include(e => e.User)
                .FirstOrDefault(e => e.UserId == id);


            if (result == null)
                return NotFound();


            // Delete associated User account
            if (result.User != null)
            {
                context.Users.Remove(result.User);
            }


            // Delete HR profile
            context.HRModels.Remove(result);

            context.SaveChanges();


            return RedirectToAction(
                nameof(HRList)
            );
        }


        // ---------------------------------------------------------
        // UPDATE HR - GET
        // ---------------------------------------------------------

        // GET: /Admin/UpdateHr
        [HttpGet]
        public async Task<IActionResult> UpdateHr(int id)
        {
            var hr = await context.HRModels
                .Include(h => h.User)
                .FirstOrDefaultAsync(
                    h => h.Id == id
                );


            if (hr == null)
                return NotFound();


            return View(hr);
        }


        // ---------------------------------------------------------
        // UPDATE HR - POST
        // ---------------------------------------------------------

        // POST: /Admin/UpdateHr
        [HttpPost]
        public async Task<IActionResult> UpdateHr(
            int id,
            HRModel model)
        {
            if (!ModelState.IsValid)
                return View(model);


            var hr = await context.HRModels
                .Include(h => h.User)
                .FirstOrDefaultAsync(
                    h => h.Id == id
                );


            if (hr == null)
                return NotFound();


            // -----------------------------------------------------
            // Update HR Profile
            // -----------------------------------------------------

            hr.FirstName =
                model.FirstName;

            hr.LastName =
                model.LastName;

            hr.Qualification =
                model.Qualification;

            hr.Experience =
                model.Experience;


            // -----------------------------------------------------
            // Update User Email
            // -----------------------------------------------------

            if (hr.User != null)
            {
                hr.User.Email =
                    model.User?.Email;
            }


            await context.SaveChangesAsync();


            return RedirectToAction(
                nameof(HRList)
            );
        }

        #endregion
    }
}