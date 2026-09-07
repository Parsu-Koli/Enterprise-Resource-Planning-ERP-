using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Models
{
    public class Applicant
    {
        public int ApplicantId { get; set; }
        public int UserId { get; set; }
        public int JobId { get; set; }
        public string ResumePath { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public int Experience { get; set; }
        public int ExamScore { get; set; }
        public string Status { get; set; } = string.Empty;

        [NotMapped]
        public IFormFile? ResumeFile { get; set; }

        public User? User { get; set; }
        public JobOpening? JobOpening { get; set; }
    }
}
