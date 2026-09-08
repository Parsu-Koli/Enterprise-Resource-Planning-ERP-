namespace ERP.Models
{
    public class Department
    {
        #region Department Information

        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        #endregion

        #region Audit Information

        public DateTime CreatedDate { get; set; }

        #endregion
    }
}