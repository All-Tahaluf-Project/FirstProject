using Ethink.Encrypt;
using Ethink.Models;
using Ethink.Models.Input_Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Ethink.Controllers.CRUD_Admin
{
    [Authorize(Roles = "Admin")]
    public class TrainersController : Controller
    {
        private Context _context = new Context();
        // GET: Trainer
        public ActionResult Index()
        {
            return View(_context.Employee.Include(a=>a.CourseSections).Where(a=>a.ApplicationUser.UserRole.Any(c=>c.IdRole == 3)));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(IN_RegisterTrainer model)
        {
            var CheckUsers = _context.ApplicationUser.FirstOrDefault(a => a.UserName == model.UserName || a.Email == model.Email);

            if (CheckUsers == null)
            {
                var User = new ApplicationUser()
                {
                    UserName = model.UserName,
                    Block = false,
                    Email = model.Email,
                    Gender = model.Gender,
                    NickName = model.NickName,
                    Password = AesOperation.EncryptString(model.Password),
                    PhoneNumber = model.PhoneNumber,
                    ImageName = "",
                };

                _context.ApplicationUser.Add(User);
                _context.SaveChanges();

                var Employee = new Employee()
                {
                    IdUser = User.Id,
                    CourseSections = new List<CourseSections>(),
                    PaySalary = new List<PaySalary>(),
                    Salary = model.Salary,
                };

                _context.Employee.Add(Employee);

                var UserRole = new UserRole()
                {
                    IdRole = _context.Role.FirstOrDefault(a => a.Name == "Trainer").Id,
                    IdUser = User.Id,
                };

                _context.UserRole.Add(UserRole);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Status = "Username Or Email Is Used.";
                return View(model);
            }
        }

        public ActionResult Delete(int? id)
        {
            Employee employee = _context.Employee.Include(a=>a.PaySalary).Include(a=>a.CourseSections).FirstOrDefault(a=>a.Id == id);
            var ApplicationUser = _context.ApplicationUser.FirstOrDefault(a => a.Id == employee.IdUser);

            if(employee.CourseSections.Any())
            {
                return RedirectToAction("Index");
            }

            _context.PaySalary.RemoveRange(employee.PaySalary);
            _context.Employee.Remove(employee);
            _context.ApplicationUser.Remove(ApplicationUser);
            _context.UserRole.Remove(_context.UserRole.FirstOrDefault(a => a.IdUser == ApplicationUser.Id));
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            var employee = _context.Employee.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            var IN_RegisterTrainer = new IN_RegisterTrainer()
            {
                Id = employee.Id,
                IdUser = employee.ApplicationUser.Id,
                Salary = employee.Salary,
                Email = employee.ApplicationUser.Email,
                Gender = employee.ApplicationUser.Gender,
                NickName = employee.ApplicationUser.NickName,
                PhoneNumber = employee.ApplicationUser.PhoneNumber,
                UserName = employee.ApplicationUser.UserName,
            };

            return View(IN_RegisterTrainer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IN_RegisterTrainer model)
        {

                var Employee = _context.Employee.FirstOrDefault(a => a.Id == model.Id);
                var ApplicationUser = _context.ApplicationUser.FirstOrDefault(a => a.Id == model.IdUser);

                ApplicationUser.UserName = model.UserName;
                ApplicationUser.Email = model.Email;
                ApplicationUser.Gender = model.Gender;
                ApplicationUser.NickName = model.NickName;
                ApplicationUser.PhoneNumber = model.PhoneNumber;

                Employee.Salary = model.Salary;

                _context.Entry(ApplicationUser).State = EntityState.Modified;
                _context.Entry(Employee).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");

        }


        public ActionResult Block(int id)
        {
            Employee employee = _context.Employee.Find(id);
            employee.ApplicationUser.Block = !employee.ApplicationUser.Block;

            _context.Entry(employee).State = EntityState.Modified;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            Employee employee = _context.Employee.Include(a=>a.ApplicationUser).FirstOrDefault(a=>a.Id == id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            return View(employee);
        }
    }
}