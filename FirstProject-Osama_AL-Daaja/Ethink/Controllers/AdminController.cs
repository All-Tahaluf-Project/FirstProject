using Ethink.Models;
using Ethink.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ethink.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private Context _context = new Context();
        public ActionResult Index()
        {
            var DTO = new DTOAdminPage()
            {
                CountCourse = _context.Course.Count(),
                CountCourseSection = _context.CourseSections.Count(),
                CountMaleTrainer = _context.Employee.Count(a => a.ApplicationUser.Gender && a.ApplicationUser.UserRole.Any(c => c.IdRole == 3)),
                CountFemaleTrainer = _context.Employee.Count(a => !a.ApplicationUser.Gender && a.ApplicationUser.UserRole.Any(c => c.IdRole == 3)),
                CountMaleTrainee = _context.ApplicationUser.Count(a => a.Gender && a.UserRole.Any(c => c.IdRole == 2)),
                CountFemaleTrainee = _context.ApplicationUser.Count(a => !a.Gender && a.UserRole.Any(c => c.IdRole == 2)),
                CourseRequests = _context.Course.Where(a => a.CourseSections.Any(r => r.RequestRegister.Any())).ToList()
            };
            return View(DTO);
        }


        public ActionResult CoureseRequests(int? id)
        {
            var Requests = _context.CourseSections.Include(a => a.RequestRegister).Include(a=>a.Course).Where(a => a.IdCourse == id && a.RequestRegister.Any());

            if (!Requests.Any())
            { return RedirectToAction("Index"); }

            return View(Requests);
        }


        public ActionResult Accept(int? id)
        {
            var CourseSection = _context.CourseSections.Include(a => a.Course).FirstOrDefault(a => a.RequestRegister.Any(r => r.Id == id));
            if (CourseSection == null) { return RedirectToAction("Index"); }

            var Request = CourseSection.RequestRegister.FirstOrDefault(a => a.Id == id);
            var CourseTrainee = new Course_Trainee()
            {
                Date = DateTime.Now,
                IdCourseSections = Request.IdCourseSections,
                IdTrainee = Request.IdTrainee
            };

            _context.RequestRegister.Remove(Request);
            _context.Course_Trainee.Add(CourseTrainee);
            _context.SaveChanges();

            return RedirectToAction("CoureseRequests", new { id = CourseSection.Course.Id });
        }

        public ActionResult UnAccept(int? id)
        {
            var CourseSection = _context.CourseSections.Include(a => a.Course).FirstOrDefault(a => a.RequestRegister.Any(r => r.Id == id));
            if (CourseSection == null) { return RedirectToAction("Index"); }

            //            return Content($"<script language='javascript' type='text/javascript'>alert('{Request.CourseSections.Course.Id}');</script>");
            _context.RequestRegister.Remove(CourseSection.RequestRegister.FirstOrDefault(a => a.Id == id));
            _context.SaveChanges();

            return RedirectToAction("CoureseRequests", new { id = CourseSection.Course.Id });
        }

        public ActionResult DetailsTrainee(int? id, int? idCourseSection)
        {
            var Trainee = _context.ApplicationUser.Include(a => a.Certificates).Include(a => a.Course_Trainee).Include(a => a.PayCard).FirstOrDefault(a => a.Id == id);

            var Course_Trainee = _context.Course_Trainee.FirstOrDefault(a => a.Id == idCourseSection);
            if (Trainee == null || Course_Trainee == null) { return RedirectToAction("Index"); }

            var C = Trainee.Certificates.Last().Name;
            var DTO = new DTODetailsTraineeForAdmin()
            {
                ApplicationUser = Trainee,
                Course_Trainee = Course_Trainee,
            };

            return View(DTO);
        }

        public ActionResult DetailsTraineeForEnroll(int? id)
        {
            var Trainee = _context.ApplicationUser.Include(a=>a.Certificates).Include(a => a.Course_Trainee).Include(a => a.PayCard).FirstOrDefault(a => a.Id == id);

            if (Trainee == null) { return RedirectToAction("Index"); }

            return View(Trainee);
        }

        public ActionResult FinancialReportTreainee(int ? id)
        {
            var Course_Trainee = _context.Course_Trainee.FirstOrDefault(a => a.Id == id);
            if (Course_Trainee == null) { return RedirectToAction("Index"); }

            var SumPay = Course_Trainee.CourseSections.PayLog.Where(a => a.PayCard.IdTrainee == Course_Trainee.ApplicationUser.Id
            && !a.Status).Sum(a => a.Value);

            var Discount = Course_Trainee.CourseSections.Course.DiscountPrice
                .Where(a => a.StartDate >= Course_Trainee.Date && a.EndDate <= Course_Trainee.Date).OrderBy(a => a.DiscountValue).FirstOrDefault();

            decimal Dis = 0;

            if (Discount != null)
            {
                Dis = SumPay * Convert.ToDecimal(Discount.DiscountValue / 100);
            }

            Course_Trainee.CourseSections.PayLog = Course_Trainee.CourseSections.PayLog.Where(a =>
            a.PayCard.IdTrainee == Course_Trainee.ApplicationUser.Id).ToList();

            var FinancialReport = new DTOFinancialReportTreainee()
            {
                SumPay = SumPay,
                SumDiscount = Dis,
                Course_Trainee = Course_Trainee,
            };

            return View(FinancialReport);
        }

        public ActionResult UploadCertificates(int IdApplicationUser, int IdCourseTrainee, HttpPostedFileBase File)
        {


            if(File == null)
            {
                return RedirectToAction("DetailsTrainee", new { id = IdApplicationUser, idCourseSection = IdCourseTrainee });
            }
            if (Path.GetExtension(File.FileName).ToLower() == ".docx" || Path.GetExtension(File.FileName).ToLower() == ".pdf")
            {
                var ApplicationUser = _context.ApplicationUser.Include(a => a.Certificates)
.FirstOrDefault(a => a.Id == IdApplicationUser);
                var CourseTrainee = _context.Course_Trainee.Include(a=>a.CourseSections).FirstOrDefault(a => a.Id == IdCourseTrainee);

                if (File != null)
                {
                    if(ApplicationUser.Certificates.Any())
                    {
                        string oldpath = Path.Combine(Server.MapPath("~/Assets/Certificates"), ApplicationUser.Certificates.OrderBy(a => a.Id).Last().Name);
                        if (!Directory.Exists(oldpath))
                        {
                            System.IO.File.Delete(oldpath);
                            _context.Certificates.Remove(ApplicationUser.Certificates.Last());
                        }
                    }
                    string path = Path.Combine(Server.MapPath("~/Assets/Certificates"), File.FileName);
                    File.SaveAs(path);
                    var Certificates = new Certificates()
                    {
                        IdTrainee = ApplicationUser.Id,
                        IdCourseSections = CourseTrainee.CourseSections.Id,
                        Name = File.FileName
                    };
                    _context.Certificates.Add(Certificates);
                    _context.SaveChanges();

                }


            }

            return RedirectToAction("DetailsTrainee", new { id = IdApplicationUser, idCourseSection = IdCourseTrainee });
        }


        public ActionResult DownloadFile(string Name)
        {
            string fullName = Server.MapPath("~/Assets/Certificates/" + Name);

            byte[] fileBytes = GetFile(fullName);
            return File(
                fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "~/Assets/Certificates/" + Name);
        }

        byte[] GetFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new System.IO.IOException(s);
            return data;
        }

        public ActionResult ContactUsList()
        {
            return View(_context.ContactUs);
        }

        public ActionResult DeleteContactUs(int ? id)
        {
            var ContactUs = _context.ContactUs.FirstOrDefault(a => a.Id == id);

            if(ContactUs == null) { return RedirectToAction("ContactUsList"); }

            _context.ContactUs.Remove(ContactUs);
            _context.SaveChanges();

            return RedirectToAction("ContactUsList");
        }

        public ActionResult TestimonialList()
        {
            return View(_context.Testimonial);
        }

        public ActionResult DeleteTestimonial(int? id)
        {
            var Testimonial = _context.Testimonial.FirstOrDefault(a => a.Id == id);

            if (Testimonial == null) { return RedirectToAction("TestimonialList"); }

            _context.Testimonial.Remove(Testimonial);
            _context.SaveChanges();

            return RedirectToAction("ContactUsList");
        }

        public ActionResult ShowTestimonial(int? id)
        {
            var Testimonial = _context.Testimonial.FirstOrDefault(a => a.Id == id);

            if (Testimonial == null) { return RedirectToAction("TestimonialList"); }

            Testimonial.Status = !Testimonial.Status;

            _context.Entry(Testimonial).State = EntityState.Modified;
            _context.SaveChanges();

            return RedirectToAction("TestimonialList");
        }
    }
}