using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Ethink.Models;

namespace Ethink.Controllers.CRUD_Admin
{
    [Authorize(Roles = "Admin")]
    public class DiscountPricesController : Controller
    {
        private Context db = new Context();

        // GET: DiscountPrices/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DiscountPrice discountPrice = db.DiscountPrice.Find(id);
            if (discountPrice == null)
            {
                return HttpNotFound();
            }
            return View(discountPrice);
        }

        // GET: DiscountPrices/Create
        public ActionResult Create(int IdCourse)
        {
            ViewBag.IdCourse = new SelectList(db.Course, "Id", "Name");

            DiscountPrice discountPrice = new DiscountPrice()
            {
                IdCourse = IdCourse
            };
            return View(discountPrice);
        }

        // POST: DiscountPrices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,StartDate,EndDate,DiscountValue,IdCourse")] DiscountPrice discountPrice)
        {
            if (ModelState.IsValid)
            {
                db.DiscountPrice.Add(discountPrice);
                db.SaveChanges();
                return RedirectToAction("Details", "Courses", new { Id = discountPrice.IdCourse });
            }

            ViewBag.IdCourse = new SelectList(db.Course, "Id", "Name", discountPrice.IdCourse);
            return View(discountPrice);
        }

        // GET: DiscountPrices/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DiscountPrice discountPrice = db.DiscountPrice.Find(id);
            if (discountPrice == null)
            {
                return HttpNotFound();
            }

            return View(discountPrice);
        }

        // POST: DiscountPrices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,StartDate,EndDate,DiscountValue,IdCourse")] DiscountPrice discountPrice)
        {
            if (ModelState.IsValid)
            {
                db.Entry(discountPrice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Courses", new { Id = discountPrice.IdCourse });
            }
            return View(discountPrice);
        }

        public ActionResult Delete(int id)
        {
            DiscountPrice discountPrice = db.DiscountPrice.Find(id);
            db.DiscountPrice.Remove(discountPrice);
            db.SaveChanges();
            return RedirectToAction("Details", "Courses", new { Id = discountPrice.IdCourse });
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
