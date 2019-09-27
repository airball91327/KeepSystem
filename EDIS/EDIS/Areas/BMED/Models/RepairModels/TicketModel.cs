using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Areas.BMED.Models.RepairModels
{
    [Table("BMEDTickets")]
    public class TicketModel
    {
        [Key]
        [Display(Name = "發票號碼")]
        public string TicketNo { get; set; }
        [Display(Name = "發票日期")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? TicDate { get; set; }
        [Display(Name = "廠商代號")]
        public int? VendorId { get; set; }
        [Display(Name = "廠商名稱")]
        public string VendorName { get; set; }
        [Display(Name = "總價")]
        public int TotalAmt { get; set; }
        [Display(Name = "稅額")]
        public int TaxAmt { get; set; }
        [NotMapped]
        [Display(Name = "累計")]
        public decimal Amt { get; set; }
        [Display(Name = "備註")]
        public string Note { get; set; }
        [Display(Name = "殘值")]
        public int ScrapValue { get; set; }
        [Display(Name = "請款日期")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? ApplyDate { get; set; }
        [Display(Name = "取消日期")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? CancelDate { get; set; }
        [Display(Name = "交易代號")]
        public string TradeCode { get; set; }

        public ICollection<TicketDtlModel> TicketDtls { get; set; }
    }
}
