using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EDIS.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace EDIS.Models.RepairModels
{
    public class EngDealingDocs
    {
        [Key, Column(Order = 1)]
        [Display(Name = "工程師代號")]
        [ForeignKey("AppUsers")]
        public int EngId { get; set; }

        [Required]
        [Display(Name = "負責案件數")]
        public int DealingDocs { get; set; }

        public virtual AppUserModel AppUsers { get; set; }
    }
}
