using ITUE.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ITUE.Controllers
{
    [Authorize(Roles = "maineditor, admin")]
    public class NewsController : Controller
    {
        private MaterialContext db = new MaterialContext();


        public ActionResult Index()
        {
            var news = db.News;
            return View(news);
        }



        // Добавление новости в БД
        public ActionResult CreateNews()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateNews(News news)
        {
            news.Date = DateTime.Now;
            news.Status = "New";
            // Добавление данных в БД
            db.News.Add(news);
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        // Редактирование новости
        public ActionResult EditNews(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            News news = db.News.Find(id);
            if (news == null)
            {
                return HttpNotFound();
            }
            return View(news);
        }
        [HttpPost]
        public ActionResult EditNews(News news)
        {
            news.Date = DateTime.Now;
            news.Status = "New";
            db.Entry(news).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        // Подробный обзор новости
        public ActionResult DetailsNews(int id = 0)
        {
            News news = db.News.Find(id);
            if (news == null)
            {
                return HttpNotFound();
            }
            return View(news);
        }



        // Удаление новости
        [HttpGet]
        public ActionResult DeleteNews(int id)
        {
            News news = db.News.Find(id);
            if (news == null)
            {
                return HttpNotFound();
            }
            return View(news);
        }
        [HttpPost, ActionName("DeleteNews")]
        public ActionResult DeleteNewsConfirmed(int id)
        {
            News news = db.News.Find(id);
            if (news == null)
            {
                return HttpNotFound();
            }
            db.News.Remove(news);
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        // Список актуальных новостей
        public ActionResult ActualNews()
        {
            var news = db.News.Where(m => m.Status == "New").OrderByDescending(m => m.Date).ToList();
            return View(news);
        }



        // Список неактуальных новостей
        public ActionResult IrrelevantNews()
        {
            var news = db.News.Where(m => m.Status == "Old").OrderByDescending(m => m.Date).ToList();
            return View(news);
        }



        [AllowAnonymous]
        // Новости на главной странице
        public ActionResult MainNews()
        {
            var news = db.News.Where(m => m.Status == "New").OrderByDescending(m => m.Date).ToList();
            return PartialView(news);
        }




        [AllowAnonymous]
        // Новости на главной странице
        public ActionResult MainNewsDetails(int id)
        {
            News news = db.News.Find(id);
            if (news == null)
            {
                return HttpNotFound();
            }
            return View(news);
        }






        // Изменение статуса новости
        public ActionResult ChangeStatus(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            News news = db.News.Find(id);
            if (news == null)
            {
                return HttpNotFound();
            }
            return View(news);
        }
        [HttpPost, ActionName("ChangeStatus")]
        public ActionResult ChangeStatusConfirmed(int? id, string Status)
        {
            News news = db.News.Find(id);
            if (news == null)
            {
                return HttpNotFound();
            }
            news.Status = Status;
            db.Entry(news).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }





    }
}