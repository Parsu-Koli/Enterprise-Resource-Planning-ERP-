using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers
{
    [Authorize(Roles = "Applicant")]
    public class ApplicantController(ERPDbContext context) : Controller
    {
        public IActionResult Jobs()
        {
            return View(context.JobOpenings.Where(j => j.IsActive).ToList());
        }

        public IActionResult Apply(int jobId)
        {
            return View(new Applicant { JobId = jobId });
        }

        [HttpPost]
        public IActionResult Apply(Applicant applicant)
        {
            applicant.UserId = int.Parse(User.FindFirst("UserId")!.Value);
            applicant.Status = "Applied";

            if (applicant.ResumeFile != null)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/resumes");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() +
                               Path.GetExtension(applicant.ResumeFile.FileName);

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    applicant.ResumeFile.CopyTo(stream);
                }

                applicant.ResumePath = "/uploads/resumes/" + fileName;
            }

            context.Applicants.Add(applicant);
            context.SaveChanges();

            return RedirectToAction(nameof(Jobs));
        }

    }

}
