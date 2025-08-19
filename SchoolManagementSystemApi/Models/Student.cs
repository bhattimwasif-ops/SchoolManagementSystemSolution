using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystemApi.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        public int ClassId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string ParentEmail { get; set; } = null!;

        [Required]
        [Phone]
        [StringLength(20)]
        public string ParentPhone { get; set; } = null!;

        // Navigation property
        public Class Class { get; set; } = null!;
    }
}
