using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Areas.BMED.Models.RepairModels
{
    [Table("BMEDRepairDtls")]
    public class RepairDtlModel
    {
        [Key]
        public string DocId { get; set; }
        [Display(Name = "處理狀態")]
        public int DealState { get; set; }
        [NotMapped]
        public string DealStateTitle { get; set; }
        [Display(Name = "處理描述")]
        public string DealDes { get; set; }
        [Display(Name = "處理狀態")]
        public string DealState2 { get; set; }
        [Required(ErrorMessage = "必填寫欄位")]
        [Display(Name = "故障描述")]
        public int FailFactor { get; set; }
        [NotMapped]
        public string FailFactorTitle { get; set; }
        [Display(Name = "故障描述")]
        public string FailFactor2 { get; set; }
        [Display(Name = "維修方式")]
        public string InOut { get; set; }
        [NotMapped]
        [Display(Name = "財產編號")]
        public string AssetNo { get; set; }
        [NotMapped]
        [Display(Name = "儀器名稱")]
        public string AssetName { get; set; }
        [NotMapped]
        [Display(Name = "立帳日")]
        public string AssetAccDate { get; set; }
        [Display(Name = "工時")]
        public decimal Hour { get; set; }
        [Display(Name = "[有][無]費用")]
        public string IsCharged { get; set; }
        [Display(Name = "費用")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Cost { get; set; }
        [Display(Name = "[是][否]為統包")]
        public string NotInExceptDevice { get; set; }
        [Display(Name = "[是][否]為器械")]
        public string IsInstrument { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "完工日期")]
        public DateTime? EndDate { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "完帳日期")]
        public DateTime? CloseDate { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "關帳日期")]
        public DateTime? ShutDate { get; set; }
        [NotMapped]
        public List<SelectListItem> DealStates { get; set; }
        [NotMapped]
        public List<SelectListItem> FailFactors { get; set; }
        [NotMapped]
        [Display(Name = "結案驗收人")]
        public string CheckerName { get; set; }
    }
}
