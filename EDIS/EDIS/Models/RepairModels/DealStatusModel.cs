using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace EDIS.Models.RepairModels
{
    public class DealStatusModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [Display(Name = "處理狀況")]
        public string Title { get; set; }
        [Required]
        [Display(Name = "狀態")]
        public string Flg { get; set; }
    }
}
