using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EDIS.Models.RepairModels
{
    public class UnsignCounts
    {
        public int RepairCount { get; set; }
        public int KeepCount { get; set; }
        public int BuyEvalateCount { get; set; }
        public int DeliveryCount { get; set; }
        public int BMEDrepCount { get; set; }
        public int BMEDkeepCount { get; set; }
    }

    public class AssignModel
    {
        [Display(Name = "表單編號")]
        public string DocId { get; set; }
        [Display(Name = "流程提示")]
        public string Hint { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [Display(Name = "簽核選項")]
        public string AssignCls { get; set; }
        [Display(Name = "意見描述")]
        public string AssignOpn { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [Display(Name = "流程關卡")]
        public string FlowCls { get; set; }
        [Display(Name = "廠商")]
        public string FlowVendor { get; set; }
        public string VendorName { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [Display(Name = "關卡人員")]
        public int? FlowUid { get; set; }
        [Display(Name = "目前關卡")]
        public string Cls { get; set; }
        public List<SelectListItem> ClsItems { get; set; }
        public List<SelectListItem> ClsUsers { get; set; }
    }
}
