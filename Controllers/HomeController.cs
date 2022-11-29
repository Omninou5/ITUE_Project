using ITUE.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.WebPages;

namespace ITUE.Controllers
{
    public class HomeController : Controller
    {
        MaterialContext db = new MaterialContext();


        public ActionResult Index()
        {
            var issue = db.Issues.OrderBy(m => m.Year).OrderBy(m => m.Number).ToList();
            return View(issue);
        }



        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }



        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }



        public ActionResult Author()
        {
            return View();
        }



        public ActionResult Editorial()
        {
            return View();
        }



        public ActionResult Actual()
        {
            return View();
        }

       

        public ActionResult Search()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Search(string search)
        {
            var materials = db.Materials.Where(m => m.TitleRus.Contains(search) || m.TitleEng.Contains(search) || m.KeywordsRus.Contains(search) || m.KeywordsEng.Contains(search) || m.AnnotationRus.Contains(search) || m.AnnotationEng.Contains(search)).ToList();
            return PartialView(materials);
        }



        // Получение файла с сервера
        [AllowAnonymous]
        public FileResult GetFile()
        {
            string file_name = "itue-oformlenie_statey.doc";
            // Путь к файлу
            string file_path = Server.MapPath("~/Files/Site/documents/itue-oformlenie_statey.doc");
            string file_type = "application/octet-stream";
            return File(file_path, file_type, file_name);
        }


    }
}