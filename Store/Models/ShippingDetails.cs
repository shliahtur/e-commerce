using System;
using System.ComponentModel.DataAnnotations;

namespace Store.Models
{
    public class ShippingDetails
    {
        [Required(ErrorMessage = "Пожалуйста, укажите Ваше имя")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите мобильный телефон")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите электронную почту")]
        public string Email { get; set; }

        public string Notes { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите населённый пункт")]
        public string City { get; set; }

        public bool GiftWrap { get; set; }
    }
}
