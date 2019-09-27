using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EDIS.Models.Identity;
using EDIS.Models.LocationModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Models.RepairModels
{
    public class FloorEngModel
    {
        [Key, Column(Order = 1)]
        [Display(Name = "負責工程師代號")]
        [ForeignKey("AppUsers")]
        public int EngId { get; set; }
        [Required]
        [Display(Name = "負責工程師")]
        public string EngName { get; set; }
        [Key, Column(Order = 2)]
        public int BuildingId { get; set; }
        [Key, Column(Order = 3)]
        public string FloorId { get; set; }
        [Display(Name = "異動人員")]
        public int Rtp { get; set; }
        [NotMapped]
        [Display(Name = "異動人員帳號")]
        public string RtpName { get; set; }
        [Display(Name = "異動時間")]
        public DateTime? Rtt { get; set; }

        public virtual BuildingModel Buildings { get; set; }
        public virtual FloorModel Floors { get; set; }
        public virtual AppUserModel AppUsers { get; set; }
    }
}
