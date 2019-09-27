using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EDIS.Models.Identity;

namespace EDIS.Models.RepairModels
{
    public class EngSubStaff
    {
        [Key]
        [Display(Name = "工程師代號")]
        public int EngId { get; set; }

        [Required]
        [Display(Name = "代理人姓名")]
        public int SubstituteId { get; set; }

        [NotMapped]
        [Display(Name = "代理人")]
        public string SubUserName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        [Display(Name = "開始日期")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        [Display(Name = "結束日期")]
        public DateTime EndDate { get; set; }

        [ForeignKey("EngId")]
        public virtual AppUserModel EngAppUsers { get; set; }
        [ForeignKey("SubstituteId")]
        public virtual AppUserModel SubAppUsers { get; set; }
    }
}
