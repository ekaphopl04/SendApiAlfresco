

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DataBase.Models{

    public class Report {
        [Key]
        public string UserId { set; get ;}
        public string NamePath { set; get ;}
        public string Path { set; get ;}
        public string CustomerId { set; get ;}
        public string IdUpload { set; get ;}
        public string Type { set; get ;}
        public string TypeName { set; get ;}
        public bool Active  {set ; get ;} = false ;
    }
    
    public class ReportContext : DbContext {
        public DbSet<Report> ReportTable { set; get ;}

        public ReportContext(DbContextOptions<ReportContext> options) : base(options) {}
    }
}