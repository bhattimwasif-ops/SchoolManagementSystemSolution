using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystemApi.Models
{
    public class Test
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int ClassId { get; set; }
        public DateTime Date { get; set; }
        public Class Class { get; set; } = null!;
        public ICollection<StudentTest> StudentTests { get; set; } = new List<StudentTest>();
    }
}
