using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagement.Classes
{
    public class Milestone
    {
        public string MilestoneName { get; set; }
        public DateTime MilestoneStartDate { get; set; }

        public DateTime MilestoneEndDate { get; set; }
        public List<Task> Tasks { get; set; } = new List<Task>();

        public Milestone(string milestoneName, DateTime milestoneStartDate, DateTime milestoneEndDate, List<Task> tasks)
        {
            MilestoneName = milestoneName;
            MilestoneStartDate = milestoneStartDate;
            MilestoneEndDate = milestoneEndDate;
            Tasks = tasks;
        }
    }
}
