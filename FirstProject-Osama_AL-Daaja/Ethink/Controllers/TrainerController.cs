using Ethink.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Ethink.Models.DTO;
using System.IO;
using Rotativa;

namespace Ethink.Controllers
{
    [Authorize(Roles = "Trainer")]
    public class TrainerController : Controller
    {
        private Context _context = new Context();
        // GET: Trainer
        public ActionResult Index(DateTime ? Start,DateTime? End)
        {
            var DTO = new DTOTrainerPage();

            var MyAccount = _context.Employee.FirstOrDefault(a => a.ApplicationUser.UserName == User.Identity.Name);

            if (Start == null || End == null)
            {
                var S = DateTime.Now;
                DTO.CountCourseSection = _context.CourseSections.Count(a =>  a.IdTrainer == MyAccount.Id && a.Course.EndDate > DateTime.Now);
                DTO.CountCourseSectionMaleTrainee = _context.Course_Trainee.Count(a => a.ApplicationUser.Gender && a.CourseSections.Course.EndDate > DateTime.Now && a.CourseSections.IdTrainer == MyAccount.Id);
                DTO.CountCourseSectionFemaleTrainee = _context.Course_Trainee.Count(a => !a.ApplicationUser.Gender && a.CourseSections.Course.EndDate > DateTime.Now && a.CourseSections.IdTrainer == MyAccount.Id);


                DTO.SumOfMarks = _context.CourseSections.Where(a=>a.IdTrainer == MyAccount.Id).Sum(a => a.Exam.Sum(e => e.TraineeExam.Sum(m => m.Mark)));
                DTO.SumOfTotalMarks = _context.CourseSections.Where(a => a.IdTrainer == MyAccount.Id).Sum(a => a.Exam.Sum(e => e.FullMark));
                DTO.SumOfAVG = _context.CourseSections.Where(a => a.IdTrainer == MyAccount.Id).Sum(a => a.Exam.Average(e => e.TraineeExam.Sum(m => m.Mark)));
                DTO.MaxMark = _context.CourseSections.Where(a => a.IdTrainer == MyAccount.Id).Max(a => a.Exam.Max(m => m.TraineeExam.Max(e => e.Mark)));
                DTO.MinMark = _context.CourseSections.Where(a => a.IdTrainer == MyAccount.Id).Min(a => a.Exam.Min(m => m.TraineeExam.Min(e => e.Mark)));

            }
            else
            {
                DTO.CountCourseSection = _context.CourseSections.Count(a => a.Course.StartDate >= a.Course.StartDate && a.Course.EndDate <= End && a.IdTrainer == MyAccount.Id);
                DTO.CountCourseSectionMaleTrainee = _context.Course_Trainee.Count(a => a.ApplicationUser.Gender && a.CourseSections.IdTrainer == MyAccount.Id);
                DTO.CountCourseSectionFemaleTrainee = _context.Course_Trainee.Count(a => !a.ApplicationUser.Gender && a.CourseSections.IdTrainer == MyAccount.Id);

                DTO.SumOfMarks = _context.CourseSections.Where(a => a.IdTrainer == MyAccount.Id && a.Course.StartDate >= a.Course.StartDate && a.Course.EndDate <= End).Sum(a => a.Exam.Sum(e => e.TraineeExam.Sum(m => m.Mark)));
                DTO.SumOfTotalMarks = _context.CourseSections.Where(a => a.IdTrainer == MyAccount.Id && a.Course.StartDate >= a.Course.StartDate && a.Course.EndDate <= End).Sum(a => a.Exam.Sum(e => e.FullMark));
                DTO.SumOfAVG = _context.CourseSections.Where(a => a.IdTrainer == MyAccount.Id && a.Course.StartDate >= a.Course.StartDate && a.Course.EndDate <= End).Sum(a => a.Exam.Average(e => e.TraineeExam.Sum(m => m.Mark)));
                DTO.MaxMark = _context.CourseSections.Where(a => a.IdTrainer == MyAccount.Id && a.Course.StartDate >= a.Course.StartDate && a.Course.EndDate <= End).Max(a => a.Exam.Max(m => m.TraineeExam.Max(e => e.Mark)));
                DTO.MinMark = _context.CourseSections.Where(a => a.IdTrainer == MyAccount.Id && a.Course.StartDate >= a.Course.StartDate && a.Course.EndDate <= End).Min(a => a.Exam.Min(m => m.TraineeExam.Min(e => e.Mark)));
            }

            return View(DTO);
        }

        public ActionResult IndexCourses(DateTime? Start, DateTime? End)
        {
            if (Start == null || End == null)
            {
                var MyCourses = _context.CourseSections.Include(a => a.Course.Categories).Include(a => a.Course_Trainee)
                    .Where(a => a.Employee.ApplicationUser.UserName == User.Identity.Name
                    && a.Course.EndDate >= DateTime.Now
                    ).ToList();

                return View(MyCourses);
            }else
            {
                var MyCourses = _context.CourseSections.Include(a => a.Course.Categories).Include(a => a.Course_Trainee)
                    .Where(a => (a.Course.StartDate <= Start && a.Course.EndDate >= Start)
                    || (a.Course.StartDate <= End && a.Course.EndDate >= End)
                    && a.Employee.ApplicationUser.UserName == User.Identity.Name).ToList();

                return View(MyCourses);
            }
        }

        public ActionResult Details(int? id)
        {
            var CourseSection = _context.CourseSections.FirstOrDefault(a => a.Id == id);

            if(CourseSection == null) { return RedirectToAction("IndexCourses"); }

            var Material = _context.Materials.Where(a => a.CourseSections.Id == CourseSection.Id);
            var MaterialsVideo = new List<Materials>();
            var MaterialsDoc = new List<Materials>();
            if (Material.Any())
            {
                 MaterialsVideo = Material.Where(a =>  a.Name.Contains(".mp4")).ToList();
                 MaterialsDoc = Material.Where(a => a.Name.Contains(".docx") || a.Name.Contains(".pdf")).ToList();
            }

            DTOCourseSection dTOCourseSection = new DTOCourseSection()
            {
                Course = CourseSection,
                Course_Trainee = _context.Course_Trainee.Include(a => a.ApplicationUser).Where(a => a.CourseSections.Id == id).ToList(),
                Exam = _context.Exam.Include(a=>a.Questions).Where(a=>a.CourseSections.Id == CourseSection.Id).ToList(),
                MaterialsVideo = MaterialsVideo,
                MaterialsDoc = MaterialsDoc,
            };

            return View(dTOCourseSection);
        }

        public ActionResult MyTraineeInCourseSection(int id)
        {
            var Trainee = _context.Course_Trainee.Include(a=>a.ApplicationUser).Where(a => a.CourseSections.Id == id);
            return View();
        }

        public ActionResult CreateMaterial(int id, HttpPostedFileBase File)
        {
            if(File == null)
            {
                return RedirectToAction("Details", new { id = id });
            }
            if (ModelState.IsValid)
            {

                if (Path.GetExtension(File.FileName).ToLower() == ".mp4" 
                    || Path.GetExtension(File.FileName).ToLower() == ".docx" 
                    || Path.GetExtension(File.FileName).ToLower() == ".pdf")
                {
                    var Material = new Materials()
                    {
                        IdCourseSections = id,
                        Name = File.FileName,
                    };

                    string path = Path.Combine(Server.MapPath("~/Assets/Courses/Materials/"), File.FileName);
                    File.SaveAs(path);

                    _context.Materials.Add(Material);
                    _context.SaveChanges();

                }
            }

            return RedirectToAction("Details", new { id = id });
        }


        public ActionResult DownloadFile(string Name)
        {
            string fullName = Server.MapPath("~/Assets/Courses/Materials/"+Name);

            byte[] fileBytes = GetFile(fullName);
            return File(
                fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "~/Assets/Courses/Materials/" + Name);
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

        public ActionResult DeleteMaterial(int id)
        {
            var Material = _context.Materials.FirstOrDefault(a => a.Id == id);

            if(Material == null) {
                return RedirectToAction("Details", new { id = Material.IdCourseSections });
            }

            string oldpath = Path.Combine(Server.MapPath("~/Assets/Courses/Materials"), Material.Name);
            System.IO.File.Delete(oldpath);

            _context.Materials.Remove(Material);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = Material.IdCourseSections });
        }

        public ActionResult CreateExam(int id)
        {
            return RedirectToAction("Create", "Exams", new { id = id });
        }


        public ActionResult DetailsTraineeForTrainer(int? id)
        {

            var ApplicationUser = _context.ApplicationUser.Include(a=>a.TraineeExam).FirstOrDefault(a => a.Id == id);

            if(ApplicationUser == null)
            {
                return RedirectToAction("IndexCourses");
            }


            return View(ApplicationUser);
        }

        public ActionResult ChangeMark(int? idUser, int? idExam, string Mark)
        {
            var User = _context.ApplicationUser.FirstOrDefault(a => a.Id == idUser);
            var Exam = _context.TraineeExam.FirstOrDefault(a => a.Id == idExam);

            if (User == null || Exam == null || Mark == null || Mark == "") {
                return RedirectToAction("IndexCourses");
                }

            Exam.Mark = Convert.ToDouble(Mark);
            _context.Entry(Exam).State = EntityState.Modified;
            _context.SaveChanges();
            return RedirectToAction("DetailsTraineeForTrainer", new { id = User.Id });
        }

        public ActionResult Result(int? id,int idApplicationUser)
        {
            var MyResult = _context.HistorySolutions.Include(a => a.TraineeExam.Exam.Questions).Include(a => a.TraineeExam.ApplicationUser)
                .Where(a => a.TraineeExam.IdExam == id
                && a.TraineeExam.IdTrainee == idApplicationUser);
            //RedirectToAction("IndexCourses");

            if (!MyResult.Any()) 
            {
                return RedirectToAction("IndexCourses"); 
                //ResultHistorySolutions = new List<HistorySolutions>();
            }

            var DTO = new DTOResultExam()
            {
                Exam = MyResult.FirstOrDefault().TraineeExam.Exam,
                HistorySolutions = MyResult.ToList(),
                IdApplicationUser= idApplicationUser
            };

            return View(DTO);
        }


        public ActionResult IndexCoursesPDF()
        {
            return new ActionAsPdf("IndexCourses")
            {
                FileName = "IndexCourses.pdf",
                PageSize = Rotativa.Options.Size.A4,
            };
        }

        public ActionResult IndexPDF()
        {
            return new ActionAsPdf("Index")
            {
                FileName = "Index.pdf",
                PageSize = Rotativa.Options.Size.A4,
            };
        }
    }
}