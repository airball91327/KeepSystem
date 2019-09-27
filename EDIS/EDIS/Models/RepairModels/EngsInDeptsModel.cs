using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EDIS.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace EDIS.Models.RepairModels
{
    public class EngsInDeptsModel
    {
        [Key, Column(Order = 1)]
        [Display(Name = "工程師代號")]
        [ForeignKey("AppUsers")]
        public int EngId { get; set; }
        [Key, Column(Order = 2)]
        public int BuildingId { get; set; }
        [Key, Column(Order = 3)]
        public string FloorId { get; set; }
        [Key, Column(Order = 4)]
        public string PlaceId { get; set; }
      
        [Display(Name = "部門代號")]
        [ForeignKey("Departments")]
        public string DptId { get; set; }

        [Required]
        [Display(Name = "負責工程師")]
        public string UserName { get; set; }
        //[Display(Name = "異動人員")]
        //public int Rtp { get; set; }
        //[NotMapped]
        //[Display(Name = "異動人員帳號")]
        //public string RtpName { get; set; }
        //[Display(Name = "異動時間")]
        //public DateTime? Rtt { get; set; }

        public virtual AppUserModel AppUsers { get; set; }
        public virtual DepartmentModel Departments { get; set; }
    }
}
