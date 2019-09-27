using System;
using System.ComponentModel.DataAnnotations;

namespace EDIS.Areas.BMED.Models.RepairModels
{
    public class EngsInAssetsViewModel
    {
        [Display(Name = "選取")]
        public Boolean IsSelected { get; set; }
        [Display(Name = "財產編號")]
        public string AssetNo { get; set; }
        [Display(Name = "醫工碼")]
        public string BmedNo { get; set; }
        [Display(Name = "設備類別")]
        public string AssetClass { get; set; }
        [Display(Name = "儀器名稱")]
        public string Cname { get; set; }
        [Display(Name = "負責工程師代號")]
        public int? EngId { get; set; }
        [Display(Name = "負責工程師帳號")]
        public string UserName { get; set; }
        [Display(Name = "負責工程師")]
        public string EngFullName { get; set; }
        [Display(Name = "異動人員ID")]
        public int? Rtp { get; set; }
        [Display(Name = "異動人員帳號")]
        public string RtpName { get; set; }
        [Display(Name = "異動時間")]
        public DateTime? Rtt { get; set; }
    }
}
