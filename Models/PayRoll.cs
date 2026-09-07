namespace ERP.Models
{
    public class PayRoll
    {
        
        public int PayRollId { get; set; }
        public int EmployeeId { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime GeneratedDate { get; set; }

        public Employee? Employee { get; set; }
    }
}
