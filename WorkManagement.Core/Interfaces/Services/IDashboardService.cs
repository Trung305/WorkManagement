using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.DTOs.Dashboard;

namespace WorkManagement.Core.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync(int userId, int userRole);
        Task<StatsDto> GetDetailedStatsAsync(int userId, int userRole, DateTime? from = null, DateTime? to = null);
    }
}
