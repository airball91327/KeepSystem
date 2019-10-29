using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDIS.Models.KeepModels
{
    [Table("KeepFormatDtls")]
    public class KeepFormatDtlModel
    {
        [Key, Column(Order = 1)]
        [Display(Name = "保養格式代號")]
        public string FormatId { get; set; }
        [Key, Column(Order = 2)]
        [Display(Name = "序號")]
        public int Sno { get; set; }
        [Display(Name = "保養項目描述")]
        public string Descript { get; set; }
        [Display(Name = "是否必填")]
        public string IsRequired { get; set; }
        public virtual KeepFormatModel KeepFormat { get; set; }
    }
}
