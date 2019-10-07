using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Areas.BMED.Models.RepairModels
{
    [Table("BMEDDeptStocks")]
    public class DeptStockModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "材料編號")]
        public int StockId { get; set; }
        [Display(Name = "庫存類別")]
        public string StockCls { get; set; }
        [Required]
        [Display(Name = "材料編號")]
        public string StockNo { get; set; }
        [Required]
        [Display(Name = "材料名稱")]
        public string StockName { get; set; }
        [Display(Name = "單位")]
        public string Unite { get; set; }
        [Display(Name = "單價")]
        [DisplayFormat(DataFormatString = "{0:0}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }
        [Display(Name = "數量")]
        public int Qty { get; set; }
        [Display(Name = "安全存量")]
        public int? SafeQty { get; set; }
        [Display(Name = "庫存地點")]
        public string Loc { get; set; }
        [Display(Name = "規格")]
        public string Standard { get; set; }
        [Display(Name = "零件廠牌")]
        public string Brand { get; set; }
        [Display(Name = "狀態")]
        [Required]
        public string Status { get; set; }
        [Display(Name = "異動人員")]
        public int Rtp { get; set; }
        [NotMapped]
        [Display(Name = "異動人員帳號")]
        public string RtpName { get; set; }
        [Display(Name = "異動時間")]
        public DateTime? Rtt { get; set; }
        [Display(Name = "機構")]
        public string CustOrgan_CustId { get; set; }

    }
}
