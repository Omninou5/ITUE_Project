using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITUE.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Place { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }

        public virtual ICollection<Material> Materials { get; set; }
        public Author()
        {
            Materials = new List<Material>();
        }
    }
}