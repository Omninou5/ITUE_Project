using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITUE.Models
{
    public class News
    {
        public int Id { get; set; }
        public string TitleRus { get; set; }
        public string TitleEng { get; set; }
        public string TextRus { get; set; }
        public string TextEng { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; }
    }
}