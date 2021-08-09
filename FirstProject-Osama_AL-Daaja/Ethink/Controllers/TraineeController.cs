using Ethink.Models;
using Ethink.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Ethink.Models.Input_Model;
using Ethink.Encrypt;
using Rotativa;

namespace Ethink.Controllers
{
    [Authorize(Roles = "Trainee")]
    public class TraineeController : Controller
    {
        private Context _context = new Context();
        // GET: Trainee
        public ActionResult Index(int?Id,string Name,string StatusTestimonial,bool? AllDiscount)
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            var Course = new List<Course>();
            DTOTraineePage dTOTraineePage = new DTOTraineePage();

            if (Id != 0 && Id != null)
            {
                Course = _context.Course.Include(a => a.DiscountPrice).Where(a => a.EndDate > DateTime.Now && a.IdCategory == Id).Take(20).ToList();
            }

            if (Name != null)
            {
                Course = _context.Course.Include(a => a.DiscountPrice).Where(a => a.EndDate > DateTime.Now && a.Name.Contains(Name)).Take(20).ToList();
            }

            if (AllDiscount != null && AllDiscount == true)
            {
                Course = _context.Course.Include(a => a.DiscountPrice).Where(a =>
                a.DiscountPrice.Any(d => d.EndDate >= DateTime.Now) &&
                 a.EndDate >= DateTime.Now).Take(20).ToList();
                //Course = _context.Course.Include(a => a.DiscountPrice).ToList();
            }

            dTOTraineePage.Courses = Course;
            ViewBag.StatusTestimonial = StatusTestimonial;

            return View(dTOTraineePage);
        }

        public ActionResult CourseDetails(int? id)
        {
            var Course = _context.Course.Include(a => a.CourseSections).Include(a=>a.DiscountPrice).FirstOrDefault(a => a.Id == id);

            if(Course == null)
            {
                return RedirectToAction("Index");
            }
            return View(Course);
        }

        public ActionResult AddRequest(int? id)
        {
            var CourseSection = _context.CourseSections.FirstOrDefault(a => a.Id == id);
            if(CourseSection == null) { return RedirectToAction("Index"); }

            var ApplicationUser = _context.ApplicationUser.FirstOrDefault(a => a.UserName == User.Identity.Name);

            var CheckRequestRegisterTable = _context.RequestRegister.Any(a => a.CourseSections.IdCourse == CourseSection.IdCourse && a.IdTrainee == ApplicationUser.Id);
            var CheckCourseSectionTable = _context.Course_Trainee.Any(a => a.CourseSections.Course.Id == CourseSection.IdCourse && a.ApplicationUser.Id == ApplicationUser.Id);
            
            if(!CheckRequestRegisterTable && !CheckCourseSectionTable)
            {
                RequestRegister requestRegister = new RequestRegister()
                {
                    IdCourseSections = CourseSection.Id,
                    IdTrainee = ApplicationUser.Id,
                    Date = DateTime.Now
                };

                _context.RequestRegister.Add(requestRegister);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return RedirectToAction("CourseDetails", new { id = CourseSection.IdCourse});
        }


        public ActionResult ListRequestRegister()
        {
            var MyRequests = _context.RequestRegister.Include(a=>a.CourseSections.Course).Where(a => a.ApplicationUser.UserName == User.Identity.Name);

            return View(MyRequests);
        }

        public ActionResult ListMyCourse()
        {
            var MyCourse = _context.Course_Trainee.Where(a => a.ApplicationUser.UserName == User.Identity.Name); 
            return View(MyCourse);
        }

        public ActionResult DetailsMyCourse(int? id)
        {
            var Course_Trainee = _context.Course_Trainee.Include(a => a.CourseSections.PayLog)
    .Include(a => a.CourseSections.Course).Include(a => a.CourseSections.Materials)
    .Include(a => a.CourseSections.Exam).FirstOrDefault(a => a.CourseSections.Id == id);
            //var CourseSection = _context.CourseSections.Include(a=>a.PayLog)
            //    .Include(a=>a.Course).Include(a=>a.Materials).FirstOrDefault(a => a.Id == id);

            if (Course_Trainee == null) { return RedirectToAction("ListMyCourse"); }

            var Material = Course_Trainee.CourseSections.Materials.Where(a => a.CourseSections.Id == Course_Trainee.CourseSections.Id);
            var MaterialsVideo = new List<Materials>();
            var MaterialsDoc = new List<Materials>();
            if (Material.Any())
            {
                MaterialsVideo = Material.Where(a => a.Name.Contains(".mp4")).ToList();
                MaterialsDoc = Material.Where(a => a.Name.Contains(".docx") || a.Name.Contains(".pdf")).ToList();
            }

            Course_Trainee.CourseSections.PayLog = Course_Trainee.CourseSections.PayLog.Where(p => p.PayCard.ApplicationUser.UserName == User.Identity.Name).ToList();

            //var SumPay = Course_Trainee.CourseSections.PayLog.Where(a => !a.Status).Sum(a => a.Value);
            //var Discount = Course_Trainee.CourseSections.Course.DiscountPrice
            //    .Where(a => a.StartDate >= Course_Trainee.Date && a.EndDate <= Course_Trainee.Date).OrderBy(a => a.DiscountValue).FirstOrDefault();

            //decimal Dis = 0;

            //if(Discount != null)
            //{
            //    Dis = SumPay * Convert.ToDecimal(Discount.DiscountValue / 100);
            //}

            var ApplicationUser = _context.ApplicationUser.Include(a=>a.TraineeExam)
                .FirstOrDefault(a => a.UserName == User.Identity.Name);

            DTOCourseSection dTOCourseSection = new DTOCourseSection()
            {
                //SumPay = SumPay,
                //SumDiscount = Dis,
                Course = Course_Trainee.CourseSections,
                Exam = _context.Exam.Where(a => a.CourseSections.Id == Course_Trainee.CourseSections.Id && a.FullMark <= a.Questions.Sum(q=>q.Mark)).ToList(),
                MaterialsVideo = MaterialsVideo,
                MaterialsDoc = MaterialsDoc,
                ApplicationUser = ApplicationUser,
            };

            return View(dTOCourseSection);
        }

        public ActionResult DownloadFile(string Name)
        {
            string fullName = Server.MapPath("~/Assets/Courses/Materials/" + Name);

            byte[] fileBytes = GetFile(fullName);
            return File(
                fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "~/Assets/Courses/Materials/" + Name);
        }

        public ActionResult DownloadCertificates(string Name)
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


        public ActionResult StartExam(int ? id)
        {
            //var Questions = _context.Questions.Include(a=>a.HistorySolutions).Include(a=>a.Solutions).Where(a => a.Exam.Id == id);

            var Exam = _context.Exam.Include(a => a.Questions).FirstOrDefault(a=>a.Id == id);
            if (Exam == null) { return RedirectToAction("ListMyCourse"); }

            var ApplicationUser = _context.ApplicationUser.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var TraineeE = _context.TraineeExam.Include(a=>a.ApplicationUser).FirstOrDefault(a =>
            a.IdTrainee == ApplicationUser.Id
            && a.IdExam == Exam.Id
            );
            if (TraineeE == null)
            {
                if (Exam.EndTime < DateTime.Now)
                {
                    return RedirectToAction("Result", new { id = Exam.Id});
                }

                TraineeE = new TraineeExam()
                {
                    IdExam = Exam.Id,
                    IdTrainee = _context.ApplicationUser.FirstOrDefault(a => a.UserName == User.Identity.Name).Id,
                    Mark = 0,
                };
                _context.TraineeExam.Add(TraineeE);
                _context.SaveChanges();
            }

            var Q = _context.Questions.Where(a => a.IdExam == Exam.Id && !a.HistorySolutions.Any(h => h.IdTraineeExam == TraineeE.Id));

            if(!Q.Any())
            {
                return RedirectToAction("Result",new { id = Exam.Id});
            }

            return View(Q.FirstOrDefault());
        }

        public ActionResult SubmitSolution(int? id)
        {
            var Solution = _context.Solutions.Include(a=>a.Questions.Exam).FirstOrDefault(a => a.Id == id);
            if (Solution == null) { return RedirectToAction("ListMyCourse"); }

            var TraineeExam = _context.TraineeExam.FirstOrDefault(a =>
a.IdTrainee == _context.ApplicationUser.FirstOrDefault(u => u.UserName == User.Identity.Name).Id
&& a.IdExam == Solution.Questions.IdExam
);

            if (Solution.Questions.Exam.EndTime < DateTime.Now)
            {
                return RedirectToAction("DetailsMyCourse", new { id = Solution.Questions.Exam.IdCourseSections });
            }

            var History = new HistorySolutions()
            {
                IdQusetions = Solution.IdQusetions,
                IdTraineeExam = TraineeExam.Id,
                IdSolutions = Solution.Id,
            };

            if(Solution.IsTrue)
            {
                TraineeExam.Mark += Solution.Questions.Mark;
            }

            _context.Entry(TraineeExam).State = EntityState.Modified;
            _context.HistorySolutions.Add(History);
            _context.SaveChanges();

            return RedirectToAction("StartExam", new { id = Solution.Questions.IdExam });
        }

        public ActionResult PaymentLog(int ?id)
        {
            var CourseSection = _context.CourseSections.FirstOrDefault(a => a.Id == id);
            if(CourseSection == null) { return RedirectToAction("DetailsMyCourse"); }

            var NewPayLog = new IN_PaymentLog()
            {
                IdCourseSection = CourseSection.Id,
            };

            ViewBag.IdCard = new SelectList(_context.PayCard.Where(a=>a.ApplicationUser.UserName == User.Identity.Name), "Id", "Name");
            return View(NewPayLog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PaymentLog(IN_PaymentLog model)
        {


            if (ModelState.IsValid)
            {
                var Card = _context.PayCard.Include(a=>a.ApplicationUser.Course_Trainee).FirstOrDefault(a => a.Id == model.IdCard);
                
                if (Card == null)
                {
                    ViewBag.IdCard = new SelectList(_context.PayCard.Where(a => a.ApplicationUser.UserName == User.Identity.Name), "Id", "Name");
                    ViewBag.Status = "Make Sure Your Card.";
                    return View(model);
                }


                decimal MyValue = 0;
                if (model.Value <= 0)
                {
                    ViewBag.IdCard = new SelectList(_context.PayCard.Where(a => a.ApplicationUser.UserName == User.Identity.Name), "Id", "Name");
                    ViewBag.Status = "Value Should Be More Than 0.";
                    return View(model);
                }else
                {
                    MyValue = model.Value;
                }

                var CourseTrainee = Card.ApplicationUser.Course_Trainee.FirstOrDefault(a=>a.IdCourseSections == model.IdCourseSection);
                var Sum = Card.PayLog.Where(p => !p.Status && p.IdCourseSection == model.IdCourseSection).Sum(a => a.Value);

                var CourseSection = _context.CourseSections.FirstOrDefault(a => a.Id == model.IdCourseSection);
                decimal Price = 0;
                if (CourseSection.Course.DiscountPrice.Any(a=>a.StartDate <= CourseTrainee.Date && a.EndDate >= CourseTrainee.Date))
                {
                    //Price = Convert.ToDouble(CourseSection.Course.Price);
                    Price = CourseSection.Course.Price;
                    Price = Price -  (Price *(Convert.ToDecimal( CourseSection
                        .Course.DiscountPrice.Where(a => a.StartDate <= CourseTrainee.Date && a.EndDate >= CourseTrainee.Date)
                        .OrderBy(a => a.EndDate).Last().DiscountValue / 100) ));
                }else
                {
                    //                    Price = Convert.ToDouble(CourseSection.Course.Price);
                    Price = CourseSection.Course.Price;
                }

                if (model.Value <= (Price - Sum))
                {

                    if (!(AesOperation.DecryptString(Card.CardCode) == model.Code))
                    {
                        ViewBag.IdCard = new SelectList(_context.PayCard.Where(a => a.ApplicationUser.UserName == User.Identity.Name), "Id", "Name");
                        ViewBag.Status = "Your Code Is Incorrect.";
                        return View(model);
                    }

                    if (Card.Total < Convert.ToDecimal(model.Value) && !model.Status)
                    {
                        ViewBag.IdCard = new SelectList(_context.PayCard.Where(a => a.ApplicationUser.UserName == User.Identity.Name), "Id", "Name");
                        ViewBag.Status = "Your Money Is Not Enough.";
                        return View(model);
                    }

                    if (model.IdCourseSection == 0 || model.IdCourseSection == null)
                    {
                        return RedirectToAction("Index");
                    }

                    if (!model.Status)
                    {
                        Card.Total -= model.Value;
                    }

                    model.Date = DateTime.Now;
                    if(MyValue == 0)
                    {
                        ViewBag.IdCard = new SelectList(_context.PayCard.Where(a => a.ApplicationUser.UserName == User.Identity.Name), "Id", "Name");
                        ViewBag.Status = "equal 0";
                        return View(model);
                    }
                    var payLog = new PayLog()
                    {
                        IdCard = model.IdCard,
                        Date = model.Date,
                        IdCourseSection = model.IdCourseSection,
                        Status = model.Status,
                        Value = MyValue,
                    };

                    _context.Entry(Card).State = EntityState.Modified;
                    _context.PayLog.Add(payLog);
                    _context.SaveChanges();
                    return RedirectToAction("DetailsMyCourse", new { id = payLog.IdCourseSection });
                }
                else
                {

                    ViewBag.IdCard = new SelectList(_context.PayCard.Where(a => a.ApplicationUser.UserName == User.Identity.Name), "Id", "Name");
                    ViewBag.Status = $"Make Sure Enter Value Less Or Equal  Than { Price - Sum}";
                    return View(model);
                }
            }

            ViewBag.IdCard = new SelectList(_context.PayCard.Where(a => a.ApplicationUser.UserName == User.Identity.Name), "Id", "Name");
            return View(model);
        }


        public ActionResult AddTestimonial(string Text)
        {
            if(Text == null || Text == "")
            {
                return RedirectToAction("Index", new { StatusTestimonial = "Make Sure Enter Text." });
            }

            var Testimonial = new Testimonial()
            {
                Status = false,
                Text = Text
            };
            _context.Testimonial.Add(Testimonial);
            _context.SaveChanges();

            return RedirectToAction("Index", new { StatusTestimonial = "Done" });
        }

        public ActionResult Result(int? id)
        {
            var ApplicationUser = _context.ApplicationUser.FirstOrDefault(a => a.UserName == User.Identity.Name);
            var MyResult = _context.HistorySolutions.Include(a => a.TraineeExam.Exam.Questions).Include(a => a.TraineeExam.ApplicationUser)
                .Where(a => a.TraineeExam.IdExam == id
                && a.TraineeExam.IdTrainee == ApplicationUser.Id);

            if (!MyResult.Any()) { return RedirectToAction("ListMyCourse"); }

            var DTO = new DTOResultExam()
            {
                Exam = MyResult.FirstOrDefault().TraineeExam.Exam,
                HistorySolutions = MyResult.ToList(),
                IdApplicationUser = ApplicationUser.Id
            };

            return View(DTO);
        }

        public ActionResult ResultPDF(int? id)
        {
            return new ActionAsPdf("Result", new { Id = id})
            {
                FileName = "Result.pdf",
                PageSize = Rotativa.Options.Size.A4,
            };
        }
    }
}