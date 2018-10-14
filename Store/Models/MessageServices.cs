using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;


namespace Store.Models
{
    class MessageServices
    {
        public static async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var _email = "vadim124mm@gmail.com";
                var _epass =  "fqaugiwivqbzzfsy";
                var _dispName = "Store";
                MailMessage myMessage = new MailMessage();
                myMessage.To.Add(email);
                myMessage.From = new MailAddress(_email, _dispName);
                myMessage.Subject = subject;
                myMessage.Body = message;
                myMessage.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.EnableSsl = true;
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(_email, _epass);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.SendCompleted += (s, e) => { smtp.Dispose(); };
                    await smtp.SendMailAsync(myMessage);
                   
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

    }
}