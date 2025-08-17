using Microsoft.EntityFrameworkCore;
using SchoolManagementSystemApi.Data;

namespace SchoolManagementSystemApi.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public ReportService(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task GenerateMonthlyReport()
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            var absences = await _context.Attendances
                .Where(a => a.Date >= startOfMonth && a.Date < endOfMonth && a.Status == "Absent")
                .GroupBy(a => a.StudentId)
                .Select(g => new { StudentId = g.Key, AbsentDays = g.Count() })
                .ToListAsync();

            foreach (var absence in absences)
            {
                var student = await _context.Students.FindAsync(absence.StudentId);
                if (student != null)
                {
                    var report = $"Monthly Report for {student.Name}: Absent {absence.AbsentDays} days in {startOfMonth:MMMM yyyy}.";
                    _notificationService.SendEmail(student.ParentEmail, "Monthly Attendance Report", report);
                    // TODO: Add PDF generation (e.g., PdfSharp)
                }
            }
        }
    }
}