using ITUE.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
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
    [Authorize (Roles = "user, admin")]
    public class AuthorController : Controller
    {

        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        private MaterialContext db = new MaterialContext();



        // Личный кабинет автора
        public ActionResult Index()
        {
            // Определение id пользователя
            ApplicationUser currentUser = UserManager.FindById(User.Identity.GetUserId());
            string currentUserId = currentUser.Id;
            var material = db.Materials.Where(m=>m.UserId == currentUserId).ToList();
            return View(material);
        }



        // Отправленные в редакцию материалы
        public ActionResult SentMaterials()
        {
            // Определение id пользователя
            ApplicationUser currentUser = UserManager.FindById(User.Identity.GetUserId());
            string currentUserId = currentUser.Id;
            var material = db.Materials.Where(m => m.Status == "Sent").Where(m => m.UserId == currentUserId);
            return View(material);
        }



        // Возвращенные автору материалы
        public ActionResult ReturnedMaterials()
        {
            // Определение id пользователя
            ApplicationUser currentUser = UserManager.FindById(User.Identity.GetUserId());
            string currentUserId = currentUser.Id;
            var material = db.Materials.Where(m => m.Status == "Correction" || m.Status == "ReviewerCorrection").Where(m => m.UserId == currentUserId);
            return View(material);
        }



        // Принятые к публикации материалы
        public ActionResult AcceptedMaterials()
        {
            // Определение id пользователя
            ApplicationUser currentUser = UserManager.FindById(User.Identity.GetUserId());
            string currentUserId = currentUser.Id;
            var material = db.Materials.Where(m => m.Status == "Accepted").Where(m => m.UserId == currentUserId);
            return View(material);
        }



        // Опубликованные материалы
        public ActionResult PublishedMaterials()
        {
            // Определение id пользователя
            ApplicationUser currentUser = UserManager.FindById(User.Identity.GetUserId());
            string currentUserId = currentUser.Id;
            var material = db.Materials.Where(m => m.Status == "Published").Where(m => m.UserId == currentUserId);
            return View(material);
        }



        // Отклоненные материалы
        public ActionResult RejectedMaterials()
        {
            // Определение id пользователя
            ApplicationUser currentUser = UserManager.FindById(User.Identity.GetUserId());
            string currentUserId = currentUser.Id;
            var material = db.Materials.Where(m => m.Status == "Rejected").Where(m => m.UserId == currentUserId);
            return View(material);
        }



        // Детали публикации
        public ActionResult Details(int id = 0)
        {
            Material material = db.Materials.Find(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            return View(material);
        }



        // Редактирование публикации
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            Material material = db.Materials.Find(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            return View(material);
        }
        [HttpPost]
        public ActionResult Edit(Material material)
        {
            db.Entry(material).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        // Добавление публикации в БД
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(HttpPostedFileBase upload, Material material)
        {
            // Отправка файла на сервер
            if (upload != null)
            {
                // Получение полного имени файла
                string fileName = Path.GetFileName(upload.FileName);
                // Сохранение полного имени файла в БД
                material.FileName = fileName;
                // Сохранение даты загрузки файла в БД
                material.DatePublication = DateTime.Now;
                // Присвоение статуса публикации
                material.Status = "Sent";
                // Поиск текущего пользователя
                ApplicationUser currentUser = UserManager.FindById(User.Identity.GetUserId());
                string currentUserId = currentUser.Id;

                material.UserId = currentUser.Id;
                var newAuthors = material.Authors.ToList();
                material.Authors.Clear();
                // Перебор всех авторов при создании
                foreach (var author in newAuthors)
                {
                    //Поиск в БД строки с таким же именем что и при создании
                    var authorBD = db.Authors.Where(m => m.Surname == author.Surname).Intersect(db.Authors.Where(m => m.Name == author.Name)).Intersect(db.Authors.Where(m => m.Patronymic == author.Patronymic)).Intersect(db.Authors.Where(m => m.Place == author.Place)).ToList();
                    //Если созданная запись есть в БД
                    if (authorBD.Count != 0)
                    {
                        material.Authors.Add(authorBD[0]);
                    }
                    else
                    {
                        material.Authors.Add(author);
                    }
                    authorBD = null;
                }
                // Добавление данных в БД
                db.Materials.Add(material);
                db.SaveChanges();
                // Получение идентификатора публикации
                string identificator = Convert.ToString(material.Id);
                // Создание директории с папкой id
                Directory.CreateDirectory(Server.MapPath("~/Files/Articles/") + identificator);
                // сохранение файла на сервере
                upload.SaveAs(Server.MapPath("~/Files/Articles/" + identificator + "/" + fileName));






                // УВЕДОМЛЕНИЕ ДЛЯ РЕДАКЦИИ
                // Электронная почта на которую приходит уведомление
                string email = "ITUEeditor@ya.ru";
                // Заголовок Уведомления
                string subject = "В журнал была загружена новая статья";
                // Список авторов
                string ListAuthor = null;
                foreach (var a in material.Authors)
                {
                    ListAuthor = ListAuthor + a.Name + " " + a.Surname + ", ";
                }
                // Тело уведомления
                string text = "&#171;" + material.TitleRus + "&#187;" + " / " + ListAuthor;
                Notification(subject, text, email);

                






            }
            return RedirectToAction("Index");
        }



        // Удаление данных из БД
        [HttpGet]
        public ActionResult Delete(int id)
        {
            Material material = db.Materials.Find(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            return View(material);
        }
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Material material = db.Materials.Find(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            db.Materials.Remove(material);
            db.SaveChanges();
            return RedirectToAction("Index");
        }




        // Изменение статуса материала
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
            return RedirectToAction("Index");
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
                            "<p style='margin-top:20px; font-size: 24px; margin-bottom:60px;'>" +
                            text +
                            "</p></td></tr><tr style='padding-top: 300px; border-top:1px solid grey;'><td>" +
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