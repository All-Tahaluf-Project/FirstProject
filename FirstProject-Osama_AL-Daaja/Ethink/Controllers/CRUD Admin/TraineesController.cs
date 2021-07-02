using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Ethink.Models;

namespace Ethink.Controllers.CRUD_Admin
{
    [Authorize(Roles = "Admin")]
    public class TraineesController : Controller
    {
        private Context db = new Context();

        // GET: Trainees
        public ActionResult Index(string UserName , string Email,DateTime? Date)
        {
            if(UserName != "" && UserName != null)
            {
                return View(db.ApplicationUser.Where(a => a.UserName.Contains(UserName) && a.UserRole.Any(r => r.IdRole == 2)).ToList());
            }


            if (Email != "" && Email != null)
            {
                return View(db.ApplicationUser.Where(a => a.Email.Contains(Email) && a.UserRole.Any(r => r.IdRole == 2)).ToList());
            }


            if (Date != null)
            {
                return View(db.ApplicationUser.Where(a => 
                a.Course_Trainee.Any(c=>c.CourseSections.Course.EndDate <= Date)
                //&& a.TraineeExam.Where(t=>t.Mark > )
                //&& a.Course_Trainee.Any(t=>t.CourseSections.Exam.Any(e=> e.TraineeExam.Average(ee=>ee.Mark) >(e.FullMark/2) && e.TraineeExam.Any(te=>te.IdTrainee == a.Id)))
                //&& a.TraineeExam.Average(t=>t.Mark) >= (a.TraineeExam.FirstOrDefault().Exam.FullMark /2)
                && db.Exam.Any(e=>e.FullMark/ 2 < e.TraineeExam.Where(t=>t.IdTrainee == a.Id).Sum(t=>t.Mark) && e.CourseSections.Course_Trainee.Any(t=>t.IdTrainee == a.Id))
                && a.UserRole.Any(r => r.IdRole == 2)).ToList());
            }

            return View(new List<ApplicationUser>());

            //return View(db.ApplicationUser.Where(a=>a.UserRole.Any(r=>r.IdRole == 2)).ToList());
        }

        // GET: Trainees/Details/5
        public ActionResult Details(int? id)
        {
            var Trainee = db.ApplicationUser.Include(a => a.Certificates).Include(a => a.Course_Trainee).Include(a => a.PayCard).FirstOrDefault(a => a.Id == id);

            if (Trainee == null) { return RedirectToAction("Index"); }

            return View(Trainee);
        }

        // GET: Trainees/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            ApplicationUser applicationUser = db.ApplicationUser.Find(id);
            if (applicationUser == null)
            {
                return RedirectToAction("Index");
            }
            return View(applicationUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationUser model)
        {
            //if (ModelState.IsValid)
            //{
            //    db.Entry(applicationUser).State = EntityState.Modified;
            //    db.SaveChanges();
            //    return RedirectToAction("Index");
            //}
            //return View(applicationUser);

            var ApplicationUser = db.ApplicationUser.FirstOrDefault(a => a.Id == model.Id);
            if(ApplicationUser == null) { return RedirectToAction("Index"); }

            if (model.UserName != ApplicationUser.UserName)
            {
                var Check = db.ApplicationUser.FirstOrDefault(a => a.UserName == model.UserName);
                if (Check != null) { ViewBag.Status = "UserName Is Used"; return View(model); }
                ApplicationUser.UserName = model.UserName;
            }

            if (model.Email != ApplicationUser.Email)
            {
                var Check = db.ApplicationUser.FirstOrDefault(a => a.Email == model.Email);
                if (Check != null) { ViewBag.Status = "Email Is Used"; return View(model); }
                ApplicationUser.Email = model.Email;
            }

            ApplicationUser.Gender = model.Gender;
            ApplicationUser.NickName = model.NickName;
            ApplicationUser.PhoneNumber = model.PhoneNumber;

            db.Entry(ApplicationUser).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            ApplicationUser applicationUser = db.ApplicationUser.Include(a=>a.TraineeExam).Include(a=>a.PayCard).Include(a=>a.UserRole)
                .Include(a=>a.Course_Trainee).Include(a=>a.Certificates).Include(a=>a.RequestRegister)
                .FirstOrDefault(a=>a.Id == id);
            if (applicationUser == null)
            {
                return RedirectToAction("Index");
            }

            foreach (var T in applicationUser.TraineeExam)
            {
                db.HistorySolutions.RemoveRange(T.HistorySolutions);
            }
            db.TraineeExam.RemoveRange(applicationUser.TraineeExam);
            foreach (var P in applicationUser.PayCard)
            {
                db.PayLog.RemoveRange(P.PayLog);
            }
            db.PayCard.RemoveRange(applicationUser.PayCard);
            db.UserRole.RemoveRange(applicationUser.UserRole);
            db.Course_Trainee.RemoveRange(applicationUser.Course_Trainee);
            if(applicationUser.Certificates.Any())
            {
                foreach(var C in applicationUser.Certificates)
                {
                    var OldPathCertificate = Path.Combine(Server.MapPath("~/Assets/Certificates"), C.Name);
                    if (!Directory.Exists(OldPathCertificate))
                    {
                        System.IO.File.Delete(OldPathCertificate);
                    }
                }
            }
            db.Certificates.RemoveRange(applicationUser.Certificates);
            db.RequestRegister.RemoveRange(applicationUser.RequestRegister);
            db.ApplicationUser.Remove(applicationUser);

            var OldPathImage = Path.Combine(Server.MapPath("~/Assets/Users"), applicationUser.ImageName);
            if (!Directory.Exists(OldPathImage))
            {
                System.IO.File.Delete(OldPathImage);
            }

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

        public ActionResult Block(int id)
        {
            var Trainee = db.ApplicationUser.Find(id);
            Trainee.Block = !Trainee.Block;

            db.Entry(Trainee).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
