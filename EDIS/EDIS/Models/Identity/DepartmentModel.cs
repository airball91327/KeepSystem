using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EDIS.Models.Identity
{
    public class DepartmentModel
    {
        [Key]
        [Display(Name = "部門代號")]
        public string DptId { get; set; }
        [Display(Name = "中文名稱")]
        public string Name_C { get; set; }
        [Display(Name = "英文名稱")]
        public string Name_E { get; set; }
        [DataType(DataType.EmailAddress)]
        [Display(Name = "部門信箱")]
        public string Email { get; set; }
        [Display(Name = "地理位置")]
        public string Loc { get; set; }
        [Display(Name = "建立日期")]
        public DateTime DateCreated { get; set; }
        [Display(Name = "最後異動日期")]
        public Nullable<DateTime> LastActivityDate { get; set; }
    }
}
