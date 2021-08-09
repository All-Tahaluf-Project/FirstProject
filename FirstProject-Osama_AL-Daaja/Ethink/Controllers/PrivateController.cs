using Ethink.Encrypt;
using Ethink.Models;
using Ethink.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Ethink.Controllers
{
    [Authorize(Roles = "Trainer,Admin,Trainee,Accountant")]
    public class PrivateController : Controller
    {
        private Context _context = new Context();
        // GET: Private
        public ActionResult EditMyProfile()
        {
            var MyData = _context.ApplicationUser.FirstOrDefault(a => a.UserName == User.Identity.Name);

            var ApplicationUser = new DTOApplicationUser()
            {
                Id = MyData.Id,
                Email = MyData.Email,
                Gender = MyData.Gender,
                ImageName = MyData.ImageName,
                NickName = MyData.NickName,
                Password = null,
                PasswordConfirm = null,
                PhoneNumber = MyData.PhoneNumber,
                UserName = MyData.UserName
            };

            return View(ApplicationUser);
        }

        [HttpPost]
        public ActionResult EditUserName(DTOApplicationUser model)
        {
            if (!_context.ApplicationUser.Any(a => a.UserName == model.UserName))
            {
                var ApplicationUser = _context.ApplicationUser.FirstOrDefault(a => a.UserName == User.Identity.Name);

                ApplicationUser.UserName = model.UserName;

                _context.Entry(ApplicationUser).State = EntityState.Modified;
                _context.SaveChanges();

                FormsAuthentication.SetAuthCookie(ApplicationUser.UserName, false);
                return RedirectToAction("RedirectToMyPage", "Public");
            }else
            {
                return RedirectToAction("EditMyProfile",model);
            }
        }

        [HttpPost]
        public ActionResult EditEmail(DTOApplicationUser model)
        {

            if (!_context.ApplicationUser.Any(a => a.Email == model.Email))
            {
                var ApplicationUser = _context.ApplicationUser.FirstOrDefault(a => a.UserName == User.Identity.Name);

                ApplicationUser.Email = model.Email;

                _context.Entry(ApplicationUser).State = EntityState.Modified;
                _context.SaveChanges();

                return RedirectToAction("RedirectToMyPage", "Public");
            }
            else
            {
                return RedirectToAction("EditMyProfile", model);
            }
        }
        
        [HttpPost]
        public ActionResult EditPassword(DTOApplicationUser model)
        {

            if (model.Password == model.PasswordConfirm)
            {
                var ApplicationUser = _context.ApplicationUser.FirstOrDefault(a => a.UserName == User.Identity.Name);

                ApplicationUser.Password = AesOperation.EncryptString(model.Password);

                _context.Entry(ApplicationUser).State = EntityState.Modified;
                _context.SaveChanges();

                return RedirectToAction("RedirectToMyPage", "Public");
            }
            else
            {
                return RedirectToAction("EditMyProfile", model);
            }
        }

        [HttpPost]
        public ActionResult EditPhoneNumber(DTOApplicationUser model)
        {
                var ApplicationUser = _context.ApplicationUser.FirstOrDefault(a => a.UserName == User.Identity.Name);

                ApplicationUser.PhoneNumber = model.PhoneNumber;

                _context.Entry(ApplicationUser).State = EntityState.Modified;
                _context.SaveChanges();

                return RedirectToAction("RedirectToMyPage", "Public");
        }

        [HttpPost]
        public ActionResult EditNickName(DTOApplicationUser model)
        {
            var ApplicationUser = _context.ApplicationUser.FirstOrDefault(a => a.UserName == User.Identity.Name);

            ApplicationUser.NickName = model.NickName;

            _context.Entry(ApplicationUser).State = EntityState.Modified;
            _context.SaveChanges();

            return RedirectToAction("RedirectToMyPage", "Public");
        }

        [HttpPost]
        public ActionResult EditGender(DTOApplicationUser model)
        {
            var ApplicationUser = _context.ApplicationUser.FirstOrDefault(a => a.UserName == User.Identity.Name);

            ApplicationUser.Gender = model.Gender;

            _context.Entry(ApplicationUser).State = EntityState.Modified;
            _context.SaveChanges();

            return RedirectToAction("RedirectToMyPage", "Public");
        }

        [HttpPost]
        public ActionResult EditImage(HttpPostedFileBase MyImage)
        {
            if (Path.GetExtension(MyImage.FileName).ToLower() == ".jpg" || Path.GetExtension(MyImage.FileName).ToLower() == ".png")
            {
                var ApplicationUser = _context.ApplicationUser.FirstOrDefault(a => a.UserName == User.Identity.Name);

                if (MyImage != null)
                {
                    string oldpath = Path.Combine(Server.MapPath("~/Assets/Users"), ApplicationUser.ImageName);
                    if (!Directory.Exists(oldpath))
                    {
                        System.IO.File.Delete(oldpath);
                    }
                    string path = Path.Combine(Server.MapPath("~/Assets/Users"), MyImage.FileName);
                    MyImage.SaveAs(path);
                    ApplicationUser.ImageName = MyImage.FileName;
                    Session["ImageName"] = ApplicationUser.ImageName;
                }

                _context.Entry(ApplicationUser).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("RedirectToMyPage", "Public");
            }
            else
            {
                return RedirectToAction("EditMyProfile");
            }

        }
    }
}