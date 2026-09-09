using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers
{
    [AllowAnonymous]
    public class ExamController(ERPDbContext context) : Controller
    {
        private readonly ERPDbContext context = context;

        #region Take Exam 
        public IActionResult TakeExam(int applicantId)
        {
            var applicant = context.Applicants
                .Include(a => a.JobOpening)
                .FirstOrDefault(a => a.ApplicantId == applicantId);

            if (applicant == null)
                return NotFound();

            var questions = context.Exams
                .Where(e => e.JobId == applicant.JobId)
                .ToList();

            ViewBag.ApplicantId = applicantId;
            return View(questions);
        }

        [HttpPost]
        public IActionResult SubmitExam(int applicantId, Dictionary<int, string> answers)
        {
            var applicant = context.Applicants
                .Include(a => a.User)
                .FirstOrDefault(a => a.ApplicantId == applicantId);

            if (applicant == null)
                return NotFound();

            var questions = context.Exams
                .Where(e => e.JobId == applicant.JobId)
                .ToList();

            int score = 0;

            foreach (var q in questions)
            {
                if (answers.TryGetValue(q.ExamId, out var ans) &&
                    ans == q.CorrectAnswer)
                {
                    score += 10;
                }
            }

            applicant.ExamScore = score;
            applicant.Status = score >= 10 ? "Selected" : "Rejected";

            var mail = new Mail
            {
                SenderId = applicant.UserId, 
                ReceiverId = applicant.UserId,
                Subject = "Exam Result",
                Body = $"Your score: {score}\nStatus: {applicant.Status}",
                SentDate = DateTime.UtcNow
            };

            context.Mails.Add(mail);
            context.SaveChanges();

            return RedirectToAction("Inbox", "Mail");
        }

        #endregion
    }
}
