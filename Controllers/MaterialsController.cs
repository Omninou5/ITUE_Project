using ITUE.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.WebPages;

namespace ITUE.Controllers
{
    [Authorize(Roles = "technicaleditor, admin")]
    public class MaterialsController : Controller
    {
        private MaterialContext db = new MaterialContext();



        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }




        // Личный кабинет техн. редактора
        public ActionResult Index()
        {
            var material = db.Materials;
            return View(material);
        }













        // Все поступившие материалы
        public ActionResult SentMaterials()
        {
            var material = db.Materials.Where(m => m.Status != "Published" && m.Status != "Rejected" && m.Status != "Accepted").OrderByDescending(m => m.DatePublication).ToList();
            return View(material);
        }



        // Принятые к публикации материалы
        public ActionResult AcceptedMaterials()
        {
            var material = db.Materials.Where(m => m.Status == "Accepted").OrderByDescending(m => m.DatePublication).ToList();
            return View(material);
        }



        // Все опубликованные материалы
        public ActionResult PublishedMaterials()
        {
            var material = db.Materials.Where(m => m.Status == "Published").OrderByDescending(m => m.DatePublication).ToList();
            return View(material);
        }



        // Все отклоненные материалы
        public ActionResult RejectedMaterials()
        {
            var material = db.Materials.Where(m => m.Status == "Rejected").OrderByDescending(m=>m.DatePublication).ToList();
            return View(material);
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
            }
            return RedirectToAction("Index");
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

            switch (Status)
            {
                case "Accepted": // Принять статью к публикации
                    {
                        if (material.UserId != null)
                        {
                            // Определение почты у пользователя
                            ApplicationUser currentUser = UserManager.FindById(material.UserId);
                            // Электронная почта на которую приходит уведомление
                            string email = currentUser.Email;
                            // Заголовок Уведомления
                            string subject = "Ваша статья была принята к публикации";
                            // Тело уведомления
                            string text = material.TitleRus;
                            Notification(subject, text, email);
                        }
                        break;
                    }
                case "Correction": // Вернуть автору на исправление
                    {
                        if (material.UserId != null)
                        {
                            // Определение почты у пользователя
                            ApplicationUser currentUser = UserManager.FindById(material.UserId);
                            // Электронная почта на которую приходит уведомление
                            string email = currentUser.Email;
                            // Заголовок Уведомления
                            string subject = "Ваша статья возвращена на исправление";
                            // Тело уведомления
                            string text = material.TitleRus;
                            Notification(subject, text, email);
                        }
                        break;
                    }
                case "Rejected": // Отказать в публикации
                    {
                        if (material.UserId != null)
                        {
                            // Определение почты у пользователя
                            ApplicationUser currentUser = UserManager.FindById(material.UserId);
                            // Электронная почта на которую приходит уведомление
                            string email = currentUser.Email;
                            // Заголовок Уведомления
                            string subject = "Ваша статья была отказана в публикации";
                            // Тело уведомления
                            string text = material.TitleRus;
                            Notification(subject, text, email);
                        }
                        break;
                    }
                default:
                break;
            }

            return RedirectToAction("Index");
        }





        // Выбор необходимого рецензента
        public ActionResult SelectReviewer(int? id, string Status)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = Status;
            var user = UserManager.Users.ToList();
            return View(user);
        }
        [HttpPost, ActionName("SelectReviewer")]
        public ActionResult SelectReviewerConfirmed(int id, string Status, string UserId)
        {
            Material material = db.Materials.Find(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            material.Status = Status;
            db.Entry(material).State = EntityState.Modified;
            db.SaveChanges();
            // Создание строки в таблице Рецензии и присвоение ей некоторых значений
            Review review = new Review { MaterialId = material.Id, Result = "Review",  UserId = UserId };



            // УВЕДОМЛЕНИЕ ДЛЯ РЕЦЕНЗЕНТА
            if (material.UserId != null)
            {
                // Определение почты у пользователя
                ApplicationUser currentUser = UserManager.FindById(UserId);
                // Электронная почта на которую приходит уведомление
                string email = currentUser.Email;
                // Заголовок Уведомления
                string subject = "Вам поступила статья на рецензирование";
                // Тело уведомления
                string text = material.TitleRus;
                Notification(subject, text, email);
            }




            db.Reviews.Add(review);
            db.SaveChanges();
            return RedirectToAction("Index");
        }




        // Формирование номера журнала
        [HttpGet]
        public ActionResult Formation()
        {
            var material = db.Materials.Include(m => m.Issue).Where(m => m.Status == "Accepted").ToList();
            return View(material);
        }
        [HttpPost]
        public ActionResult Formation(HttpPostedFileBase upload, IEnumerable<HttpPostedFileBase> uploads, Issue issue, int[] selectedMaterials)
        {
            // Отправка файла на сервер
            if (upload != null)
            {
                // Получение полного имени файла
                string fileName = Path.GetFileName(upload.FileName);
                // Сохранение полного имени файла в БД
                issue.FileName = fileName;
                // Сохранение даты загрузки файла в БД
                issue.DatePublication = DateTime.Now;
                // Добавление данных в БД
                db.Issues.Add(issue);
                db.SaveChanges();

                // Сброс счетчика позиции статьи
                int i = 1;
                Material newMaterial;
                foreach (var item in selectedMaterials)
                {
                    newMaterial = db.Materials.Find(item);
                    // Счетчик для загрузки новых статей
                    int j = i - 1;

                    var article = uploads.ElementAt(j);
                    string articleFileName = article.FileName;
                    //string articleFileName = System.IO.Path.GetFileName(uploads.ElementAt(j).FileName);
                    //string articleFileName = uploads.ElementAt(i).FileName;
                    newMaterial.FileName = articleFileName;
                    // сохраняем новый файл статьи PDF в папку статей
                    article.SaveAs(Server.MapPath("~/Files/Articles/" + newMaterial.Id + "/" + articleFileName));

                    newMaterial.Status = "Published";
                    newMaterial.Position = i++;
                    newMaterial.IssueId = issue.Id;
                    db.Entry(newMaterial).State = EntityState.Modified;





                    // УВЕДОМЛЕНИЕ ДЛЯ АВТОРОВ
                    if (newMaterial.UserId != null)
                    {
                        // Определение почты у пользователя
                        ApplicationUser currentUser = UserManager.FindById(newMaterial.UserId);
                        // Электронная почта на которую приходит уведомление
                        string email = currentUser.Email;
                        // Заголовок Уведомления
                        string subject = "Ваша статья была опубликована в номере журнала";
                        // Тело уведомления
                        string text = newMaterial.TitleRus;
                        Notification(subject, text, email);
                    }





                }
                db.SaveChanges();

                // Получение идентификатора публикации
                string identificator = Convert.ToString(issue.Id);
                // Создание директории с папкой id
                Directory.CreateDirectory(Server.MapPath("~/Files/Issues/") + identificator);
                // сохранение файла на сервере
                upload.SaveAs(Server.MapPath("~/Files/Issues/" + identificator + "/" + fileName));
            }
            return RedirectToAction("Index");
        }




        // Подробности статьи на рецензировании
        public ActionResult DetailsReview(int id = 0)
        {
            Material material = db.Materials.Find(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            return View(material);
        }



        // Получение файла с сервера
        [AllowAnonymous]
        public FileResult GetFile(int id)
        {
            Material material = db.Materials.Find(id);
            string file_name = material.FileName;
            // Путь к файлу
            string file_path = Server.MapPath("~/Files/Articles/" + Convert.ToString(id) + "/" + file_name);
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