using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC.Models
{
    public class EcTaskInfoGameMode : TaskInfoModel
    {
        public float bonusAmount { get; set; }
        public bool IsLearningMode { get; set; }
    }
}
