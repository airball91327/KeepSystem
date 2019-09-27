using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Models.RepairModels
{
    public class RepairModel
    {
        [Key]
        [Display(Name = "表單編號")]
        public string DocId { get; set; }
        [Required]
        [Display(Name ="申請人代號")]
        public int UserId { get; set; }
        [Required]
        [Display(Name = "申請人姓名")]
        public string UserName { get; set; }
        [NotMapped]
        [Display(Name = "申請人帳號")]
        public string UserAccount { get; set; }
        [Required]
        [Display(Name = "所屬部門")]
        public string DptId { get; set; }
        [NotMapped]
        public string DptName { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [Display(Name = "分機")]
        public string Ext { get; set; }
        [Display(Name = "MVPN")]
        public string Mvpn { get; set; }
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        [Display(Name = "申請日期")]
        public DateTime ApplyDate { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [Display(Name = "成本中心")]
        public string AccDpt { get; set; }
        [NotMapped]
        public string AccDptName { get; set; }
        [Display(Name = "擺設地點")]
        public string PlaceLoc { get; set; }
        [Display(Name = "財產編號")]
        public string AssetNo { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [Display(Name = "物品名稱")]
        public string AssetName { get; set; }
        [NotMapped]
        [Display(Name = "立帳日")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? AssetAccDate { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [Display(Name = "請修類別")]
        public string RepType { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [Display(Name = "請修地點")]
        public string LocType { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [Display(Name = "期別")]
        public string Building { get; set; }
        [NotMapped]
        public string BuildingName { get; set; }
        [Display(Name = "樓層")]
        public string Floor { get; set; }
        [NotMapped]
        public string FloorName { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [Display(Name = "地點")] // Table "Place"
        public string Area { get; set; }
        [NotMapped]
        public string AreaName { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [Display(Name = "請修原因")]
        public string TroubleDes { get; set; }
        [Required]
        [Display(Name = "負責工程師")]
        public int EngId { get; set; }
        [NotMapped]
        public string EngName { get; set; }
        [NotMapped]
        [Display(Name = "指定工程師")]
        public int? PrimaryEngId { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [NotMapped]
        [Display(Name = "單位主管")]
        public int? DptMgrId { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [Display(Name = "驗收人")]
        public int? CheckerId { get; set; }
        [NotMapped]
        public string CheckerName { get; set; }
        //
        [NotMapped]
        public List<SelectListItem> Buildings { get; set; }
    }
}
