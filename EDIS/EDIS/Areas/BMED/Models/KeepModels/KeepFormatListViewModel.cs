using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDIS.Areas.BMED.Models.KeepModels
{
    public class KeepFormatListVModel
    {
        [Display(Name = "表單編號")]
        public string Docid { get; set; }
        [Display(Name = "格式代號")]
        public string FormatId { get; set; }
        [Display(Name = "儀器名稱")]
        public string Plants { get; set; }
        [Display(Name = "序號")]
        public int Sno { get; set; }
        [Display(Name = "項目描述")]
        public string Descript { get; set; }
        [Display(Name = "保養紀錄")]
        public string KeepDes { get; set; }
    }
}
