using Ethink.Models;
using Ethink.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.IO;
using OfficeOpenXml;
using Rotativa;

namespace Ethink.Controllers
{
    [Authorize(Roles ="Accountant")]
    public class AccountantController : Controller
    {
        private Context _context = new Context();
        public ActionResult Index(DateTime? Start , DateTime? End)
        {
            ViewBag.Start = Start;
            ViewBag.End = End;
            if (Start == null || End == null)
            {
                var D = DateTime.Now.AddDays(-300);
                var ListOfPay = _context.PayLog.Where(a=> a.Date > D);

                var CourseSectionList = _context.CourseSections.Include(a=>a.PayLog);

                var CourseSection = new CourseSections()
                {
                    Course = new Course() { Name = "Empty" },
                };

                if (CourseSectionList.Any()) 
                {
                    CourseSection = CourseSectionList.OrderBy(a =>
                     a.PayLog.Where(p => !p.Status).Sum(s => s.Value)).ToList().Last(); 
                }

                var DTO = new DTOAccountantPage()
                {
                    CourseSections = CourseSection,
                    SumPay = ListOfPay.Where(a => a.IdCourseSection != null && !a.Status).Any() ? ListOfPay.Where(a => a.IdCourseSection != null && !a.Status).Sum(a => a.Value) : 0,
                    CountOfPay = ListOfPay.Count(),
                    CountOfPayNotProblem = ListOfPay.Where(a => !a.Status).Count(),
                    CountOfPayProblem = ListOfPay.Where(a => a.Status).Count(),
                };

                return View(DTO);
            }else
            {
                var ListOfPay = _context.PayLog.Where(a=>a.Date >= Start && a.Date <= End);

                //var CourseSection = _context.CourseSections
                //    .OrderBy(a => a.PayLog.Where(p => p.Date >= Start && p.Date <= End && !p.Status).Sum(s => s.Value)).Take(2).ToList();
                //if(!CourseSection.Any()) { CourseSection = new List<CourseSections>(); }

                //           var CourseSection = _context.CourseSections.Where(a=> a.PayLog.Where(p=>p.Date >= Start && p.Date <= End).Any()).OrderBy(a =>
                //a.PayLog.Where(p => !p.Status).Sum(s => s.Value)).ToList().Last();

                var CourseSectionList = _context.CourseSections.Include(a => a.PayLog)
                    .Where(a => a.PayLog.Where(p => p.Date >= Start && p.Date <= End).Any());

                var CourseSection = new CourseSections() {
                Course  = new Course() { Name = "Empty"},
                };

                if (CourseSectionList.Any())
                {
                    CourseSection = CourseSectionList.OrderBy(a =>
                     a.PayLog.Where(p => !p.Status).Sum(s => s.Value)).ToList().Last();
                }

                var DTO = new DTOAccountantPage()
                {
                    CourseSections = CourseSection,
                    SumPay = ListOfPay.Where(a => a.IdCourseSection != null && !a.Status).Any() ? ListOfPay.Where(a => a.IdCourseSection != null && !a.Status).Sum(a=>a.Value) : 0,
                    CountOfPay = ListOfPay.Count(),
                    CountOfPayNotProblem = ListOfPay.Where(a => !a.Status).Count(),
                    CountOfPayProblem = ListOfPay.Where(a => a.Status).Count(),
                };
                return View(DTO);
            }
        }

        public ActionResult ProblemPayments()
        {
            return View(_context.PayLog.Include(a=>a.PayCard.ApplicationUser).Where(a => a.Status).ToList());
        }

        public ActionResult DeletePayLog(int ?id)
        {
            var Payment = _context.PayLog.FirstOrDefault(a => a.Id == id);

            if (Payment == null) { return RedirectToAction("Index"); }

            _context.PayLog.Remove(Payment);
            _context.SaveChanges();

            return RedirectToAction("ProblemPayments");
        }

        public ActionResult ListOfTrainee(int? IdCategory, string Name,string Email)
        {
            var Trainee = new List<ApplicationUser>();
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");

            if (IdCategory != 0)
            {
                Trainee = _context.ApplicationUser
                    .Where(a => a.UserRole.Any(r=>r.IdRole == 2) && 
                    a.Course_Trainee.Where(c =>c.CourseSections.Course.Categories.Id == IdCategory).Any()).ToList();
            }
            if(Name != "" && Name != null)
            {
                Trainee = _context.ApplicationUser.Where(a => a.UserRole.Any(r => r.IdRole == 2) && a.UserName.Contains(Name)).ToList();
            }
            if (Email != "" && Email != null)
            {
                Trainee = _context.ApplicationUser.Where(a => a.UserRole.Any(r => r.IdRole == 2) && a.Email.Contains(Email)).ToList();
            }

            return View(Trainee);
        }

        public ActionResult DetailsApplicationUser(int? id)
        {
            var ApplicationUser = _context.ApplicationUser.FirstOrDefault(a => a.Id == id);

            if(ApplicationUser == null) { return RedirectToAction("Index"); }

            return View(ApplicationUser);
        }

        public ActionResult DetailsEmployee(int? id)
        {
            var Employee = _context.Employee.FirstOrDefault(a => a.Id == id);

            if (Employee == null) { return RedirectToAction("Index"); }

            return View(Employee);
        }


        public ActionResult FinancialReportTreainee(int? id)
        {
            var Course_Trainee = _context.Course_Trainee.FirstOrDefault(a => a.Id == id);

            if(Course_Trainee == null) { return RedirectToAction("Index"); }
            var SumPay = Course_Trainee.CourseSections.PayLog.Where(a => a.PayCard.IdTrainee == Course_Trainee.ApplicationUser.Id
            && !a.Status).Sum(a => a.Value);

            //var Discount = Course_Trainee.CourseSections.Course.DiscountPrice
            //    .Where(a => a.StartDate >= Course_Trainee.Date && a.EndDate <= Course_Trainee.Date).OrderBy(a => a.DiscountValue).FirstOrDefault();

            //decimal Dis = 0;

            //if (Discount != null)
            //{
            //    Dis = SumPay * Convert.ToDecimal(Discount.DiscountValue / 100);
            //}

            Course_Trainee.CourseSections.PayLog = Course_Trainee.CourseSections.PayLog.Where(a =>
            a.PayCard.IdTrainee == Course_Trainee.ApplicationUser.Id).ToList();

            var FinancialReport = new DTOFinancialReportTreainee()
            {
                //SumPay = SumPay,
                //SumDiscount = Dis,
                Course_Trainee = Course_Trainee,
            };

            return View(FinancialReport);
        }

        public ActionResult DeletePay(int? idPayLog, int? IdCourseTrainee)
        {
            var PayLog = _context.PayLog.Include(a => a.PayCard).FirstOrDefault(a => a.Id == idPayLog);
            var Course_Trainee = _context.Course_Trainee.FirstOrDefault(a => a.Id == IdCourseTrainee);

            if (PayLog == null || Course_Trainee == null) { return RedirectToAction("Index"); }

            if (!PayLog.Status)
            {
                PayLog.PayCard.Total += PayLog.Value;
            }
            _context.PayLog.Remove(PayLog);
            _context.SaveChanges();

            return RedirectToAction("FinancialReportTreainee", new { id = Course_Trainee.Id });
        }




        public ActionResult EmployeeList(int? IdRole, string UserName, string Email)
        {
            var Employee = new List<Employee>();
            var Roles = _context.Role.Where(a => a.Name != "Trainee" && a.Name != "Admin");

            ViewBag.Roles = new SelectList(Roles, "Id", "Name");

            if (IdRole != 0)
            {
                Employee = _context.Employee
                    .Where(a => a.ApplicationUser.UserRole.Any(r => r.IdRole == IdRole)).ToList();
            }
            if (UserName != "" && UserName != null)
            {
                Employee = _context.Employee.Where(a => a.ApplicationUser.UserName.Contains(UserName)).ToList();
            }
            if (Email != "" && Email != null)
            {
                Employee = _context.Employee.Where(a => a.ApplicationUser.Email.Contains(Email)).ToList();
            }

            return View(Employee);
        }

        public ActionResult AddPaymentSalary(int? id)
        {
            var Employee = _context.Employee.FirstOrDefault(a => a.Id == id);

            if(Employee == null) { return RedirectToAction("Index"); }

            var NewPaySalary = new PaySalary()
            {
                IdEmployee = Employee.Id,
            };

            return View(NewPaySalary);
        }

        [HttpPost]
        public ActionResult AddPaymentSalary(PaySalary model)
        {
            var NewPaySalary = new PaySalary()
            {
                IdEmployee = model.IdEmployee,
                Value = model.Value,
                Date = DateTime.Now
            };

            _context.PaySalary.Add(NewPaySalary);
            _context.SaveChanges();
            return RedirectToAction("DetailsEmployee", new {id = NewPaySalary.IdEmployee });
        }

        //Excel
        //public ActionResult ExportEmployeeList()
        //{
        //    var DataEmployee = _context.Employee.ToList();
        //    var stream = new MemoryStream();

        //    using (var package = new ExcelPackage(stream))
        //    {
        //        var Sheet = package.Workbook.Worksheets.Add("Pdf");

        //        Sheet.Cells.LoadFromCollection(DataEmployee, true);

        //        package.Save();
        //    }

        //    stream.Position = 0;
        //    var FileName = $"ManagesGlobitelEmployeesEmployeeList{DateTime.Now.ToString("yyyy/MM/dd//HH:mm:ss")}.xlsx";
        //    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", FileName);
        //}

        public ActionResult PDF()
        {
            return new ActionAsPdf("Index")
            {
                FileName = "Index.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageHeight = 1000
            };
        }
    }



}