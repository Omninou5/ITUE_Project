using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITUE.Models
{
    public class Issue
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime? DatePublication { get; set; }
        public int Number { get; set; }
        public int Year { get; set; }

        public virtual ICollection<Material> Materials { get; set; }

        public Issue()
        {
            Materials = new List<Material>();
        }
    }
}