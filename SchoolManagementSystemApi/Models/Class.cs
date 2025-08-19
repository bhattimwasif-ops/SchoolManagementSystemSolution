using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystemApi.Models
{
    public class Class
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ClassName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Section { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Teacher { get; set; } = null!;

        // Navigation property for students
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
