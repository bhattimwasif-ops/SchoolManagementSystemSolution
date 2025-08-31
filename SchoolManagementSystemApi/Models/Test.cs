using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystemApi.Models
{
    public class Test
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Type { get; set; } = "Test"; // "Test" or "Exam"
        public int ClassId { get; set; }
        public string Session { get; set; } = null!; // e.g., "2024-2025"
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime Date { get; set; } // Keep for consistency, can be removed if redundant
        public Class Class { get; set; } = null!;
        public ICollection<StudentTest> StudentTests { get; set; } = new List<StudentTest>();
    }
}
