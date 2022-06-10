using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyEvernote.Entities.ValueObjects
{
    public class LoginViewModel
    {
        [DisplayName("Kullanıcı Adı"), Required(ErrorMessage = "{0} alanı boş geçilemez."), StringLength(40, ErrorMessage = "{0} Max {1} karakter olmalı.")]
        public string Username { get; set; }
        [DisplayName("Şifre"), Required(ErrorMessage = "{0} alanı boş geçilemez."),DataType(DataType.Password), StringLength(40, ErrorMessage = "{0} Max {1} karakter olmalı.")]
        public string Password { get; set; }
    }
}