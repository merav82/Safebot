//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MVC
{
    using System;
    using System.Collections.Generic;
    
    public partial class EcGame
    {
        public int expNum { get; set; }
        public string workerId { get; set; }
        public string assignmentId { get; set; }
        public int gameId { get; set; }
        public bool isComplete { get; set; }
        public System.DateTime insertTime { get; set; }
        public bool isLearningMode { get; set; }
        public Nullable<System.DateTime> completeTime { get; set; }
        public Nullable<int> bonus { get; set; }
        public int tasksCompleted { get; set; }
        public string currentTask { get; set; }
        public bool finished { get; set; }
    }
}