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
        // =========================================================
        // DASHBOARD
        // =========================================================

        #region Dashboard

        public IActionResult Dashboard()
        {
            return View();
        }

        #endregion


        // =========================================================
        // JOB MANAGEMENT
        // =========================================================

        #region Job Management

        // GET: /HR/JobOpenings
        [AllowAnonymous]
        public IActionResult JobOpenings()
        {
            return View(context.JobOpenings.ToList());
        }


        // GET: /HR/CreateJob
        public IActionResult CreateJob()
        {
            ViewBag.Departments = context.Departments.ToList();

            return View();
        }


        // POST: /HR/CreateJob
        [HttpPost]
        public IActionResult CreateJob(JobOpening job)
        {
            job.PostedDate = DateTime.UtcNow;
            job.IsActive = true;

            context.JobOpenings.Add(job);
            context.SaveChanges();

            return RedirectToAction(nameof(JobOpenings));
        }


        // POST: /HR/DeleteJobVacancy
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

        #endregion


        // =========================================================
        // APPLICANT MANAGEMENT
        // =========================================================

        #region Applicant Management

        // GET: /HR/Applicants
        public IActionResult Applicants()
        {
            var applicants = context.Applicants
                .Include(a => a.User)
                .Include(a => a.JobOpening)
                .ToList();

            return View(applicants);
        }


        // GET: /HR/SendExam
        public IActionResult SendExam(int applicantId)
        {
            var applicant = context.Applicants
                .Include(a => a.User)
                .Include(a => a.JobOpening)
                .FirstOrDefault(a => a.ApplicantId == applicantId);

            if (applicant == null || applicant.User?.Email == null)
                return NotFound();

            // Generate exam URL
            var examLink = Url.Action(
                "TakeExam",
                "Exam",
                new { applicantId },
                Request.Scheme
            );

            // Create mail record
            var mail = new Mail
            {
                SenderId = int.Parse(User.FindFirst("UserId")!.Value),
                ReceiverId = applicant.UserId,
                Subject = "Online Exam Invitation",
                Body = $"Please attend your exam using the link below:\n{examLink}",
                SentDate = DateTime.UtcNow
            };

            // Send actual email
            SendRealEmail(
                applicant.User.Email,
                mail.Subject,
                mail.Body
            );

            // Save email record in database
            context.Mails.Add(mail);
            context.SaveChanges();

            return RedirectToAction(nameof(Applicants));
        }

        #endregion


        // =========================================================
        // EXAM MANAGEMENT
        // =========================================================

        #region Exam Management

        // GET: /HR/AddExam
        public IActionResult AddExam(int jobId)
        {
            return View(new Exam
            {
                JobId = jobId
            });
        }


        // POST: /HR/AddExam
        [HttpPost]
        public IActionResult AddExam(Exam exam)
        {
            context.Exams.Add(exam);
            context.SaveChanges();

            return RedirectToAction(nameof(JobOpenings));
        }

        #endregion


        // =========================================================
        // EMPLOYEE MANAGEMENT
        // =========================================================

        #region Employee Management

        // GET: /HR/Employees
        public IActionResult Employees()
        {
            var employees = context.Employees
                .Include(e => e.User)
                .Include(e => e.Department)
                .ToList();

            return View(employees);
        }


        // GET: /HR/CreateEmployee
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
                DateOfJoining = DateTime.UtcNow
            };

            return View(employee);
        }


        // POST: /HR/CreateEmployee
        [HttpPost]
        public IActionResult CreateEmployee(
            Employee employee,
            int applicantId)
        {
            var applicant = context.Applicants
                .Include(a => a.User)
                .FirstOrDefault(a => a.ApplicantId == applicantId);

            if (applicant == null || applicant.User == null)
                return NotFound("User not found for applicant");


            // -----------------------------------------------------
            // Prevent duplicate employee
            // -----------------------------------------------------

            var existingEmployee = context.Employees
                .FirstOrDefault(e =>
                    e.UserId == applicant.User.UserId);

            if (existingEmployee != null)
                return BadRequest(
                    "This applicant is already an employee."
                );


            // -----------------------------------------------------
            // Assign User information
            // -----------------------------------------------------

            employee.UserId = applicant.User.UserId;
            employee.Email = applicant.User.Email;


            // -----------------------------------------------------
            // Update applicant status
            // -----------------------------------------------------

            applicant.Status = "Selected";


            // -----------------------------------------------------
            // Change user role
            // -----------------------------------------------------

            applicant.User.Role = "Employee";


            // -----------------------------------------------------
            // Save employee
            // -----------------------------------------------------

            context.Employees.Add(employee);
            context.SaveChanges();

            return RedirectToAction(nameof(Applicants));
        }


        // GET: /HR/ViewEmployee
        public IActionResult ViewEmployee(int id)
        {
            var employee = context.Employees
                .Include(e => e.Department)
                .Include(e => e.User)
                .FirstOrDefault(e => e.UserId == id);

            if (employee == null)
                return NotFound("Employee not found.");

            return View(employee);
        }


        // GET: /HR/EditEmployee
        public IActionResult EditEmployee(int id)
        {
            var employee = context.Employees
                .Include(e => e.User)
                .FirstOrDefault(e => e.UserId == id);

            if (employee == null)
                return NotFound();

            ViewBag.Departments =
                context.Departments.ToList();

            return View(employee);
        }


        // POST: /HR/EditEmployee
        [HttpPost]
        public IActionResult EditEmployee(Employee employee)
        {
            var existingEmployee = context.Employees
                .Include(e => e.User)
                .FirstOrDefault(e =>
                    e.EmployeeId == employee.EmployeeId);

            if (existingEmployee == null)
                return NotFound();


            // -----------------------------------------------------
            // Update employee information
            // -----------------------------------------------------

            existingEmployee.FirstName =
                employee.FirstName;

            existingEmployee.LastName =
                employee.LastName;

            existingEmployee.DepartmentId =
                employee.DepartmentId;

            existingEmployee.Designation =
                employee.Designation;

            existingEmployee.Email =
                employee.Email;

            existingEmployee.Phone =
                employee.Phone;

            existingEmployee.Salary =
                employee.Salary;

            existingEmployee.Gender =
                employee.Gender;

            existingEmployee.BirthDate =
                employee.BirthDate;


            context.SaveChanges();

            return RedirectToAction(
                nameof(ViewEmployee),
                new
                {
                    id = employee.EmployeeId
                }
            );
        }

        #endregion


        // =========================================================
        // PAYROLL MANAGEMENT
        // =========================================================

        #region Payroll Management

        // GET: /HR/Payrolls
        public IActionResult Payrolls()
        {
            var payrolls = context.PayRolls
                .Include(p => p.Employee)
                .ToList();

            return View(payrolls);
        }


        // GET: /HR/GeneratePayroll
        public IActionResult GeneratePayroll(int employeeId)
        {
            var employee = context.Employees
                .Find(employeeId);

            if (employee == null)
                return NotFound();


            // -----------------------------------------------------
            // Calculate payroll
            // -----------------------------------------------------

            decimal basicSalary = employee.Salary;
            decimal allowances = 2000;
            decimal deductions = 500;

            decimal netSalary =
                basicSalary +
                allowances -
                deductions;


            // -----------------------------------------------------
            // Create payroll record
            // -----------------------------------------------------

            var payroll = new PayRoll
            {
                EmployeeId = employee.EmployeeId,

                BasicSalary = basicSalary,

                Allowances = allowances,

                Deductions = deductions,

                NetSalary = netSalary,

                GeneratedDate = DateTime.UtcNow
            };


            // -----------------------------------------------------
            // Save payroll
            // -----------------------------------------------------

            context.PayRolls.Add(payroll);
            context.SaveChanges();

            return RedirectToAction(nameof(Payrolls));
        }


        // GET: /HR/GetPayrollByEmployee
        public async Task<IActionResult> GetPayrollByEmployee(
            int employeeId)
        {
            var payrolls = await context.PayRolls
                .Include(p => p.Employee)
                .Where(p => p.EmployeeId == employeeId)
                .ToListAsync();

            return View(payrolls);
        }


        // GET: /HR/PayslipDetails
        public IActionResult PayslipDetails(int id)
        {
            var payroll = context.PayRolls
                .Include(p => p.Employee)
                    .ThenInclude(e => e!.Department)
                .FirstOrDefault(p =>
                    p.PayRollId == id);

            if (payroll == null)
                return NotFound();

            return View(payroll);
        }

        #endregion


        // =========================================================
        // EMAIL / SMTP
        // =========================================================

        #region Email / SMTP

        private void SendRealEmail(
            string toEmail,
            string subject,
            string body)
        {
            var smtp = new SmtpClient
            {
                Host = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    ["Smtp:Host"]!,

                Port = 587,

                EnableSsl = true,

                Credentials = new NetworkCredential(
                    "demo@gmail.com",
                    "xx"
                )
            };


            var message = new MailMessage
            {
                From = new MailAddress(
                    "demo@gmail.com",
                    "ERP System"
                ),

                Subject = subject,

                Body = body,

                IsBodyHtml = false
            };


            message.To.Add(toEmail);

            smtp.Send(message);
        }

        #endregion
    }
}