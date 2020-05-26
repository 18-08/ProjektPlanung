using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProjektPlanung.Models
{
    public class LogIn
    {
        [Display(Name = "Email Adresse")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email Adresse benötigt!")]
        [DataType(DataType.EmailAddress)]
        public string Mail { get; set; }

        [Display(Name = "Passwort")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Passwort benötigt!")]
        [DataType(DataType.Password)]
        public string Passwort { get; set; }

        public bool RememberMe { get; set; }
    }
}