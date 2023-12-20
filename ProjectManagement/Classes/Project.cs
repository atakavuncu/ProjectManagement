using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagement.Classes
{
    public class Project
    {
        public string ProjectName { get; set; }
        public string ProjectNumber { get; set; }
        public string ProjectManagerName { get; set; }
        public string ProjectGoal { get; set; }
        public string Description { get; set; }
        public string Scope { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime ProjectStart { get; set; }
        public DateTime ProjectEnd { get; set; }
        public DateTime EstimatedStart { get; set; }
        public DateTime EstimatedEnd { get; set; }
        public string Status { get; set; }
        public int MonetaryReturn { get; set; }
        public string MonetaryReturnType { get; set; }
        public string ProjectDocuments { get; set; }
        public string ProjectType { get; set; }
    }

    public enum ProjectStatus
    {
        ToDo,
        ApprovalPending,
        InProgress,
        Done
    }

    public enum MonetaryReturnType
    {
        Daily,
        Monthly,
        Yearly
    }

    public enum ProjectType
    {
        Abroad,
        TUBITAK,
        KOBI
    }

}
