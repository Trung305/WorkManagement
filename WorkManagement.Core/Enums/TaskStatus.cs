using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkManagement.Core.Enums
{
    public enum TaskStatus
    {
        Pending = 1,
        InProgress = 2,
        PendingReview = 3,
        Completed = 4,
        Rejected =5
    }
}
