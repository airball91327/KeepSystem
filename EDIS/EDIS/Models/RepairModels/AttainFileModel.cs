using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace EDIS.Models.RepairModels
{
    public class AttainFileModel
    {
        [Key, Column(Order = 1)]
        [Display(Name = "表單類別")]
        public string DocType { get; set; }
        [Key, Column(Order = 2)]
        [Display(Name = "表單編號")]
        public string DocId { get; set; }
        [Key, Column(Order = 3)]
        [Display(Name = "序號")]
        public int SeqNo { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [Display(Name = "摘要")]
        public string Title { get; set; }
        [Required]
        [Display(Name = "檔案連結")]
        public string FileLink { get; set; }
        [Display(Name = "是否公開?")]
        public string IsPublic { get; set; }
        [NotMapped]
        public bool IsPub { get; set; }
        [Display(Name = "財產編號")]
        public string AssetNo { get; set; }
        [Display(Name = "異動人員")]
        public int? Rtp { get; set; }
        [NotMapped]
        public string UserName { get; set; }
        [Display(Name = "異動時間")]
        public DateTime Rtt { get; set; }
        [NotMapped]
        public List<IFormFile> Files { get; set; }
    }
}
