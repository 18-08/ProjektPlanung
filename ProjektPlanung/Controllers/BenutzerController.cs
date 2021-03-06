﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ProjektPlanung.Models;

namespace ProjektPlanung.Controllers
{
    public class BenutzerController : Controller
    {
        // Registrierung 
        [HttpGet]
        public ActionResult Registrierung()
        {
            return View();
        }
        
        // Registrierung POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrierung([Bind(Exclude = "istBestaetigt,AktivierungsCode")] Benutzer Benutzer)
        {
            bool Status = false;
            string Message = "";

            // Model Validation
            if(ModelState.IsValid)
            {
                //Email bereits vorhanden
                var existiert = ExistiertEmail(Benutzer.Mail);
                if (existiert)
                {
                    ModelState.AddModelError("EmailExistiert", "Email existiert bereits");
                    return View(Benutzer);
                }

                //Aktivierungscode generieren
                Benutzer.AktivierungsCode = Guid.NewGuid();

                // Hashing Passwort
                Benutzer.Passwort = Crypto.Hash(Benutzer.Passwort);
                Benutzer.bestPasswort = Crypto.Hash(Benutzer.bestPasswort);

                Benutzer.istBestaetigt = false;

                // Speichern der Daten
                using (DatabaseEntities dc = new DatabaseEntities())
                {
                    dc.Database.Connection.Open();
                    dc.Benutzers.Add(Benutzer);
                    dc.SaveChanges();
                }

                // Email senden
                SendeEmail(Benutzer.Mail, Benutzer.AktivierungsCode.ToString());
                Message = "Registrierung erfolgreich. Account Aktivierungslink wurde ihnen an folgende Email Adresse zugesandt: " + Benutzer.Mail;
                Status = true; 
            }
            else
            {
                Message = "Bad request";
            }







            ViewBag.Message = Message;
            ViewBag.Status = Status;
            return View(Benutzer);
        }

        //Verify Account
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;

            using (DatabaseEntities db = new DatabaseEntities())
            {
                db.Configuration.ValidateOnSaveEnabled = false;
                var v = db.Benutzers.Where(a => a.AktivierungsCode == new Guid(id)).FirstOrDefault();

                if (v != null)
                {
                    v.istBestaetigt = true;
                    db.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Ungültige Anfrage";
                }
            }

            ViewBag.Status = Status;
            return View();
        }

        //Login
        [HttpGet]
        public ActionResult LogIn()
        {
            return View();
        }

        //Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LogIn login, string ReturnUrl = "")
        {
            string message = "";

            using (DatabaseEntities db = new DatabaseEntities())
            {
                var v = db.Benutzers.Where(a => a.Mail == login.Mail).FirstOrDefault();
                if(v != null)
                {
                    if(string.Compare(Crypto.Hash(login.Passwort), v.Passwort) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20;
                        var ticket = new FormsAuthenticationTicket(login.Mail, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                        if(Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        message = "Passwort falsch";
                    }
                }
                else
                {
                    message = "Email Adresse oder Passwort falsch";
                }
            }

            ViewBag.Message = message;
            return View();
        }

        //LogOut
        [Authorize]
        [HttpPost]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Benutzer");
        }

        [NonAction]
        public bool ExistiertEmail(string mail)
        {
            using (DatabaseEntities db = new DatabaseEntities())
            {
                var v = db.Benutzers.Where(a => a.Mail == mail).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void SendeEmail(string mail, string aktivierungsCode)
        {
            var verifyUrl = "/Benutzer/VerifyAccount/" + aktivierungsCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("projekteinsatzplanungazubi@gmail.com", "Projekt Planung");
            var toEmail = new MailAddress(mail);
            var fromEmailPasswort = "Einsatzplanung"; /////////REEEEEEEEEEEEEEEEEEEPPPPPPPPPPPPPLLLLLLLLLLLLLLLAAAAAAAAAAAAAAACCCCCCCCCCEEEEEEEEEEEEEEEEE
            string subject = "Dein Account wurde erfolgreich erstellt!";
            string body = "<br/><br/>Diese Email bestätigt die erfolgreiche Erstellung deines Accounts bei der Projekt Planung.<br/>Bitte klicke auf den folgenden Link um den Account zu bestätigen.<br/><a href='"+link+"'>"+link+"</a>";
            var smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPasswort)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            })

            smtp.Send(message);
        }
    }
}