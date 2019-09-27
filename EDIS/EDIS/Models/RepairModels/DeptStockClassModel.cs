using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Models.RepairModels
{
    public class DeptStockClassModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "庫存類別編號")]
        public int StockClsId { get; set; }
        [Required]
        [Display(Name = "庫存類別名稱")]
        public string StockClsName { get; set; }
        [Display(Name = "是否顯示")]
        public string Flg { get; set; }
        [Display(Name = "異動人員")]
        public int Rtp { get; set; }
        [NotMapped]
        [Display(Name = "異動人員帳號")]
        public string RtpName { get; set; }
        [Display(Name = "異動時間")]
        public DateTime Rtt { get; set; }
    }
}
