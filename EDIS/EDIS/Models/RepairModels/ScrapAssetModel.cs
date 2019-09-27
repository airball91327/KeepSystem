using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Models.RepairModels
{
    public class ScrapAssetModel
    {
        [Key, Column(Order = 1)]
        [Required]
        [Display(Name = "表單編號")]
        public string DocId { get; set; }
        [Key, Column(Order = 2)]
        [Required]
        [ForeignKey("Assets")]
        [Display(Name = "設備編號")]
        public string DeviceNo { get; set; }
        [Key, Column(Order = 3)]
        [Required]
        [Display(Name = "財產編號")]
        public string AssetNo { get; set; }

        public virtual AssetModel Assets { get; set; }
    }
}
