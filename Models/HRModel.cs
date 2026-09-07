using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Models
{
    public class HRModel
    {
        public int Id {  get; set; }
        public int UserId {  get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string ResumePath { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public int Experience { get; set; }

        [NotMapped]
        public IFormFile? ResumeFile { get; set; }
        public User? User { get; set; }

    }
}
