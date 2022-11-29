using ITUE.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ITUE.Controllers
{
    public class IssueController : Controller
    {
        private MaterialContext db = new MaterialContext();



        // Список всех выпусков журнала
        public ActionResult Index()
        {
            var issue = db.Issues.OrderBy(m => m.Year).OrderBy(m=>m.Number).ToList();
            return View(issue);
        }



        // Номер журнала
        public ActionResult Issue(int id = 0)
        {
            Issue issue = db.Issues.Find(id);
            if (issue == null)
            {
                return HttpNotFound();
            }
            return View(issue);
        }


        
        // Статья номера
        public ActionResult Article(int id = 0)
        {
            Material material = db.Materials.Find(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            return View(material);
        }



        // Вывод актуальных статей на главную страницу
        public ActionResult MainPublished()
        {
            var item = db.Materials.Where(m => m.Status == "Published").Where(m => m.IssueId != null && m.Position != null).OrderByDescending(m => m.DatePublication).Take(5).ToList();
            return PartialView(item);
        }



        // Актуальный выпуск
        public ActionResult Actual(int id = 1)
        {
            Issue issue = db.Issues.OrderByDescending(m => m.DatePublication).First();
            return View(issue);
        }



        // Получение файла с сервера
        public FileResult GetFile(int id)
        {
            Issue issue = db.Issues.Find(id);
            string file_name = issue.FileName;
            // Путь к файлу
            string file_path = Server.MapPath("~/Files/Issues/" + Convert.ToString(id) + "/" + file_name);
            string file_type = "application/octet-stream";

            return File(file_path, file_type, file_name);
        }




        //[HttpPost]
        public ActionResult Keyword(string search)
        {
            var materials = db.Materials.Where(m => m.KeywordsRus.Contains(search) || m.KeywordsEng.Contains(search)).ToList();
            if (materials.Count() == 0)
            {
                return HttpNotFound();
            }
            else
            {
                return View(materials);
            }
            
        }


    }
}