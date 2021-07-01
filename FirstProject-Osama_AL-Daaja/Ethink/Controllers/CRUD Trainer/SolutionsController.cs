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
    public class SolutionsController : Controller
    {
        private Context db = new Context();

        // GET: Solutions
        public ActionResult Index()
        {
            var solutions = db.Solutions.Include(s => s.Questions);
            return View(solutions.ToList());
        }

        // GET: Solutions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Solutions solutions = db.Solutions.Find(id);
            if (solutions == null)
            {
                return HttpNotFound();
            }
            return View(solutions);
        }

        // GET: Solutions/Create
        public ActionResult Create(int? id)
        {
            var Question = db.Questions.FirstOrDefault(a => a.Id == id);

            if(Question == null) { return RedirectToAction( "IndexCourses", "Trainer"); }
            var Solution = new Solutions()
            {
                IdQusetions = Question.Id,
            };
            return View(Solution);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,IdQusetions,Name,IsTrue")] Solutions solutions)
        {
            if(solutions.Name == "" || solutions.Name == null)
            {
                return View(solutions);
            }
            if (ModelState.IsValid)
            {
                db.Solutions.Add(solutions);
                db.SaveChanges();
                return RedirectToAction("Details","Questions",new { id = solutions.IdQusetions});
            }

            return View(solutions);
        }

        // GET: Solutions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("IndexCourses", "Trainer");
            }
            Solutions solutions = db.Solutions.Find(id);
            if (solutions == null)
            {
                return HttpNotFound();
            }
            return View(solutions);
        }

        // POST: Solutions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IdQusetions,Name,IsTrue")] Solutions solutions)
        {
            if (ModelState.IsValid)
            {
                db.Entry(solutions).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Questions", new { id = solutions.IdQusetions });
            }
            return View(solutions);
        }

        public ActionResult Delete(int id,int idQ)
        {
            Solutions solutions = db.Solutions.Find(id);
            db.Solutions.Remove(solutions);
            db.SaveChanges();
            return RedirectToAction("Details","Questions",new { id = idQ});
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
