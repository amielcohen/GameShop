﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Shop.Utility
{
    public class EmailSender : IEmailSender
    {
        
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mail = "gameshop.project.sce@gmail.com";
            var pw = "jknn fczo prtj jmck";

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(mail, pw)
            };

            return client.SendMailAsync(new MailMessage(
                from: mail,
                to: email,
                subject,
                htmlMessage)
            { IsBodyHtml = true });

        }
    }
}
