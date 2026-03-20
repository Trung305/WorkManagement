using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkManagement.Core.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        // Stats cards
        public int TotalTasks { get; set; }
        public int TodayTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int CompletedThisWeek { get; set; }
        public int CompletedLastWeek { get; set; }
        public double CompletionRate { get; set; }

        // Task lists
        public List<DashboardTaskDto> UpcomingTasks { get; set; } = new();
        public List<DashboardTaskDto> NearDeadlineTasks { get; set; } = new();

        // Calendar — ngày có task deadline
        public List<int> DeadlineDays { get; set; } = new();
        public int Today { get; set; }
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
    }

    public class DashboardTaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int Status { get; set; }
        public int Priority { get; set; }
        public DateTime? Deadline { get; set; }
        public string? AssignedToName { get; set; }
        public string? CreatedByName { get; set; }
        public bool IsOverdue => Deadline.HasValue && Deadline.Value < DateTime.UtcNow && Status < 4;
    }
}
