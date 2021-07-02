using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Ethink.Models;
using Ethink.Models.Input_Model;

namespace Ethink.Controllers.CRUD_Admin
{
    [Authorize(Roles = "Admin")]
    public class CourseSectionsController : Controller
    {
        private Context db = new Context();

        // GET: CourseSections/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseSections courseSections = db.CourseSections.Include(a=>a.Course_Trainee).FirstOrDefault(a=>a.Id == id);
            if (courseSections == null)
            {
                return HttpNotFound();
            }
            return View(courseSections);
        }

        // GET: CourseSections/Create
        public ActionResult Create(int IdCourse)
        {
            var Employees = db.Employee.Where(a => a.ApplicationUser.UserRole.Any(r => r.IdRole == 3)).Select(a => new
            {
                Id = a.Id,
                NickName = a.ApplicationUser.NickName
            });
            ViewBag.IdTrainer = new SelectList(Employees, "Id", "NickName");
            IN_CourseSection courseSections = new IN_CourseSection()
            {
                IdCourse = IdCourse,
            };
            return View(courseSections);
        }

        // POST: CourseSections/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Number,IdCourse,MaxCount,IdTrainer")] IN_CourseSection model)
        {
            if(model.MaxCount == 0 || model.Number == 0 || model.IdTrainer == 0)
            {
                var Employees = db.Employee.Where(a=>a.ApplicationUser.UserRole.Any(r=>r.IdRole == 3)).Select(a => new
                {
                    Id = a.Id,
                    NickName = a.ApplicationUser.NickName
                });
                ViewBag.IdTrainer = new SelectList(Employees, "Id", "NickName");
                ViewBag.Status = "Make Sure Enter All Values.";
                return View();
            }
            if (ModelState.IsValid)
            {
                CourseSections courseSections = new CourseSections()
                {
                    IdCourse = model.IdCourse,
                    IdTrainer = model.IdTrainer,
                    Number = model.Number,
                    MaxCount = model.MaxCount,
                };
                db.CourseSections.Add(courseSections);
                db.SaveChanges();
                return RedirectToAction("Details", "Courses", new { Id = courseSections.IdCourse });
            }

            var AllEmployee = db.Employee.Where(a => a.ApplicationUser.UserRole.Any(r => r.IdRole == 3)).Select(a => new
            {
                Id = a.Id,
                NickName = a.ApplicationUser.NickName
            });
            ViewBag.IdTrainer = new SelectList(AllEmployee, "Id", "NickName");
            return View(model);
        }

        // GET: CourseSections/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseSections courseSections = db.CourseSections.Find(id);
            if (courseSections == null)
            {
                return HttpNotFound();
            }
            var AllEmployee = db.Employee.Select(a => new
            {
                Id = a.Id,
                NickName = a.ApplicationUser.NickName
            });
            ViewBag.IdTrainer = new SelectList(AllEmployee, "Id", "NickName");
            return View(courseSections);
        }

        // POST: CourseSections/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Number,IdCourse,MaxCount,IdTrainer")] CourseSections courseSections)
        {
            if (ModelState.IsValid)
            {
                db.Entry(courseSections).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Courses",new {Id = courseSections.IdCourse });
            }
            var AllEmployee = db.Employee.Select(a => new
            {
                Id = a.Id,
                NickName = a.ApplicationUser.NickName
            });
            ViewBag.IdTrainer = new SelectList(AllEmployee, "Id", "NickName");
            return View(courseSections);
        }

        public ActionResult Delete(int id)
        {
            CourseSections courseSections = db.CourseSections.Include(a=>a.Certificates).Include(a=>a.Course_Trainee).Include(a=>a.Exam)
                .Include(a=>a.Materials).Include(a=>a.PayLog).FirstOrDefault(a=>a.Id == id);

            if(courseSections.Certificates.Any() || courseSections.Course_Trainee.Any() || courseSections.Exam.Any()
                || courseSections.Materials.Any() || courseSections.PayLog.Any())
            {
                return RedirectToAction("Details", new { Id = courseSections.Id });
            }
            db.CourseSections.Remove(courseSections);
            db.SaveChanges();
            return RedirectToAction("Details", "Courses", new { Id = courseSections.IdCourse });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult DeleteTraineeFromCourseSection(int? id)
        {
            var trainee = db.Course_Trainee.FirstOrDefault(a => a.Id == id);

            if(trainee == null) { return RedirectToAction("Index", "Admin"); }

            db.Course_Trainee.Remove(trainee);
            db.SaveChanges();
            return RedirectToAction("Details",new { id = trainee.IdCourseSections});
        }
    }
}
