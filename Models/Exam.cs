namespace ERP.Models
{
    public class Exam
    {
        public int ExamId { get; set; }
        public int JobId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;

        public JobOpening? JobOpening { get; set; }
    }
}
