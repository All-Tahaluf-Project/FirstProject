using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Ethink.Models;
using MvcContrib.Filters;

namespace Ethink.Controllers.CRUD_Trainer
{
    [Authorize(Roles = "Trainer")]
    public class ExamsController : Controller
    {
        private Context db = new Context();

        // GET: Exams
        public ActionResult Index()
        {
            var exam = db.Exam.Include(e => e.CourseSections);
            return View(exam.ToList());
        }

        // GET: Exams/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("IndexCourses", "Trainer");
            }
            Exam exam = db.Exam.Include(a=>a.Questions).FirstOrDefault(a=>a.Id == id );
            if (exam == null)
            {
                return HttpNotFound();
            }
            return View(exam);
        }

        // GET: Exams/Create
        public ActionResult Create(int? Id)
        {
            var CourseSection = db.CourseSections.FirstOrDefault(a => a.Id == Id);

            if(CourseSection == null) { return RedirectToAction("IndexCourses", "Trainer"); }
            
            return View(new Exam() {IdCourseSections = CourseSection.Id });
        }

        // POST: Exams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( Exam exam)
        {
            if (ModelState.IsValid)
            {
                db.Exam.Add(exam);
                db.SaveChanges();
            }

            return RedirectToAction("Details","Trainer",new { id = exam.IdCourseSections});
        }

        // GET: Exams/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("IndexCourses", "Trainer");
            }
            Exam exam = db.Exam.Find(id);
            if (exam == null)
            {
                return HttpNotFound();
            }
            return View(exam);
        }

        // POST: Exams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,IdCourseSections,StartTime,EndTime,FullMark")] Exam exam)
        {
            if (ModelState.IsValid)
            {
                db.Entry(exam).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details","Trainer",new { id = exam.IdCourseSections});
            }

            return View(exam);
        }

        public ActionResult Delete(int? id)
        {
            Exam exam = db.Exam.Include(a=>a.Questions).Include(a=>a.TraineeExam).FirstOrDefault(a=>a.Id == id);
            if(exam == null) { return RedirectToAction("IndexCourses", "Trainer"); }

            if(exam.Questions.Any(a=>a.HistorySolutions.Any()) || exam.TraineeExam.Any())
            {
                return RedirectToAction("IndexCourses","Trainer");
            }

            foreach(var S in exam.Questions)
            {
                if (S.Solutions.Any())
                {
                    db.Solutions.RemoveRange(S.Solutions);
                }
            }

            db.Questions.RemoveRange(exam.Questions);
            db.Exam.Remove(exam);
            db.SaveChanges();
            return RedirectToAction("Details","Trainer",new { id = exam.IdCourseSections});
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [Route("AddQuestion")]
        public ActionResult AddQuestion(string Name,Exam exam)
        {
            return Content($"<script language='javascript' type='text/javascript'>alert('Thanks for Feedback! {exam.Name}');</script>");

            //var Q = new Questions();
            //Q.Solutions.Add(new Solutions());
            //exam.Questions.Add(Q);
            //return this.RedirectToAction(m => m.Create(exam));
            return RedirectToAction("Create",new { exam = exam });
        }
    }
}
