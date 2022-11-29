using ITUE.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace ITUE.Controllers
{
    [Authorize(Roles = "reviewer, admin")]
    public class ReviewerController : Controller
    {
        private MaterialContext db = new MaterialContext();


        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }



        // Личный кабинет пользователя
        public ActionResult Index()
        {
            // Определение id пользователя
            ApplicationUser currentUser = UserManager.FindById(User.Identity.GetUserId());
            string currentUserId = currentUser.Id;
            var reviews = db.Reviews.Where(m => m.UserId == currentUserId).ToList();
            return View(reviews);
        }



        // Новые материалы на рецензирование
        public ActionResult ReviewArticles()
        {
            // Определение id пользователя
            ApplicationUser currentUser = UserManager.FindById(User.Identity.GetUserId());
            string currentUserId = currentUser.Id;
            var reviews = db.Reviews.Where(m => m.Result == "Review").Where(m => m.UserId == currentUserId).ToList();
            return View(reviews);
        }



        //  Отправленные рецензии
        public ActionResult SentReview()
        {
            // Определение id пользователя
            ApplicationUser currentUser = UserManager.FindById(User.Identity.GetUserId());
            string currentUserId = currentUser.Id;
            var reviews = db.Reviews.Where(m => m.Result == "ReviewerCorrection" || m.Result == "NotRecommended" || m.Result == "Recommended" || m.Result == "RefusalOfReview").Where(m => m.UserId == currentUserId).ToList();
            return View(reviews);
        }



        // Подробности рецензии
        public ActionResult Details(int? id)
        {
            Material material = db.Materials.Find(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            return View(material);
        }



        // Изменение статуса материала из-за рецензии
        public ActionResult ChangeStatus(int? id, string Status)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = Status;
            Material material = db.Materials.Find(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            return View(material);
        }
        [HttpPost, ActionName("ChangeStatus")]
        public ActionResult ChangeStatusConfirmed(int? id, string Status)
        {
            Material material = db.Materials.Find(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            material.Status = Status;
            db.Entry(material).State = EntityState.Modified;
            db.SaveChanges();




            // УВЕДОМЛЕНИЕ ДЛЯ ЖУРНАЛА
            // Электронная почта на которую приходит уведомление
            string email = "ITUEeditor@ya.ru";
            // Заголовок Уведомления
            string subject = "Поступила рецензия на статью";
            // Тело уведомления
            string text = material.TitleRus;
            Notification(subject, text, email);



            return RedirectToAction("Index");
        }




        // Отправка рецензии в БД
        public ActionResult Sending(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }
        [HttpPost]
        public ActionResult Sending(HttpPostedFileBase upload, Review review)
        {
            // Отправка файла на сервер
            if (upload != null)
            {
                // Получение полного имени файла
                string fileName = Path.GetFileName(upload.FileName);
                // Сохранение полного имени файла в БД
                review.FileName = fileName;
                // Присвоение статуса публикации
                review.Result = review.Result;
                // Изменение данных в таблице рецензий БД
                db.Entry(review).State = EntityState.Modified;

                // Изменение статуса материала
                Material material = db.Materials.Find(review.MaterialId);
                material.Status = review.Result;
                db.Entry(material).State = EntityState.Modified;

                db.SaveChanges();
                // Получение идентификатора публикации
                string identificator = Convert.ToString(review.Id);
                // Создание директории с папкой id
                Directory.CreateDirectory(Server.MapPath("~/Files/Review/") + identificator);
                // сохранение файла на сервере
                upload.SaveAs(Server.MapPath("~/Files/Review/" + identificator + "/" + fileName));





                // УВЕДОМЛЕНИЕ ДЛЯ ЖУРНАЛА
                // Электронная почта на которую приходит уведомление
                string email = "ITUEeditor@ya.ru";
                // Заголовок Уведомления
                string subject = "Поступила рецензия на статью";
                // Тело уведомления
                string text = material.TitleRus;
                Notification(subject, text, email);




            }
            return RedirectToAction("Index");
        }




        // Отказ от рецензирования
        public ActionResult Refusal(int? id, string Result)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = Result;
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }
        [HttpPost, ActionName("Refusal")]
        public ActionResult RefusalConfirmed(int? id, string Result)
        {
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            review.Result = Result;
            db.Entry(review).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        // Получение файла с сервера
        [AllowAnonymous]
        public FileResult GetFile(int id)
        {
            Review review = db.Reviews.Find(id);
            string file_name = review.FileName;
            // Путь к файлу
            string file_path = Server.MapPath("~/Files/Review/" + Convert.ToString(id) + "/" + file_name);
            string file_type = "application/octet-stream";
            return File(file_path, file_type, file_name);
        }




        // Отправка письма-уведомления на почту
        public ActionResult Notification(string subject, string text, string email)
        {
            // Почта редакции (отправитель) - устанавливаем адрес и отображаемое в письме имя
            MailAddress from = new MailAddress("ITUEeditor@ya.ru", "Редакция журнала ИТУЭ");
            // кому отправляем
            MailAddress to = new MailAddress(email);
            // создаем объект сообщения
            MailMessage m = new MailMessage(from, to);
            // тема письма
            m.Subject = subject;
            // текст письма
            m.Body = "<table style='width:100%'><tr style='width:100%'><td style='padding:30px 20px 20px' align='center' bgcolor='#fff'><img alt='ITUE' src='https://itue6.azurewebsites.net/Files/Site/image/logo2.gif' style='display:inline-block; width:40%; max-width:70%'></td></tr><tr align='center' style='width:100%'><td>" +
                            "<h2 style='color: #000000; font-weight: 300; margin-top:50px;'>" +
                             subject +
                            "</h2></td></tr><tr align='center' style='width:100%;'><td>" +
                            "<p style='margin-top:20px; font-size: 24px; margin-bottom:60px;'> &#171;" +
                            text +
                            "&#187; </p></td></tr><tr style='padding-top: 300px; border-top:1px solid grey;'><td>" +
                            "<p style='color: #a4a4a4; font-size: 12px; line-height: 16px; text-align: center'>© 2020 - Информационные технологии в управлении и экономике.<br>ФГБОУ ВО «Ухтинский государственный технический университет»</p></td></tr></table>";
            // письмо представляет код html
            m.IsBodyHtml = true;
            // адрес smtp-сервера и порт, с которого будет отправляться письмо
            SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 587);
            // логин и пароль
            smtp.Credentials = new NetworkCredential("ITUEeditor@ya.ru", "itueitueitue");
            smtp.EnableSsl = true;
            smtp.Send(m);
            return View("Index");
        }



    }
}