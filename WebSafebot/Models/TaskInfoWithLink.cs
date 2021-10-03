using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC.Models
{
    public class TaskInfoWithLink : TaskInfoModel
    {
        public string Link { set; get; }
        public string Culture { set; get; }
        public string HitCode { get; set; }
    }
}