using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Models.RepairModels
{
    [Table("RepairShutRecords")]
    public class RepairShutRecordsModel
    {
        [Key]
        [Display(Name = "請修單號")]
        public string DocId { get; set; }
        [Display(Name = "關帳人員")]
        public int Rtp { get; set; }
        [Display(Name = "異動時間")]
        public DateTime Rtt { get; set; }
    }
}
