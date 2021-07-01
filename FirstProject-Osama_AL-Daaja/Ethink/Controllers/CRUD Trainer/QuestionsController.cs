using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Ethink.Models;

namespace Ethink.Controllers.CRUD_Trainer
{
    [Authorize(Roles = "Trainer")]
    public class QuestionsController : Controller
    {
        private Context db = new Context();
        //public static Questions MyQuestions = new Questions();

        // GET: Questions
        public ActionResult Index()
        {
            var questions = db.Questions.Include(q => q.Exam);
            return View(questions.ToList());
        }

        // GET: Questions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("IndexCourses", "Trainer");
            }
            Questions questions = db.Questions.Include(a=>a.Exam).Include(a=>a.Solutions).FirstOrDefault(a=>a.Id == id);
            if (questions == null)
            {
                return HttpNotFound();
            }
            return View(questions);
        }

        // GET: Questions/Create
        public ActionResult Create(int? id)
        {
            var Exam = db.Exam.FirstOrDefault(a => a.Id == id);
            if (Exam == null)
            {
                return RedirectToAction("IndexCourses", "Trainer");
            }

            //if (Exam == null || Number == null)
            //{
            //    return RedirectToAction("IndexCourses", "Trainer");
            //}

            //if(MyQuestions == null)
            //{
            //    var Q = new Questions()
            //    {
            //        IdExam = Exam.Id,
            //        Solutions = new List<Solutions>()
            //    };
            //}

            //for(int i = 0; i<Number;i++)
            //{
            //    MyQuestions.Solutions.Add(new Solutions());
            //}

            //return View(MyQuestions);

            var Question = new Questions()
            {
                IdExam = Exam.Id,
            };
            return View(Question);
        }

        // POST: Questions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,IdExam,Name,Mark")] Questions questions)
        {
            //if(questions.Solutions.Count() == 0) { return View(questions); }
            //if(questions.Name == "" || questions.Name == null ||
            //    questions.Mark == 0 ||
            //    !questions.Solutions.Any(a=>a.Name == null || a.Name == "" ) || questions.Solutions.Count(a=>a.IsTrue) == 0)
            //{
            //    ViewBag.Status = "Make Sure Enter All Values.";
            //    return View(questions);
            //}
            if(questions.Name == null || questions.Name == "" || questions.Mark == 0)
            {
                ViewBag.Status = "Make Sure Enter All Values.";
                return View(questions);
            }

            if (ModelState.IsValid)
            {
                db.Questions.Add(questions);
                db.SaveChanges();

                //foreach(var S in questions.Solutions)
                //{
                //    var Solution = new Solutions()
                //    {
                //        IdQusetions = questions.Id,
                //        IsTrue = S.IsTrue,
                //        Name = S.Name,
                //    };

                //    db.Solutions.Add(Solution);
                //}

                db.SaveChanges();
                return RedirectToAction("Details", "Exams",new { id = questions.IdExam});
            }


            return View(questions);
        }

        // GET: Questions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("IndexCourses", "Trainer");
            }
            Questions questions = db.Questions.Find(id);
            if (questions == null)
            {
                return HttpNotFound();
            }
            return View(questions);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IdExam,Name,Mark")] Questions questions)
        {
            if(questions.Name == "" || questions.Name == null || questions.Mark == 0)
            {
                return View(questions);
            }
            if (ModelState.IsValid)
            {
                db.Entry(questions).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Exams", new { id = questions.IdExam });
            }
            return View(questions);
        }

        // GET: Questions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Questions questions = db.Questions.Find(id);
            if (questions == null)
            {
                return HttpNotFound();
            }
            return View(questions);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Questions questions = db.Questions.Find(id);
            db.Questions.Remove(questions);
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


        public ActionResult AddChild()
        {
            return PartialView("_Question", new Solutions());
        }

        //public ActionResult AddQuestion(string Name,bool IsTrue)
        //{
        //    Solutions solutions = new Solutions() { Name = Name,IsTrue = IsTrue};
        //    MyQuestions.Solutions.Add(solutions);
        //    return RedirectToAction("Create", new { id = solutions.IdQusetions, number = 1 });
        //    //if(MyQuestions.Solutions.Any())
        //    //{
        //    //    return RedirectToAction("RedirectToMyPage", "Public");
        //    //}
        //    //else
        //    //{
        //    //    MyQuestions.Solutions.Add(new Solutions() { Name = "Name", IsTrue = true });
        //    //    return RedirectToAction("Create", new { id = id,number = 1});
        //    //}
        //}
    }
}
