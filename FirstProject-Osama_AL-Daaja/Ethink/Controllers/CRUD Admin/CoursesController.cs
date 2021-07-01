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
using Ethink.Models.DTO;
using Ethink.Models.Input_Model;
using Microsoft.AspNetCore.Hosting;

namespace Ethink.Controllers.CRUD_Admin
{
    [Authorize(Roles = "Admin")]
    public class CoursesController : Controller
    {
        private Context db = new Context();

        //[HttpPost]
        //public ActionResult SearchByCategory(int Id)
        //{
        //    return RedirectToAction("Index", db.Course.Where(a => a.IdCategory == Id).ToList());
        //}

        // GET: Courses
        public ActionResult Index(int? Id,string Name)
        {
            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name");
            var Course = new List<Course>();

            if(Name != null)
            {
                return View(db.Course.Where(a=>a.Name.Contains(Name)).ToList());
            }


            if (Id == 0) 
            {
                return View(Course); 
            }
            else
            {
                return View(db.Course.Where(a => a.IdCategory == Id).Take(20).ToList());
            }
            //var course = db.Course.Include(c => c.Categories);
            //return View(courses);
        }

        // GET: Courses/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Course.Include(a=>a.Categories).Include(a=>a.CourseSections).Include(a=>a.DiscountPrice).FirstOrDefault(a=>a.Id == id);
            if (course == null)
            {
                return HttpNotFound();
            }

            var Course = new DTOCourse()
            {
                Course = course,
                DiscountPrice = course.DiscountPrice.ToList(),
                CourseSections = course.CourseSections.ToList(),
            };

            return View(Course);
        }

        // GET: Courses/Create
        public ActionResult Create()
        {
            ViewBag.IdCategory = new SelectList(db.Categories, "Id", "Name");
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( IN_Course model,HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {

                if (Path.GetExtension(upload.FileName).ToLower() == ".jpg" || Path.GetExtension(upload.FileName).ToLower() == ".png")
                {
                    var course = new Course()
                    {
                        IdCategory = model.IdCategory,
                        Image = upload.FileName,
                        Name = model.Name,
                        Price = model.Price,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                    };


                    string path = Path.Combine(Server.MapPath("~/Assets/Courses/"), upload.FileName);
                    upload.SaveAs(path);

                    db.Course.Add(course);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }else
                {
                    ViewBag.IdCategory = new SelectList(db.Categories, "Id", "Name");
                    ViewBag.Status = "Make Sure File Extension.   "; 
                    return View();
                }
            }

            ViewBag.IdCategory = new SelectList(db.Categories, "Id", "Name", model.IdCategory);
            return View(model);
        }

        // GET: Courses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Course.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdCategory = new SelectList(db.Categories, "Id", "Name", course.IdCategory);
            var model = new IN_Course()
            {
                Id = course.Id,
                Name = course.Name,
                Price = course.Price,
                EndDate = course.EndDate,
                StartDate = course.StartDate,
                IdCategory = course.IdCategory,
                Image = course.Image,
            };
            return View(model);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,IdCategory,StartDate,EndDate,Price,Image")] IN_Course model, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                var Course = db.Course.FirstOrDefault(a => a.Id == model.Id);


                if (upload != null)
                {
                    string oldpath = Path.Combine(Server.MapPath("~/Assets/Courses"), Course.Image);
                    System.IO.File.Delete(oldpath);
                    string path = Path.Combine(Server.MapPath("~/Assets/Courses"), upload.FileName);
                    upload.SaveAs(path);
                    Course.Image = upload.FileName;
                }

                Course.IdCategory = model.IdCategory;
                Course.Name = model.Name;
                Course.Price = model.Price;
                if(model.StartDate != null)
                {
                    Course.StartDate = model.StartDate;
                }
                if (model.EndDate != null)
                {
                    Course.EndDate = model.EndDate;
                }

                db.Entry(Course).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdCategory = new SelectList(db.Categories, "Id", "Name", model.IdCategory);
            return View(model);
        }

        //// GET: Courses/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Course course = db.Course.Find(id);
        //    if (course == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(course);
        //}

        // POST: Courses/Delete/5

        public ActionResult Delete(int id)
        {
            Course course = db.Course.Include(a=>a.CourseSections).Include(a=>a.DiscountPrice).FirstOrDefault(a=>a.Id == id);

            if(course.CourseSections.Any())
            {
                return RedirectToAction("Index");
            }

            db.DiscountPrice.RemoveRange(course.DiscountPrice);

            if(course.Image != "" || course.Image != null)
            {
                string oldpath = Path.Combine(Server.MapPath("~/Assets/Courses"), course.Image);
                System.IO.File.Delete(oldpath);
            }
            db.Course.Remove(course);
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
    }
}
