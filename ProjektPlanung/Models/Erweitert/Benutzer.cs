using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ProjektPlanung.Models
{
    [MetadataType(typeof(BenutzerMetaDaten))]
    public partial class Benutzer
    {
        public string bestPasswort { get; set; }
    }

    public class BenutzerMetaDaten
    {
        [Display(Name = "Vorname")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Dieses Feld wird benötigt.")]
        public string Vorname { get; set; }

        [Display(Name = "Nachname")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Dieses Feld wird benötigt.")]
        public string Nachname { get; set; }

        [Display(Name = "Email Adresse")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Dieses Feld wird benötigt.")]
        [DataType(DataType.EmailAddress)]
        public string Mail { get; set; }

        [Display(Name = "Passwort")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Dieses Feld wird benötigt.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Das Passwort muss mindestens 6 Zeichen lang sein.")]
        public string Passwort { get; set; }

        [Display(Name = "Passwort wiederholen")]
        [DataType(DataType.Password)]
        [Compare("Passwort" , ErrorMessage = "Passwörter stimmen nicht überein.")]
        public string bestPasswort { get; set; }
    }
}