using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;


namespace Store.Models
{
    public class EmailSettings
    {
        public string MailToAddress = "vadim124mm@gmail.com";
        public string MailFromAddress = "vadim124mm@gmail.com";
        public bool UseSsl = true;
        public string Username = "vadim124mm@gmail.com";
        public string Password = "fqaugiwivqbzzfsy";
        public string ServerName = "smtp.gmail.com";
        public int ServerPort = 587;
        public bool WriteAsFile = false;
    }

    public class EmailOrderProcessor : IOrderProcessor
    {
        private EmailSettings emailSettings;

        public EmailOrderProcessor(EmailSettings settings)
        {
            emailSettings = settings;
        }
        

        public void ProcessOrder(Cart cart, ShippingDetails shippingInfo)
        {
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.EnableSsl = emailSettings.UseSsl;
                smtpClient.Host = emailSettings.ServerName;
                smtpClient.Port = emailSettings.ServerPort;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password);

               

                StringBuilder body = new StringBuilder()
                    .AppendLine("Новый заказ!")
                    .AppendLine("---");
                   

                foreach (var line in cart.Lines)
                {
                    var subtotal = line.Product.Price * line.Quantity;
                    body.AppendFormat("({0} x {1}: {2} грн ", line.Quantity, line.Product.Name, subtotal);
                }

                body.AppendFormat("| Итого: {0} грн)", cart.ComputeTotalValue())

                    .AppendLine("")
                    .AppendLine(shippingInfo.Name)
                    .AppendLine(shippingInfo.Phone)
                    .AppendLine(shippingInfo.Email)
                    .AppendLine(shippingInfo.Notes)


                    .AppendLine("---")
                    .AppendFormat("Доставка: {0}", shippingInfo.GiftWrap ? "Да" : "Нет")
                    .AppendLine("");
                    body.AppendFormat("Дата: {0}", cart.PostedOnDate());

                MailMessage mailMessage = new MailMessage(
                                        emailSettings.MailFromAddress,
                                        emailSettings.MailToAddress,
                                        "Заказ!",
                                        body.ToString());

                if (emailSettings.WriteAsFile)
                {
                    mailMessage.BodyEncoding = Encoding.UTF8;
                }

                smtpClient.Send(mailMessage);
            }
        }
    }
}
