﻿using Helperland.Data;
using Helperland.Functionality;
using Helperland.GlobalVariable;
using Helperland.Models;
using Helperland.Models.ViewModel;
using Helperland.Models.ViewModelRepository;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace Helperland.Controllers
{
    public class AccountController : Controller
    {
        HelperlandContext _helperlandContext;
        MailRequest mailRequest = new MailRequest();

        MD5 md5Hash = MD5.Create();
        public AccountController(HelperlandContext helperlandContext)
        {
            _helperlandContext = helperlandContext;
        }


        [Route("become-a-service-provider")]
        public IActionResult BecomeProvider()
        {
            return View();
        }
        [HttpPost]
        [Route("become-a-service-provider")]
        public IActionResult BecomeProvider(BecomeProviderViewModel becomeProviderViewModel)
        {
            if (ModelState.IsValid)
            {
                BecomeProviderRepository becomeProviderRepository = new BecomeProviderRepository();
                User user = becomeProviderRepository.GetUser(becomeProviderViewModel);
                int noOfSameAccountWithEmail = _helperlandContext.Users.Where(u => u.Email == becomeProviderViewModel.Email).Count();
                if (noOfSameAccountWithEmail == 0)
                {
                    user.Status = new GlobalData().ACCOUNT_STATUS_DEACTIVE;
                    _helperlandContext.Users.Add(user);
                    _helperlandContext.SaveChanges();
                    return RedirectToAction("Index", "Home", new { loginModal = "true" });
                }
                else
                {
                    System.Console.WriteLine("Email already exists");
                }
            }
            else
            {
                System.Console.WriteLine("Data missing");
            }
            return RedirectToAction("SignUp", "Account", new { spSignUpError = "true" });
        }


        [Route("signup")]
        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        [Route("signup")]
        public IActionResult SignUp(SignUpViewModel signUpViewModel)
        {
            if (ModelState.IsValid)
            {
                SignUpRepository signUpRepository = new SignUpRepository();
                User user = signUpRepository.GetUser(signUpViewModel);
                int noOfSameAccountWithEmail = _helperlandContext.Users.Where(u => u.Email == signUpViewModel.Email).Count();
                if (noOfSameAccountWithEmail == 0)
                {
                    _helperlandContext.Users.Add(user);
                    _helperlandContext.SaveChanges();
                    return RedirectToAction("Index", "Home", new { loginModal = "true" });
                }
                else
                {
                    System.Console.WriteLine("Email already exists");
                }
            }
            else
            {
                System.Console.WriteLine("Data missing");
            }
            return RedirectToAction("SignUp", "Account", new { signUpError = "true" });
        }


        [HttpPost]
        public IActionResult Login(HomeViewModel homeViewModel)
        {
            User? user = _helperlandContext.Users.Where(u => u.Email == homeViewModel.Login.Email).FirstOrDefault();

            if (user != null)
            {
                string hashPassword = new MD5Hashing().GetMd5Hash(md5Hash, homeViewModel.Login.Password);
                if (user.Password == hashPassword)
                {
                    GlobalData globalData = new GlobalData();
                    if ((user.UserTypeId == globalData.CustomerTypeId || user.UserTypeId == globalData.SpTypeId || user.UserTypeId == globalData.AdminTypeId) && user.Status == new GlobalData().ACCOUNT_STATUS_ACTIVE)
                    {
                        String token = new TokenGenerator().GetToken();
                        user.KeepMeLoggedInToken = token;
                        Response.Cookies.Append("keepMeLoggedInToken", token);

                        _helperlandContext.Users.Update(user);
                        _helperlandContext.SaveChanges();

                        if (user.UserTypeId == globalData.CustomerTypeId)
                        {
                            return RedirectToAction("ServiceHistory", "Customer");
                        }
                        else if (user.UserTypeId == globalData.SpTypeId)
                        {
                            return RedirectToAction("Dashboard", "ServiceProvider");
                        }
                        else
                        {
                            return RedirectToAction("ServiceRequests", "Admin");
                        }
                    }
                    else
                    {
                        //In case of UserId is not 1 2 3
                        Console.WriteLine("UserId is not valid or you are not approved");
                        return RedirectToAction("Index", "Home", new { notApproved = "true" });
                    }
                }
                else
                {
                    //Password is wrong
                    Console.WriteLine("Email or password is wrong");
                    return RedirectToAction("Index", "Home", new { emailPass = "wrong" });
                }
            }
            else
            {
                //Email is wrong
                Console.WriteLine("User not found");
                return RedirectToAction("Index", "Home", new { emailPass = "wrong" });
            }
        }


        public IActionResult ForgotPassword(HomeViewModel homeViewModel)
        {
            User? user = _helperlandContext.Users.Where(u => u.Email == homeViewModel.Forgot.Email).FirstOrDefault();
            if (user != null)
            {
                String token = new TokenGenerator().GetToken();
                user.ResetToken = token;
                _helperlandContext.Users.Update(user);
                _helperlandContext.SaveChanges();

                var passwordResetLink = Url.Action("ResetPassword", "Account",
                    new { email = homeViewModel.Forgot.Email, token = token }, Request.Scheme);
                Console.WriteLine(passwordResetLink);


                SendMailViewModel sendMailViewModel = new SendMailViewModel();
                sendMailViewModel.Email = homeViewModel.Forgot.Email;
                sendMailViewModel.Name = user.FirstName + " " + user.LastName;
                sendMailViewModel.Subject = "Reset Password";
                sendMailViewModel.Body =
                "Hello,\n" +
                user.FirstName + " " + user.LastName + "\n\n" +
                "Your password reset link is:\n" +
                passwordResetLink;

                Thread threadSendMail = new Thread(mailRequest.SendEmail);
                threadSendMail.Start(sendMailViewModel);
            }
            else
            {
                //Email is wrong
                Console.WriteLine("User not found");
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index", "Home", new { resetLinkSent = "true" });
        }


        public IActionResult ResetPassword(string email, string token)
        {
            User? user = _helperlandContext.Users.Where(u => u.Email == email && u.ResetToken == token).FirstOrDefault();
            if (user != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {

            User? user = _helperlandContext.Users.Where(u => u.Email == resetPasswordViewModel.Email && u.ResetToken == resetPasswordViewModel.Token).FirstOrDefault();
            if (user != null)
            {
                string hashPassword = new MD5Hashing().GetMd5Hash(md5Hash, resetPasswordViewModel.Password);
                user.Password = hashPassword;
                user.ResetToken = null;
                _helperlandContext.Users.Update(user);
                _helperlandContext.SaveChanges();
                return RedirectToAction("Index", "Home", new { loginModal = "true" });
            }
            else
            {
                //In a case of user recored has been deleted
                Console.WriteLine("User not found");
                return RedirectToAction("Error", "Home");
            }
        }


        public IActionResult LogOut()
        {
            String? keepMeLoggedInToken = Request.Cookies["keepMeLoggedInToken"];
            if (keepMeLoggedInToken != null)
            {

                User? user = _helperlandContext.Users.Where(u => u.KeepMeLoggedInToken == keepMeLoggedInToken).FirstOrDefault();
                if (user != null)
                {
                    user.KeepMeLoggedInToken = null;
                    _helperlandContext.Users.Update(user);
                    _helperlandContext.SaveChanges();

                    //Removing all cookies...
                    foreach (var cookie in HttpContext.Request.Cookies)
                    {
                        Response.Cookies.Delete(cookie.Key);
                    }
                }
                else
                {
                    Console.WriteLine("User not found");
                }
            }
            else
            {
                Console.WriteLine("Token not found");
            }
            return RedirectToAction("Index", "Home");
        }







        /*----------->Temporary for second submission<-----------*/
        public IActionResult TempCustomer()
        {
            return View();
        }
        public IActionResult TempServiceProvider()
        {
            return View();
        }
        public IActionResult TempAdmin()
        {
            return View();
        }
    }
}
