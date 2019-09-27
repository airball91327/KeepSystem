using System;
using System.ComponentModel.DataAnnotations;

namespace EDIS.Models.RepairModels
{
    public class RepairReportListVModel
    {
        [Display(Name = "姓名")]
        public string FullName { get; set; }
        [Display(Name = "增設(內修)件數")]
        public int RepAddIns { get; set; }
        [Display(Name = "增設(外修)件數")]
        public int RepAddOuts { get; set; }
        [Display(Name = "增設(內外修)件數")]
        public int RepAddInOuts { get; set; }
        [Display(Name = "增設(未處理)件數")]
        public int RepAddNoDeals { get; set; }
        [Display(Name = "維修(內修)件數")]
        public int RepIns { get; set; }
        [Display(Name = "維修(外修)件數")]
        public int RepOuts { get; set; }
        [Display(Name = "維修(內外修)件數")]
        public int RepInOuts { get; set; }
        [Display(Name = "維修(未處理)件數")]
        public int RepNoDeals { get; set; }
        [Display(Name = "報廢件數")]
        public int RepScraps { get; set; }
        [Display(Name = "總件數")]
        public int RepTotals { get; set; }
        [Display(Name = "平均每人件數")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal RepAvgPerPerson { get; set; }
        [Display(Name = "維修完工率")]
        public string RepEndRate { get; set; }
        [Display(Name = "維修(內修)3日內完成率")]
        public string RepInEndRate1 { get; set; }
        [Display(Name = "4~7日完成率")]
        public string RepInEndRate2 { get; set; }
        [Display(Name = "8日以上完成率")]
        public string RepInEndRate3 { get; set; }
        [Display(Name = "維修(外修、內外修)15日內完成率")]
        public string RepOutEndRate1 { get; set; }
        [Display(Name = "16~30日完成率")]
        public string RepOutEndRate2 { get; set; }
        [Display(Name = "31日以上完成率")]
        public string RepOutEndRate3 { get; set; }
        [Display(Name = "增設15日內完成率")]
        public string RepAddEndRate1 { get; set; }
        [Display(Name = "16~30日完成率")]
        public string RepAddEndRate2 { get; set; }
        [Display(Name = "31日以上完成率")]
        public string RepAddEndRate3 { get; set; }
        [Display(Name = "有費用件數")]
        public int IsChargedReps { get; set; }
        [Display(Name = "無費用件數")]
        public int NoChargedReps { get; set; }
        [Display(Name = "總費用")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalCosts { get; set; }
        [Display(Name = "平均每件維修費用")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal AvgCosts { get; set; }
    }
}
