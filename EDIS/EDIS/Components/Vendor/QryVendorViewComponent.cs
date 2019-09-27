using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.RepairModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.Vendor
{
    public class QryVendorViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public QryVendorViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            QryVendor qryVendor = new QryVendor();
            List<SelectListItem> items = new List<SelectListItem>();
            //items.Add(new SelectListItem { Text = "請選擇",Value = "0" });
            items.Add(new SelectListItem { Text = "", Value = "0" });
            /* 預設廠商資料 */
            items.Add(new SelectListItem { Text = "群益開發生技有限公司", Value = "53266746" });
            items.Add(new SelectListItem { Text = "上揚廣告社", Value = "78622428" });
            items.Add(new SelectListItem { Text = "大山電子材料行", Value = "59165371" });
            items.Add(new SelectListItem { Text = "永美木器行", Value = "58110602" });
            items.Add(new SelectListItem { Text = "全宏鎖店", Value = "93891885" });
            items.Add(new SelectListItem { Text = "育林水電企業有限公司", Value = "23721395" });
            items.Add(new SelectListItem { Text = "雷寶企業有限公司", Value = "84809153" });
            items.Add(new SelectListItem { Text = "建興天井行", Value = "58359814" });
            items.Add(new SelectListItem { Text = "新建源五金行", Value = "59188607" });
            items.Add(new SelectListItem { Text = "協進通信企業行", Value = "05932919" });
            items.Add(new SelectListItem { Text = "建寶鎖店", Value = "20338895" });
            qryVendor.VendorList = items;
            return View(qryVendor);
        }
    }  
}
