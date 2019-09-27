using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDIS.Areas.BMED.Models.KeepModels
{
    [Table("BMEDKeepRecords")]
    public class KeepRecordModel
    {
        [Key, Column(Order = 1)]
        [Display(Name = "表單編號")]
        public string DocId { get; set; }
        [Key, Column(Order = 2)]
        [Display(Name = "格式代號")]
        public string FormatId { get; set; }
        [Key, Column(Order = 3)]
        [Display(Name = "序號")]
        public int Sno { get; set; }
        [Display(Name = "保養項目描述")]
        public string Descript { get; set; }
        [Display(Name = "保養紀錄")]
        public string KeepDes { get; set; }
    }
}
