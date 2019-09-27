using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace EDIS.Areas.BMED.Models
{
    [Table("ExceptDevice")]
    public class ExceptDeviceModel
    {
        [Key]
        [Display(Name = "財產編號")]
        public string AssetNo { get; set; }
        [Display(Name = "設備名稱")]
        public string AssetName { get; set; }
        [Display(Name = "廠牌")]
        public string Brand { get; set; }
        [Display(Name = "型號")]
        public string Type { get; set; }
        [Display(Name = "成本中心")]
        public string AccDpt { get; set; }
        [Display(Name = "成本中心名稱")]
        public string AccDptName { get; set; }
    }
}
