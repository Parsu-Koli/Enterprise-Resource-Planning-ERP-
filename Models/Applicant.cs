using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Models
{
    public class Applicant
    {
        #region Primary Information

        public int ApplicantId { get; set; }
        public int UserId { get; set; }
        public int JobId { get; set; }

        #endregion

        #region Applicant Details

        public string Qualification { get; set; } = string.Empty;
        public int Experience { get; set; }

        #endregion

        #region Resume

        public string ResumePath { get; set; } = string.Empty;

        [NotMapped]
        public IFormFile? ResumeFile { get; set; }

        #endregion

        #region Exam & Application Status

        public int ExamScore { get; set; }
        public string Status { get; set; } = string.Empty;

        #endregion

        #region Navigation Properties

        public User? User { get; set; }
        public JobOpening? JobOpening { get; set; }

        #endregion
    }
}