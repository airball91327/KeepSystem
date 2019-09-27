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

namespace EDIS.Areas.Mobile.Components.Repair
{
    public class mRepIndexViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;

        public mRepIndexViewComponent(ApplicationDbContext context,
                                      IRepository<AppUserModel, int> userRepo)
        {
            _context = context;
            _userRepo = userRepo;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var ur = _userRepo.Find(us => us.UserName == this.User.Identity.Name).FirstOrDefault();

            List<SelectListItem> FlowlistItem = new List<SelectListItem>();
            FlowlistItem.Add(new SelectListItem { Text = "待簽核", Value = "待簽核" });
            FlowlistItem.Add(new SelectListItem { Text = "流程中", Value = "流程中" });
            FlowlistItem.Add(new SelectListItem { Text = "已結案", Value = "已結案" });
            ViewData["FLOWTYPE"] = new SelectList(FlowlistItem, "Value", "Text", "待簽核");

            /* 成本中心的下拉選單資料(關卡在登入者身上的案件) */
            var userRepairs = _context.Repairs.Join(_context.RepairFlows.Where(f => f.Status == "?" && f.UserId == ur.Id),
                                               r => r.DocId, f => f.DocId,
                                               (r, f) => new
                                               {
                                                   repair = r,
                                                   flow = f
                                               }).ToList();
            var accDpts = userRepairs.GroupBy(r => r.repair.AccDpt).Select(group => group.FirstOrDefault()).ToList();
            var departments = _context.Departments.ToList();
            List<SelectListItem> listItem = new List<SelectListItem>();
            foreach(var item in accDpts)
            {
                var dpt = departments.Where(d => d.DptId == item.repair.AccDpt).FirstOrDefault();
                listItem.Add(new SelectListItem
                {
                    Text = dpt.Name_C + "(" + dpt.DptId + ")",    //show DptName(DptId)
                    Value = dpt.DptId
                });
            }
            ViewData["ACCDPT"] = new SelectList(listItem, "Value", "Text");

            /* 處理狀態的下拉選單 */
            List<SelectListItem> listItem2 = new List<SelectListItem>();
            listItem2.Add(new SelectListItem { Text = "未處理", Value = "未處理" });
            listItem2.Add(new SelectListItem { Text = "處理中", Value = "處理中" });
            ViewData["DealStatus"] = new SelectList(listItem2, "Value", "Text", "未處理");

            QryRepListData data = new QryRepListData();

            return View(data);
        }
    }
}
