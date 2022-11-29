using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITUE.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Result { get; set; }
        public string UserId { get; set; }
       
        public int? MaterialId { get; set; }
        public virtual Material Material { get; set; }

    }
}