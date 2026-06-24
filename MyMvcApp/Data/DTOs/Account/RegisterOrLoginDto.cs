using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Data.DTOs.Account
{
    public class RegisterOrLoginDto
    {
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Enter your {0}")]
        [MaxLength(30, ErrorMessage = "{0} cannot be more than {1} characters.")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Enter password")]
        [MaxLength(20, ErrorMessage = "Password cannot be more than {1} characters.")]
        public string Password { get; set; }
    }
}