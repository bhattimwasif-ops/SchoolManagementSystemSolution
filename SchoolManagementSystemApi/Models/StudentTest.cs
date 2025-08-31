using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystemApi.Models
{
    public class StudentTest
    {
        [Key]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int TestId { get; set; }
        public string Subject { get; set; } = null!;
        public int TotalMarks { get; set; }
        public int ObtainedMarks { get; set; }
        public decimal Percentage { get; set; }
        public string Grade { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = null!; // New field for user ID or name
        public Student Student { get; set; } = null!;
        public Test Test { get; set; } = null!;
    }
}
