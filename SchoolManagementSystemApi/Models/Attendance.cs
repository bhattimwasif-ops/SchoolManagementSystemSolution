using System.Text.Json;
using System.Text.Json.Serialization;

namespace SchoolManagementSystemApi.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public string Status { get; set; } = "Present"; // Present, Absent, Late
                                                        // Cool feature: JSONB for flexible metadata (e.g., absence reason)
        ///[JsonExtensionData]  // For JSONB in PostgreSQL
        public JsonDocument? Metadata { get; set; }
    }
}
