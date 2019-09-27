using EDIS.Areas.BMED.Data;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.RepairModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.BMEDRepair
{
    public class BMEDRepResignViewComponent : ViewComponent
    {
        private readonly BMEDDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;
        private readonly CustomRoleManager roleManager;

        public BMEDRepResignViewComponent(BMEDDbContext context,
                                          IRepository<AppUserModel, int> userRepo,
                                          CustomUserManager customUserManager,
                                          CustomRoleManager customRoleManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
            roleManager = customRoleManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            /* 處理工程師查詢的下拉選單 */
            var engs = roleManager.GetUsersInRole("MedEngineer").ToList();
            List<SelectListItem> listItem1 = new List<SelectListItem>();
            foreach (string l in engs)
            {
                var u = _context.AppUsers.Where(ur => ur.UserName == l).FirstOrDefault();
                if (u != null)
                {
                    listItem1.Add(new SelectListItem
                    {
                        Text = u.FullName + "(" + u.UserName + ")",
                        Value = u.Id.ToString()
                    });
                }
            }
            ViewData["BMEDassignEngId"] = new SelectList(listItem1, "Value", "Text");

            /* 所有尚未指派工程師的案件 */
            var rps = _context.BMEDRepairs.Where(r => r.EngId == 0);

            List<RepairListVModel> rv = new List<RepairListVModel>();
            rps.Join(_context.BMEDRepairDtls, r => r.DocId, d => d.DocId,
                    (r, d) => new
                    {
                        repair = r,
                        repdtl = d
                    })
                    .Join(_context.Departments, j => j.repair.AccDpt, d => d.DptId,
                    (j, d) => new
                    {
                        repair = j.repair,
                        repdtl = j.repdtl,
                        dpt = d
                    })
                    .ToList()
                    .ForEach(j => rv.Add(new RepairListVModel
                    {
                        DocType = "醫工請修",
                        RepType = j.repair.RepType,
                        DocId = j.repair.DocId,
                        ApplyDate = j.repair.ApplyDate,
                        PlaceLoc = j.repair.PlaceLoc,
                        ApplyDpt = j.repair.DptId,
                        AccDpt = j.repair.AccDpt,
                        AccDptName = j.dpt.Name_C,
                        TroubleDes = j.repair.TroubleDes,
                        Days = DateTime.Now.Subtract(j.repair.ApplyDate).Days,
                        repdata = j.repair
                    }));

            /* 設備編號"有"、"無"的對應，"有"讀取table相關data，"無"只顯示申請人輸入的設備名稱 */
            foreach (var item in rv)
            {
                if (!string.IsNullOrEmpty(item.repdata.AssetNo))
                {
                    var asset = _context.BMEDAssets.Where(a => a.AssetNo == item.repdata.AssetNo).FirstOrDefault();
                    if (asset != null)
                    {
                        item.AssetNo = asset.AssetNo;
                        item.AssetName = asset.Cname;
                        item.Brand = asset.Brand;
                        item.Type = asset.Type;
                    }
                }
                else
                {
                    item.AssetName = item.repdata.AssetName;
                }
            }

            return View(rv);
        }

    }
}
