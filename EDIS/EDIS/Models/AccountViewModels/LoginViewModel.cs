using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Models.AccountViewModels
{
    public class LoginViewModel
    {

        [Required(ErrorMessage = "必須輸入帳號")]
        [Display(Name = "使用者帳號")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "必須輸入密碼")]
        [Display(Name = "密碼")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        //[Required]
        //[Display(Name = "登入角色")]
        //public string LoginType { get; set; }

        [Display(Name = "保持登入?")]
        public bool RememberMe { get; set; }
    }
}
