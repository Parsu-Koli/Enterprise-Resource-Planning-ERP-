using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace ERP.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController(ERPDbContext context) : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult JobOpenings()
        {
            return View(context.JobOpenings.ToList());
        }

        public IActionResult CreateJob()
        {
            ViewBag.Departments = context.Departments.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult CreateJob(JobOpening job)
        {
            job.PostedDate = DateTime.Now;
            job.IsActive = true;
            context.JobOpenings.Add(job);
            context.SaveChanges();
            return RedirectToAction(nameof(JobOpenings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteJobVacancy(int jobid)
        {
            var job = context.JobOpenings.Find(jobid);
            if (job == null)
                return NotFound();

            context.JobOpenings.Remove(job);
            context.SaveChanges();

            return RedirectToAction(nameof(JobOpenings));
        }



        public IActionResult Applicants()
        {
            var data = context.Applicants
                .Include(a => a.User)
                .Include(a => a.JobOpening)
                .ToList();

            return View(data);
        }

        public IActionResult AddExam(int jobId)
        {
            return View(new Exam { JobId = jobId });
        }

        [HttpPost]
        public IActionResult AddExam(Exam exam)
        {
            context.Exams.Add(exam);
            context.SaveChanges();
            return RedirectToAction(nameof(JobOpenings));
        }

        public IActionResult Payrolls()
        {
            return View(context.PayRolls
                .Include(e => e.Employee)
                .ToList());
        }

        public IActionResult GeneratePayroll(int employeeId)
        {
            var emp = context.Employees.Find(employeeId);

            if (emp == null)
                return NotFound();

            var payroll = new PayRoll
            {
                EmployeeId = emp.EmployeeId,
                BasicSalary = emp.Salary,
                Allowances = 2000,
                Deductions = 500,
                NetSalary = emp.Salary + 2000 - 500,
                GeneratedDate = DateTime.Now
            };

            context.PayRolls.Add(payroll);
            context.SaveChanges();

            return RedirectToAction(nameof(Payrolls));
        }

        [HttpGet]
        public IActionResult CreateEmployee(int applicantId)
        {
            var applicant = context.Applicants
                .Include(a => a.User)
                .FirstOrDefault(a => a.ApplicantId == applicantId);

            if (applicant == null)
                return NotFound();

            ViewBag.Departments = context.Departments.ToList();
            ViewBag.ApplicantId = applicantId;

            var employee = new Employee
            {
                Email = applicant.User!.Email,
                DateOfJoining = DateTime.Now
            };

            return View(employee);
        }


        [HttpPost]
        public IActionResult CreateEmployee(Employee employee, int applicantId)
        {
            var applicant = context.Applicants
                .Include(a => a.User)
                .FirstOrDefault(a => a.ApplicantId == applicantId);

            if (applicant == null || applicant.User == null)
                return NotFound("User not found for applicant");

            // Prevent duplicate employee
            var existingEmployee = context.Employees
                .FirstOrDefault(e => e.UserId == applicant.User.UserId);

            if (existingEmployee != null)
                return BadRequest("This applicant is already an employee.");

            employee.UserId = applicant.User.UserId;
            employee.Email = applicant.User.Email;

            applicant.Status = "Selected";
            applicant.User.Role = "Employee";

            context.Employees.Add(employee);

            context.SaveChanges();

            return RedirectToAction(nameof(Applicants));
        }


        public IActionResult ViewEmployee(int id)
        {
            var emp = context.Employees
                .Include(e => e.Department)
                .Include(e => e.User)
                .FirstOrDefault(e => e.UserId == id);

            if (emp == null)
                return NotFound("Employee not found.");

            return View(emp);
        }

        [HttpGet]
        public IActionResult EditEmployee(int id) // id = UserId
        {
            var emp = context.Employees
                .Include(e => e.User)
                .FirstOrDefault(e => e.UserId == id); // ✅ FIX

            if (emp == null)
                return NotFound();

            ViewBag.Departments = context.Departments.ToList();
            return View(emp);
        }


        [HttpPost]
        public IActionResult EditEmployee(Employee employee)
        {
            var existing = context.Employees
                .Include(e => e.User)
                .FirstOrDefault(e => e.EmployeeId == employee.EmployeeId);

            if (existing == null)
                return NotFound();

            existing.FirstName = employee.FirstName;
            existing.LastName = employee.LastName;
            existing.DepartmentId = employee.DepartmentId;
            existing.Designation = employee.Designation;
            existing.Email = employee.Email;
            existing.Phone = employee.Phone;
            existing.Salary = employee.Salary;
            existing.Gender = employee.Gender;
            existing.BirthDate = employee.BirthDate;

            context.SaveChanges();

            return RedirectToAction("ViewEmployee", new { id = employee.EmployeeId });
        }
        public IActionResult SendExam(int applicantId)
        {
            var applicant = context.Applicants
                .Include(a => a.User)
                .Include(a => a.JobOpening)
                .FirstOrDefault(a => a.ApplicantId == applicantId);

            if (applicant == null || applicant.User?.Email == null)
                return NotFound();

            var examLink = Url.Action(
                "TakeExam",
                "Exam",
                new { applicantId },
                Request.Scheme);

            var mail = new Mail
            {
                SenderId = int.Parse(User.FindFirst("UserId")!.Value),
                ReceiverId = applicant.UserId,
                Subject = "Online Exam Invitation",
                Body = $"Please attend your exam using the link below:\n{examLink}",
                SentDate = DateTime.Now
            };


            SendRealEmail(applicant.User.Email, mail.Subject, mail.Body);


            context.Mails.Add(mail);
            context.SaveChanges();

            return RedirectToAction("Applicants");
        }
        private void SendRealEmail(string toEmail, string subject, string body)
        {
            var smtp = new SmtpClient
            {
                Host = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()["Smtp:Host"]!,
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    "koliprashant11112@gmail.com",
                    "vdngpzmyrwtoapbk"
                )
            };

            var message = new MailMessage
            {
                From = new MailAddress("koliprashant11112@gmail.com", "ERP System"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            smtp.Send(message);
        }

        public IActionResult Employees()
        {
            var employees = context.Employees
                .Include(e => e.User)
                .Include(e => e.Department)
                .ToList();

            return View(employees);
        }

        public async Task<IActionResult> GetPayrollByEmployee(int employeeId)
        {
            var payrolls = await context.PayRolls
                .Include(p => p.Employee)
                .Where(p => p.EmployeeId == employeeId)
                .ToListAsync();

            return View(payrolls);
        }




    }
}
