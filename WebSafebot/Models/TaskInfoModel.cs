using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC.Models
{
    public class TaskInfoModel
    {
        public string WorkerId { get; set; }
        public string AssignmentId { get; set; }
    }

    public class TaskInfoWithCulture : TaskInfoModel
    {
        public int Culture { get; set; }
    }
}
