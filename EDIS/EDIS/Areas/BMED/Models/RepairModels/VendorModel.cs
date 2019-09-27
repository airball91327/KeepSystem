using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Areas.BMED.Models.RepairModels
{
    [Table("BMEDVendors")]
    public class VendorModel
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(Name = "廠商編號")]
        public int VendorId { get; set; }
        [Required]
        [Display(Name = "廠商名稱")]
        public string VendorName { get; set; }
        [Display(Name = "地址")]
        public string Address { get; set; }
        [Display(Name = "聯絡電話")]
        public string Tel { get; set; }
        [Display(Name = "傳真")]
        public string Fax { get; set; }
        [Display(Name = "電子郵件")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [Display(Name = "統一編號")]
        public string UniteNo { get; set; }
        [Display(Name = "稅籍地址")]
        public string TaxAddress { get; set; }
        [Display(Name = "稅籍地址區號")]
        public string TaxZipCode { get; set; }
        [Display(Name = "聯絡人姓名")]
        public string Contact { get; set; }
        [Display(Name = "聯絡人電話")]
        public string ContactTel { get; set; }
        [DataType(DataType.EmailAddress)]
        [Display(Name = "聯絡人Email")]
        public string ContactEmail { get; set; }
        [Display(Name = "開始日期")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "結束日期")]
        public DateTime? EndDate { get; set; }
        [Display(Name = "狀態")]
        public string Status { get; set; }
        [Display(Name = "類別")]
        public string Kind { get; set; }

        //public static IEnumerable<SelectListItem> GetList()
        //{
        //    ApplicationDbContext _context;
        //    List<VendorModel> dt = new List<VendorModel>();
        //    if (Roles.IsUserInRole("Admin"))
        //        dt = _context.Vendors.ToList();
        //    else
        //        dt.Add(_context.Vendors.Find(_context.AppUsers.Find(WebSecurity.CurrentUserId).VendorId));
        //    return new SelectList(dt, "VendorId", "VendorName", "");
        //}
    }

    [Table("VendorType")]
    public class VendorType
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(Name = "類別編號")]
        public int Typid { get; set; }
        [Display(Name = "類別名稱")]
        public string TypName { get; set; }
    }

    public class QryVendor
    {
        [Display(Name = "查詢方式")]
        public string QryType { get; set; }
        [Display(Name = "關鍵字")]
        public string KeyWord { get; set; }
        [Display(Name = "統一編號")]
        public string UniteNo { get; set; }
        [Display(Name = "廠商")]
        public string Vno { get; set; }
        [Display(Name = "廠商列表")]
        public IEnumerable<SelectListItem> VendorList { get; set; }
    }
}
