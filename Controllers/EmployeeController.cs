using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController(ERPDbContext context) : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Profile()
        {
            int userId = int.Parse(User.FindFirst("UserId")!.Value);

            var employee = context.Employees
                                  .Include(e => e.Department)
                                  .FirstOrDefault(e => e.UserId == userId);

            if (employee == null)
                return Content("No Employee record found for this user.");

            var applicant = context.Applicants
                                   .FirstOrDefault(a => a.UserId == userId);

            ViewBag.ResumePath = applicant?.ResumePath;

            return View(employee);
        }



        public IActionResult MyLeaves()
        {
            return View(context.Leaves.ToList());
        }

        public IActionResult ApplyLeave()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ApplyLeave(Leave leave)
        {
            int userId = int.Parse(User.FindFirst("UserId")!.Value);

            // Find the employee linked to this user
            var employee = context.Employees.FirstOrDefault(e => e.UserId == userId);
            if (employee == null)
            {
                return Content("No employee record found. Please contact admin.");
            }

            // Set EmployeeId correctly
            leave.EmployeeId = employee.EmployeeId;
            leave.Status = "Pending";
            leave.AppliedDate = DateTime.Now;

            context.Leaves.Add(leave);
            context.SaveChanges();

            return RedirectToAction(nameof(MyLeaves));
        }


        public IActionResult MySalary()
        {
            var claim = User.FindFirst("UserId");

            if (claim == null)
                return Content("UserId claim not found in token.");

            int userId = int.Parse(claim.Value);

            var employeeId = context.Employees
                                    .Where(e => e.UserId == userId)
                                    .Select(e => e.EmployeeId)
                                    .FirstOrDefault();

            if (employeeId == 0)
            {
                return Content("No Employee record found for this logged-in user. Salary cannot be loaded.");
            }

            var salaries = context.PayRolls
                                  .Where(p => p.EmployeeId == employeeId)
                                  .OrderByDescending(p => p.GeneratedDate)
                                  .ToList();

            return View(salaries);
        }


    }

}
