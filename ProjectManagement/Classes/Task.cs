using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagement.Classes
{
    public class Task
    {
        public string TaskName { get; set; }
        public DateTime TaskStartDate { get; set; }
        public DateTime TaskEndDate { get; set; }

        public Task(string taskName, DateTime taskStartDate, DateTime taskEndDate)
        {
            TaskName = taskName;
            TaskStartDate = taskStartDate;
            TaskEndDate = taskEndDate;
        }

        public string toString()
        {
            return $"{TaskName} | {TaskStartDate.Date.ToShortDateString()} - {TaskEndDate.Date.ToShortDateString()}";
        }
    }
}
