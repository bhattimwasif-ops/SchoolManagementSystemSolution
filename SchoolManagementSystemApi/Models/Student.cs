namespace SchoolManagementSystemApi.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string ParentEmail { get; set; } = string.Empty;
        public string ParentPhone { get; set; } = string.Empty;
    }
}
