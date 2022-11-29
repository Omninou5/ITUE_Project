using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITUE.Models
{
    public class Material
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime? DatePublication { get; set; }
        public string TitleRus { get; set; }
        public string TitleEng { get; set; }
        public string Type { get; set; }
        public string Rubric { get; set; }
        public int? Pages { get; set; }
        public string AnnotationRus { get; set; }
        public string AnnotationEng { get; set; }
        public string KeywordsRus { get; set; }
        public string KeywordsEng { get; set; }
        public string Status { get; set; }


        public virtual ICollection<Author> Authors { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public Material()
        {
            Authors = new List<Author>();
            Reviews = new List<Review>();
        }

        public int? IssueId { get; set; }
        public virtual Issue Issue { get; set; }


        public int? Position { get; set; }
        public string UserId { get; set; }


    }

}