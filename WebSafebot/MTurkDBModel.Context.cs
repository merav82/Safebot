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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MTurkDBEntities : DbContext
    {
        public MTurkDBEntities()
            : base("name=MTurkDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<EcGame> EcGames { get; set; }
        public virtual DbSet<EcWorker> EcWorkers { get; set; }
        public virtual DbSet<GnException> GnExceptions { get; set; }
        public virtual DbSet<EcTasksCompleted> EcTasksCompleteds { get; set; }
        public virtual DbSet<EcQuest> EcQuests { get; set; }
        public virtual DbSet<EcChat> EcChats { get; set; }
        public virtual DbSet<GnBonu> GnBonus { get; set; }
    }
}
