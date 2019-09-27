using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDIS.Areas.BMED.Models.KeepModels
{
    [Table("BMEDKeepFormats")]
    public class KeepFormatModel
    {
        [Key]
        [Display(Name = "保養格式代號")]
        public string FormatId { get; set; }
        [Display(Name = "可套用的設備")]
        public string Plants { get; set; }
        [Display(Name = "格式")]
        public string Format { get; set; }
        [Display(Name = "異動人員")]
        public int? Rtp { get; set; }
        [Display(Name = "異動時間")]
        public DateTime? Rtt { get; set; }
    }
}
