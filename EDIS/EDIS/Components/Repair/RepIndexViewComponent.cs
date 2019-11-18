using EDIS.Data;
using EDIS.Models.RepairModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.Repair
{
    public class RepIndexViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public RepIndexViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<SelectListItem> FlowlistItem = new List<SelectListItem>();
            FlowlistItem.Add(new SelectListItem { Text = "待簽核", Value = "待簽核" });
            FlowlistItem.Add(new SelectListItem { Text = "流程中", Value = "流程中" });
            FlowlistItem.Add(new SelectListItem { Text = "已結案", Value = "已結案" });
            ViewData["FLOWTYPE"] = new SelectList(FlowlistItem, "Value", "Text", "待簽核");

            /* 成本中心 & 申請部門 的下拉選單資料 */
            var dptList = new[] { "K", "P", "C"};   //本院部門
            var departments = _context.Departments.Where(d => dptList.Contains(d.Loc)).ToList();
            List<SelectListItem> listItem = new List<SelectListItem>();
            foreach(var item in departments)
            {
                listItem.Add(new SelectListItem
                {
                    Text = item.Name_C + "(" + item.DptId + ")",    //show DptName(DptId)
                    Value = item.DptId
                });
            }
            ViewData["ACCDPT"] = new SelectList(listItem, "Value", "Text");
            ViewData["APPLYDPT"] = new SelectList(listItem, "Value", "Text");

            /* 處理狀態 的下拉選單 */
            var dealStatuses = _context.DealStatuses.ToList();
            List<SelectListItem> listItem2 = new List<SelectListItem>();
            foreach (var item in dealStatuses)
            {
                listItem2.Add(new SelectListItem
                {
                    Text = item.Title,
                    Value = item.Title
                });
            }
            ViewData["DealStatus"] = new SelectList(listItem2, "Value", "Text");

            /* 有無費用 的下拉選單 */
            List<SelectListItem> listItem3 = new List<SelectListItem>();
            listItem3.Add(new SelectListItem { Text = "有", Value = "Y" });
            listItem3.Add(new SelectListItem { Text = "無", Value = "N" });
            ViewData["IsCharged"] = new SelectList(listItem3, "Value", "Text");

            /* 日期查詢 的下拉選單 */
            List<SelectListItem> listItem4 = new List<SelectListItem>();
            listItem4.Add(new SelectListItem { Text = "申請日", Value = "申請日" });
            listItem4.Add(new SelectListItem { Text = "完工日", Value = "完工日" });
            listItem4.Add(new SelectListItem { Text = "結案日", Value = "結案日" });
            ViewData["DateType"] = new SelectList(listItem4, "Value", "Text", "申請日");

            /* 請修類別 的下拉選單 */
            List<SelectListItem> listItem5 = new List<SelectListItem>();
            listItem5.Add(new SelectListItem { Text = "請修", Value = "請修" });
            listItem5.Add(new SelectListItem { Text = "送修", Value = "送修" });
            listItem5.Add(new SelectListItem { Text = "增設", Value = "增設" });
            listItem5.Add(new SelectListItem { Text = "借物", Value = "借物" });
            ViewData["qtyRepType"] = new SelectList(listItem5, "Value", "Text");

            QryRepListData data = new QryRepListData();

            return View(data);
        }
    }
}
