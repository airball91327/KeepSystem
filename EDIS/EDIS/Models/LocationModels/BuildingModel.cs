using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Models.LocationModels
{
    public class BuildingModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BuildingId { get; set; }
        [Display(Name = "建築名稱")]
        public string BuildingName { get; set; }
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
    public class FloorModel
    {
        [Key, Column(Order = 1)]
        public int BuildingId { get; set; }
        [Key, Column(Order = 2)]
        public string FloorId { get; set; }
        [Display(Name = "樓層名稱")]
        public string FloorName { get; set; }
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
    public class PlaceModel
    {
        [Key, Column(Order = 1)]
        public int BuildingId { get; set; }
        [Key, Column(Order = 2)]
        public string FloorId { get; set; }
        [Key, Column(Order = 3)]
        [Display(Name = "地點代號")]
        public string PlaceId { get; set; }
        [Display(Name = "地點名稱")]
        public string PlaceName { get; set; }
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
