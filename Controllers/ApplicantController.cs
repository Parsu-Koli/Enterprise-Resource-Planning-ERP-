using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers
{
    [Authorize(Roles = "Applicant")]
    public class ApplicantController(ERPDbContext context) : Controller
    {
      
        #region Job Management

        // GET: /Applicant/Jobs
        public IActionResult Jobs()
        {
            var jobs = context.JobOpenings
                .Where(j => j.IsActive)
                .ToList();

            return View(jobs);
        }

        #endregion


        #region Job Application

        // GET: /Applicant/Apply
        public IActionResult Apply(int jobId)
        {
            var applicant = new Applicant
            {
                JobId = jobId
            };

            return View(applicant);
        }


        // POST: /Applicant/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Apply(Applicant applicant)
        {
            // -----------------------------------------------------
            // Get logged-in Applicant UserId
            // -----------------------------------------------------

            applicant.UserId =
                int.Parse(
                    User.FindFirst("UserId")!.Value
                );


            // -----------------------------------------------------
            // Set Application Status
            // -----------------------------------------------------

            applicant.Status = "Applied";


            // -----------------------------------------------------
            // Resume Upload
            // -----------------------------------------------------

            if (applicant.ResumeFile != null)
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
                        applicant.ResumeFile.FileName
                    );


                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName
                );


                // Save uploaded resume
                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    applicant.ResumeFile.CopyTo(stream);
                }


                // Save resume URL in database
                applicant.ResumePath =
                    "/uploads/resumes/" + fileName;
            }


            // -----------------------------------------------------
            // Save Application
            // -----------------------------------------------------

            context.Applicants.Add(applicant);

            context.SaveChanges();


            // -----------------------------------------------------
            // Redirect to Jobs
            // -----------------------------------------------------

            return RedirectToAction(
                nameof(Jobs)
            );
        }

        #endregion
    }
}