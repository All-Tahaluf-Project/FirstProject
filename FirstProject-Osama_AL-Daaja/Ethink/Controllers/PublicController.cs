using Ethink.Encrypt;
using Ethink.Models;
using Ethink.Models.DTO;
using Ethink.Models.Input_Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Ethink.Controllers
{
    [AllowAnonymous]
    public class PublicController : Controller
    {
        private Context _context = new Context();

        [AllowAnonymous]
        public ActionResult Home(string StatusContactUS)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("RedirectToMyPage");
            }

            ViewBag.StatusContactUS = StatusContactUS;

            var HomeData = new DTOPublicHome()
            {
                Testimonial = _context.Testimonial.Where(a=>a.Status).ToArray()
            };
            return View(HomeData);
        }

        public ActionResult LogIn()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("RedirectToMyPage");
            }
            return View();
        }

        [HttpPost]
        public ActionResult LogIn(IN_LogIn model)
        {

            if (ModelState.IsValid)
            {
                var user = _context.ApplicationUser.FirstOrDefault(x => (x.UserName == model.UserName || x.Email == model.UserName ));
                if (user == null)
                {
                    ViewBag.Status = "Account Is Not Found.";
                }
                else
                {
                    if(user.Block)
                    {
                        ViewBag.Status = "Your Account Is Blocked";
                        return View();
                    }
                    if (AesOperation.DecryptString(user.Password) == model.Password)
                    {
                        Session["UserName"] = user.UserName;
                        Session["NickName"] = user.NickName;
                        Session["ImageName"] = user.ImageName;

                        FormsAuthentication.SetAuthCookie(model.UserName, false);
                        return RedirectToAction("RedirectToMyPage");
                    }else
                    {
                        ViewBag.Status = "Password Is Incorrect";
                        ViewBag.RefundPassword = "Refund Password";
                    }
                }
            }
            return View(model);
        }

        public ActionResult RedirectToMyPage(string Status)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Home", new { StatusContactUS = "Done" });
            }
            if (User.IsInRole("Trainee"))
            {
                return RedirectToAction("Index", "Trainee");
            }
            else
            {
                if (User.IsInRole("Trainer"))
                {
                    return RedirectToAction("Index", "Trainer");
                }
                else
                {
                    if (User.IsInRole("Accountant"))
                    {
                        return RedirectToAction("Index", "Accountant");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                }
            }
        }


        public ActionResult RegisterTrainee()
        {
            if(User.Identity.IsAuthenticated)
            {
                return RedirectToAction("RedirectToMyPage");
            }
            return View();
        }

        [HttpPost]
        public ActionResult RegisterTrainee(IN_RegisterTrainee model)
        {
            if (
                model.UserName == "" || model.UserName == null ||
                model.Email == "" || model.Email == null ||
                model.Password == "" || model.Password == null ||
                model.PhoneNumber == "" || model.PhoneNumber == null ||
                model.NickName == "" || model.NickName == null 
                )
            {
                ViewBag.Status = "Make Sure Enter All Values.";
                return View(model);
            }
            var CheckUsers = _context.ApplicationUser.FirstOrDefault(a => a.UserName == model.UserName || a.Email == model.Email);

            if (CheckUsers == null)
            {
                return RedirectToAction("ConfirmEmail", new { UserName = model.UserName, Email = model.Email
                ,
                    Gender = model.Gender,
                    NickName = model.NickName,
                    Password = model.Password,
                    PhoneNumber = model.PhoneNumber,
                });
            }else
            {
                ViewBag.Status = "Username Or Email Is Used.";
                return View(model);
            }
        }


        public ActionResult ConfirmEmail(string UserName, string Email, bool Gender,string NickName, string Password,string PhoneNumber)
        {
            if (Email == null)
            {
                return RedirectToAction("Home");
            }
            MailMessage MyMessage = new MailMessage("t.u.world1996worldweb39@gmail.com", Email);

            var CodeEmail = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 10)
      .Select(s => s[new Random().Next(s.Length)]).ToArray());


            MyMessage.Body = "Code : " + CodeEmail;

            MyMessage.Subject = "Confirm Card Code";

            SmtpClient Client = new SmtpClient("smtp.gmail.com", 587);
            Client.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "t.u.world1996worldweb39@gmail.com",
                Password = "Osama1996"
            };

            Client.EnableSsl = true;
            Client.Send(MyMessage);


            var Register = new IN_ConfirmEmail()
            {
                CodeEmail = CodeEmail,
                Email = Email,
                CodeConfirm = "",
                Gender = Gender,
                NickName = NickName,
                Password = Password,
                PhoneNumber = PhoneNumber,
                UserName = UserName,
            };

            return View(Register);
        }

        [HttpPost]
        public ActionResult ConfirmEmail(IN_ConfirmEmail model)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("RedirectToMyPage");
            }

            if(model.CodeEmail != model.CodeConfirm)
            {
                ViewBag.Status = "Make Sure Code.";
                return View(model);
            }

            var NewUser = new ApplicationUser()
            {
                UserName = model.UserName,
                Block = false,
                Rating = new List<Rating>(),
                Certificates = new List<Certificates>(),
                Course_Trainee = new List<Course_Trainee>(),
                Email = model.Email,
                Gender = model.Gender,
                NickName = model.NickName,
                Password = AesOperation.EncryptString(model.Password),
                PayCard = new List<PayCard>(),
                PhoneNumber = model.PhoneNumber,
                RequestRegister = new List<RequestRegister>(),
                TraineeExam = new List<TraineeExam>(),
                ImageName = "",
            };

            _context.ApplicationUser.Add(NewUser);
            _context.SaveChanges();

            var UserRole = new UserRole()
            {
                IdRole = _context.Role.FirstOrDefault(a => a.Name == "Trainee").Id,
                IdUser = NewUser.Id,
            };

            _context.UserRole.Add(UserRole);
            _context.SaveChanges();

            FormsAuthentication.SetAuthCookie(NewUser.UserName, false);
            return RedirectToAction("Index", "Trainee");
        }


        public ActionResult ConfirmPassword(string Name)
        {
            if(Name == null) { return RedirectToAction("Home"); }

            var ApplicationUser = _context.ApplicationUser.FirstOrDefault(a => a.Email == Name || a.UserName == Name);

            if(ApplicationUser == null) { return RedirectToAction("Home"); }

            MailMessage MyMessage = new MailMessage("t.u.world1996worldweb39@gmail.com", ApplicationUser.Email);

            var CodeEmail = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 10)
      .Select(s => s[new Random().Next(s.Length)]).ToArray());


            MyMessage.Body = "Code : " + CodeEmail;

            MyMessage.Subject = "Confirm Card Code";

            SmtpClient Client = new SmtpClient("smtp.gmail.com", 587);
            Client.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "t.u.world1996worldweb39@gmail.com",
                Password = "Osama1996"
            };

            Client.EnableSsl = true;
            Client.Send(MyMessage);

            var Confirm = new IN_ConfirmPassword()
            {
                UserName = ApplicationUser.UserName,
                CodeEmail = CodeEmail,
                ConfirmPassword = "",
                CodeConfirm = "",
                NewPassword = "",
            };

            return View(Confirm);
        }
        [HttpPost]
        public ActionResult ConfirmPassword(IN_ConfirmPassword model)
        {
            if(model.CodeEmail == "" || model.CodeEmail == null || model.UserName == "" || model.UserName == null)
            {
                return RedirectToAction("Home");
            }
            if(model.CodeConfirm == "" || model.CodeConfirm == null
                || model.NewPassword == "" || model.NewPassword == null|| model.ConfirmPassword == "" || model.ConfirmPassword == null)
            {
                ViewBag.Status = "Make Sure Enter All Values";
                return View(model);
            }

            if(model.NewPassword != model.ConfirmPassword)
            {
                ViewBag.Status = "New Password And Confirm Password Is Not Equal.";
                return View(model);
            }

            if(model.CodeConfirm == model.CodeEmail)
            {
                var ApplicationUser = _context.ApplicationUser.FirstOrDefault(a => a.UserName == model.UserName);
                ApplicationUser.Password = AesOperation.EncryptString(model.NewPassword);
                _context.Entry(ApplicationUser).State = EntityState.Modified;
                _context.SaveChanges();
                if (ApplicationUser.Block)
                {
                    ViewBag.Status = "Your Account Is Blocked";
                    return View(model);
                }

                    Session["UserName"] = ApplicationUser.UserName;
                    Session["NickName"] = ApplicationUser.NickName;
                    Session["ImageName"] = ApplicationUser.ImageName;

                    FormsAuthentication.SetAuthCookie(ApplicationUser.UserName, false);
                    return RedirectToAction("RedirectToMyPage");
            }

            ViewBag.Status = "Make Sure Code";
            return View(model);
        }

        public ActionResult UnAuthorizePage()
        {
            FormsAuthentication.SignOut();
            return View();
        }


        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Home");
        }


        public ActionResult ContactUs(string Name,string Email,string Phone,string Message)
        {
            if(
                Name == "" || Name == null ||
                Email == "" || Email == null ||
                Phone == "" || Phone == null ||
                Message == "" || Message == null 
                )
            {
                return RedirectToAction("RedirectToMyPage", new { StatusContactUS = "Make Sure Enter All Values." });
            }


            MailMessage MyMessage = new MailMessage("t.u.world1996worldweb39@gmail.com", Email);

            MyMessage.Body = "Thanks For Contact Us We Will Contact With You ^_^";

            MyMessage.Subject = "E Think Supporter.";

            SmtpClient Client = new SmtpClient("smtp.gmail.com", 587);
            Client.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "t.u.world1996worldweb39@gmail.com",
                Password = "Osama1996"
            };

            Client.EnableSsl = true;
            Client.Send(MyMessage);

            var NewContactUs = new ContactUs()
            {
                Email = Email,
                Message = Message,
                Name = Name,
                PhoneNumber = Phone
            };

            _context.ContactUs.Add(NewContactUs);
            _context.SaveChanges();

            return RedirectToAction("RedirectToMyPage", new { StatusContactUS = "Done" });
        }
    }
}