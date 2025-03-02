﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Helperland.Models.ViewModel
{
    public class ContactUViewModel
    {
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = null!;

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = null!;

        [Required]
        [Display(Name = "Phone number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        [Display(Name = "E-mail")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Display(Name = "Subject")]
        public string Subject { get; set; } = null!;

        [Required]
        [Display(Name ="Message")]
        public string Message { get; set; } = null!;
    }
}
