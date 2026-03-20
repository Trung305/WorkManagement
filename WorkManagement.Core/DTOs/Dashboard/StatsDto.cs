using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkManagement.Core.DTOs.Dashboard
{
    public class StatsDto
    {
        // Tổng quan
        public int TotalTasks { get; set; }
        public int Pending { get; set; }
        public int InProgress { get; set; }
        public int PendingReview { get; set; }
        public int Completed { get; set; }
        public int Rejected { get; set; }

        // Đúng hạn vs quá hạn
        public int OnTime { get; set; }
        public int Overdue { get; set; }

        // Hoàn thành theo tuần (7 tuần gần nhất)
        public List<WeeklyStatsDto> WeeklyCompleted { get; set; } = new();

        // Hiệu suất thành viên
        public List<MemberStatsDto> MemberStats { get; set; } = new();
    }

    public class WeeklyStatsDto
    {
        public string Label { get; set; } = "";  // "T2", "T3"... hoặc "Tuần 1"
        public int Completed { get; set; }
        public int Assigned { get; set; }
    }

    public class MemberStatsDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public int Total { get; set; }
        public int Completed { get; set; }
        public int InProgress { get; set; }
        public int Overdue { get; set; }
        public double CompletionRate => Total > 0 ? Math.Round((double)Completed / Total * 100, 1) : 0;
    }
}
