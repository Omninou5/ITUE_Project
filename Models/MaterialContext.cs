using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ITUE.Models
{
    public class MaterialContext : DbContext
    {
        public DbSet<Material> Materials { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<Review> Reviews { get; set; }
    }
}