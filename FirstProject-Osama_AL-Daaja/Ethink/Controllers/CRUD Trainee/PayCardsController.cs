using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Ethink.Encrypt;
using Ethink.Models;
using Ethink.Models.DTO;
using Ethink.Models.Input_Model;

namespace Ethink.Controllers.CRUD_Trainee
{
    [Authorize(Roles = "Trainee")]
    public class PayCardsController : Controller
    {
        private Context db = new Context();

        // GET: PayCards
        public ActionResult Index()
        {
            var payCard = db.PayCard.Where(a=>a.ApplicationUser.UserName == User.Identity.Name);
            return View(payCard.ToList());
        }

        // GET: PayCards/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            PayCard payCard = db.PayCard.Include(a=>a.PayLog).FirstOrDefault(a=>a.Id == id);

            return View(payCard);
        }

        // GET: PayCards/Create
        public ActionResult Create(int ? id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( PayCard payCard)
        {
            if (ModelState.IsValid)
            {
                payCard.IdTrainee = db.ApplicationUser.FirstOrDefault(a => a.UserName == User.Identity.Name).Id;
                payCard.CardCode = AesOperation.EncryptString(payCard.CardCode);
                db.PayCard.Add(payCard);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(payCard);
        }

        // GET: PayCards/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PayCard payCard = db.PayCard.Find(id);
            if (payCard == null)
            {
                return RedirectToAction("Index");
            }
            payCard.CardCode = "";
            return View(payCard);
        }

        // POST: PayCards/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Total,CardCode,IdTrainee")] PayCard payCard)
        {
            if(payCard.CardCode == "" || payCard.CardCode == null)
            {
                ViewBag.Status = "Make Sure Enter Code.";
                return View();
            }
            if (ModelState.IsValid)
            {
                var PayCard = db.PayCard.Find(payCard.Id);
                //var PayCard = db.PayCard.Find(payCard.Id);
                //if (payCard.CardCode != "")
                //{
                //    PayCard.CardCode = AesOperation.EncryptString(payCard.CardCode);
                //}

                //db.Entry(PayCard).State = EntityState.Modified;
                //db.SaveChanges();

                return RedirectToAction("CardCodeConfirm",new { Email = PayCard.ApplicationUser.Email,Code = payCard.CardCode,});
            }
            return View(payCard);
        }

        public ActionResult Delete(int id)
        {
            PayCard payCard = db.PayCard.Include(a=>a.PayLog).FirstOrDefault(a=>a.Id == id);

            if(payCard.Total > 0) { return RedirectToAction("Details", new { id = id }); }
            db.PayLog.RemoveRange(payCard.PayLog);
            db.PayCard.Remove(payCard);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }



        // GET: PayCards/Edit/5
        public ActionResult CardCodeConfirm(string Email,string Code)
        {
            if(Email == null || Code == null)
            {
                return RedirectToAction("Index");
            }
            MailMessage MyMessage = new MailMessage("t.u.world1996worldweb39@gmail.com", Email);

            var CodeEmail = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 10)
      .Select(s => s[new Random().Next(s.Length)]).ToArray());


            MyMessage.Body = "Code : " + CodeEmail;

            MyMessage.Subject = "Confirm Card Code";

            SmtpClient Client = new SmtpClient("smtp.gmail.com", 587);
            Client.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "t.u.world1996worldweb39@gmail.com",
                Password = "Osama1996"
            };

            Client.EnableSsl = true;
            Client.Send(MyMessage);

            IN_CardCodeConfirm dTOCardCodeConfirm = new IN_CardCodeConfirm()
            {
                Code = Code,
                CodeEmail = CodeEmail,
                Email = Email
            };

            return View(dTOCardCodeConfirm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CardCodeConfirm(IN_CardCodeConfirm model)
        {
            if (model.CodeEmail == model.ConfirmCode)
            {
                var PayCard = db.PayCard.FirstOrDefault(a => a.ApplicationUser.Email == model.Email);

                PayCard.CardCode = AesOperation.EncryptString(model.Code);

                db.Entry(PayCard).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            } else
            {
                ViewBag.Status = "Code Is Incorrect";
                return View(model);
            }

        }

        public ActionResult CreatePayLog(int ?id)
        {
            var PayCard = db.PayCard.FirstOrDefault(a => a.Id == id);
            if(PayCard == null) { return RedirectToAction("Index"); }
            var PayLog = new PayLog()
            {
                IdCard = PayCard.Id
            };
            return View(PayLog);
        }

        [HttpPost]
        public ActionResult CreatePayLog(PayLog model)
        {
            if (model.Value <= 0)
            {
                return RedirectToAction("Details", new { id = model.IdCard });
            }

            var Log = new PayLog()
            {
                Status = model.Status,
                Value = model.Value,
                IdCourseSection = null,
                IdCard = model.IdCard,
                Date = DateTime.Now,
            };

            if (!model.Status)
            {
                var PayCard = db.PayCard.FirstOrDefault(a => a.Id == model.IdCard);
                PayCard.Total += model.Value;
                db.Entry(PayCard).State = EntityState.Modified;
            }

            db.PayLog.Add(Log);
            db.SaveChanges();

            return RedirectToAction("Details", new { id = model.IdCard });
        }



    }
}
