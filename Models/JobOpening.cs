namespace ERP.Models
{
    public class JobOpening
    {
        public int JobId { get; set; }
        public int DepartmentId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public int ExperienceRequired { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime PostedDate { get; set; }
        public bool IsActive { get; set; }

        public Department? Department { get; set; }
    }
}
