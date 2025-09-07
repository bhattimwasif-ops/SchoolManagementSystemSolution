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


        [StringLength(255)]
        public string Image { get; set; } = null!;

        [StringLength(500)]
        public string Address { get; set; } = null!;

        [StringLength(50)]
        public string RollNo { get; set; } = null!;

        [StringLength(100)]
        public string FatherOccupation { get; set; } = null!;

        [StringLength(100)]
        public string GuardianName { get; set; } = null!;

        [StringLength(100)]
        public string GuardianOccupation { get; set; } = null!;

        [StringLength(100)]
        public string MotherName { get; set; } = null!;

        public DateTime? AdmissionDate { get; set; }

        [StringLength(100)]
        public string? CreateBy { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? DateOfBirth { get; set; }


        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }
        // Navigation property
        public Class Class { get; set; } = null!;
        public ICollection<StudentTest> StudentTests { get; set; } = new List<StudentTest>();

    }
}
