using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Areas.BMED.Models.RepairModels
{
    [Table("BMEDAssets")]
    public class AssetModel
    {
        [Key]
        [Required(ErrorMessage = "必填寫項目")]
        [Display(Name = "財產編號")]
        public string AssetNo { get; set; }
        [Display(Name = "醫工碼")]
        public string BmedNo { get; set; }
        [Required(ErrorMessage = "必填寫項目")]
        [Display(Name = "設備類別")]
        public string AssetClass { get; set; }
        [Required(ErrorMessage = "必填寫項目")]
        [Display(Name = "中文名稱")]
        public string Cname { get; set; }
        [Display(Name = "英文名稱")]
        public string Ename { get; set; }
        [Display(Name = "立帳日")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? AccDate { get; set; }
        [Display(Name = "購入日")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? BuyDate { get; set; }
        [Display(Name = "出廠日")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? RelDate { get; set; }
        [Display(Name = "廠牌")]
        public string Brand { get; set; }
        [Display(Name = "規格")]
        public string Standard { get; set; }
        [Display(Name = "型號")]
        public string Type { get; set; }
        [Display(Name = "產地")]
        public string Origin { get; set; }
        [Display(Name = "電壓")]
        public string Voltage { get; set; }
        [Display(Name = "電流")]
        public string Current { get; set; }
        [Display(Name = "代理商")]
        public int? VendorId { get; set; }
        [NotMapped]
        [Display(Name = "廠商名稱")]
        public string VendorName { get; set; }
        [Required(ErrorMessage ="必填寫項目")]
        [Display(Name = "處分性質")]
        public string DisposeKind { get; set; }
        [Required(ErrorMessage = "必填寫項目")]
        [Display(Name = "保管部門")]
        public string DelivDpt { get; set; }
        [NotMapped]
        [Display(Name = "保管部門名稱")]
        public string DelivDptName { get; set; }
        [Required(ErrorMessage = "必填寫項目")]
        [Display(Name = "保管人代號")]
        public int? DelivUid { get; set; }
        [Display(Name = "保管人姓名")]
        public string DelivEmp { get; set; }
        [Display(Name = "存放地點")]
        public string LeaveSite { get; set; }
        [Required(ErrorMessage = "必填寫項目")]
        [Display(Name = "成本中心")]
        public string AccDpt { get; set; }
        [NotMapped]
        [Display(Name = "成本中心名稱")]
        public string AccDptName { get; set; }
        [Display(Name = "成本")]
        public Nullable<decimal> Cost { get; set; }
        [Display(Name = "計算基數")]
        public Nullable<decimal> Shares { get; set; }
        [Display(Name = "風險等級")]
        public string RiskLvl { get; set; }
        [Display(Name = "使用年限")]
        public int? UseLife { get; set; }
        [Display(Name = "固定IP位址")]
        public string IpAddr { get; set; }
        [Display(Name = "MAC位址")]
        public string MacAddr { get; set; }
        [Display(Name = "製造號碼")]
        public string MakeNo { get; set; }
        [Display(Name = "備註")]
        public string Note { get; set; }
        [Display(Name = "採購評估單號")]
        public string Docid { get; set; } //採購評估單號
        [Display(Name = "設備工程師")]
        public int? AssetEngId { get; set; }
        [Display(Name = "工程師姓名")]
        public string AssetEngName { get; set; }

        [Display(Name = "異動人員")]
        public int? Rtp { get; set; }

        [Display(Name = "異動時間")]
        public DateTime? Rtt { get; set; }
        [NotMapped]
        public string upload { get; set; }
    }

    [Table("BMEDDeviceClassCodes")]
    public class DeviceClassCode
    {
        [Display(Name = "設備分類碼")]
        [Key]
        public string M_code { get; set; }
        [Required(ErrorMessage = "必填寫項目")]
        [Display(Name = "設備分類名稱")]
        public string M_name { get; set; }
    }

    public class RepAssetVModel
    {
        [Display(Name = "購入日")]
        public DateTime? BuyDate { get; set; }
        [Display(Name = "廠商")]
        public string VenderName { get; set; }
        [Display(Name = "成本")]
        public decimal? Cost { get; set; }

    }

    public class QryAsset
    {
        [Display(Name = "成本中心")]
        public string AccDpt { get; set; }
        [Display(Name = "保管部門")]
        public string DelivDpt { get; set; }
        [Display(Name = "財產編號")]
        public string AssetNo { get; set; }
        [Display(Name = "設備名稱")]
        public string AssetName { get; set; }
        [Display(Name = "醫工碼")]
        public string BmedNo { get; set; }
        [Display(Name = "型號")]
        public string Type { get; set; }

    }
}
