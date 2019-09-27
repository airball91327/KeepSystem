using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDIS.Areas.BMED.Models.RepairModels
{
    [Table("BMEDRepairEmps")]
    public class RepairEmpModel
    {
        [Key, Column(Order = 1)]
        public string DocId { get; set; }
        [Key, Column(Order = 2)]
        [Display(Name = "代號")]
        public int UserId { get; set; }
        [NotMapped]
        [Display(Name = "工程師代號")]
        public string UserName { get; set; }
        [NotMapped]
        [Display(Name = "工程師姓名")]
        public string FullName { get; set; }
        [Display(Name = "小時")]
        public int Hour { get; set; }
        [Display(Name = "分鐘")]
        public int Minute { get; set; }
        [NotMapped]
        public List<SelectListItem> Users { get; set; }
    }
}