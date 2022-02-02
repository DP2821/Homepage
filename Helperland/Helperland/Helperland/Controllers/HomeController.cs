﻿using Helperland.Data;
using Helperland.Models;
using Helperland.Models.ViewModel;
using Helperland.Models.ViewModelRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Helperland.Controllers
{
    public class HomeController : Controller
    {

        HelperlandContext _helperlandContext;
    

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger,HelperlandContext helperlandContext)
        {
            _logger = logger;
            _helperlandContext = helperlandContext;
        }

        public IActionResult Index()
        {
            return View();
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
            BecomeProviderRepository becomeProviderRepository = new BecomeProviderRepository();
            User user = becomeProviderRepository.GetUser(becomeProviderViewModel);
            _helperlandContext.Users.Add(user);
            _helperlandContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [Route("prices")]
        public IActionResult Prices()
        {
            return View();
        }

        [Route("contact-us")]
        public IActionResult Contact()
        {
            return View();
        }

        [Route("about")]
        public IActionResult About()
        {
            return View();
        }

        [Route("faqs")]
        public IActionResult Faq()
        {
            return View();
        }

        [Route("contact-us")]
        [HttpPost]
        public IActionResult Contact(ContactUViewModel viewContactU)
        {
            /*MailRequest mailRequest = new MailRequest();
            mailRequest.SendEmail(viewContactU.Email,viewContactU.FirstName,viewContactU.Subject,viewContactU.Message);*/

            _helperlandContext.ContactUs.Add(new ContactUsRepository().GetContactU(viewContactU));
            _helperlandContext.SaveChanges();
            return RedirectToAction("Index");
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
            SignUpRepository signUpRepository = new SignUpRepository();
            User user = signUpRepository.GetUser(signUpViewModel);
            _helperlandContext.Users.Add(user);
            _helperlandContext.SaveChanges();
            return RedirectToAction("Index");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}