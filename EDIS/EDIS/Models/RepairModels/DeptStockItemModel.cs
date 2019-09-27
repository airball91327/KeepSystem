using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Models.RepairModels
{
    public class DeptStockItemModel
    {
        [Key, Column(Order = 1)]
        [ForeignKey("DeptStockClasses")]
        [Display(Name = "庫存類別")]
        public int StockClsId { get; set; }
        [Key, Column(Order = 2)]
        [Display(Name = "庫存品項編號")]
        public int StockItemId { get; set; }
        [Required]
        [Display(Name = "庫存品項名稱")]
        public string StockItemName { get; set; }
        [Display(Name = "是否顯示")]
        public string Flg { get; set; }
        [Display(Name = "異動人員")]
        public int Rtp { get; set; }
        [NotMapped]
        [Display(Name = "異動人員帳號")]
        public string RtpName { get; set; }
        [Display(Name = "異動時間")]
        public DateTime Rtt { get; set; }

        public virtual DeptStockClassModel DeptStockClass { get; set; }
    }
}
