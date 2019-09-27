using System;
using System.ComponentModel.DataAnnotations;

namespace EDIS.Models.RepairModels
{
    public class EngsInDeptsViewModel
    {
        [Display(Name = "選取")]
        public Boolean IsSelected { get; set; }
        [Display(Name = "建築代號")]
        public int BuildingId { get; set; }
        [Display(Name = "建築名稱")]
        public string BuildingName { get; set; }
        [Display(Name = "樓層代號")]
        public string FloorId { get; set; }
        [Display(Name = "樓層名稱")]
        public string FloorName { get; set; }
        [Display(Name = "地點代號")]
        public string PlaceId { get; set; }
        [Display(Name = "地點名稱")]
        public string PlaceName { get; set; }
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
