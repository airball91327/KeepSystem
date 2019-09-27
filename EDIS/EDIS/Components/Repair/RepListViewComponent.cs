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

namespace EDIS.Components.Repair
{
    public class RepListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public RepListViewComponent(ApplicationDbContext context,
                                  IRepository<AppUserModel, int> userRepo,
                                  CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<RepairListVModel> rv = new List<RepairListVModel>();
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();

            _context.RepairFlows.Where(f => f.Status == "?")
                .Where(f => f.UserId == ur.Id)
                .Select(f => new
                {
                    f.DocId,
                    f.UserId,
                    f.Status,
                    f.Cls
                })
                .Join(_context.Repairs, f => f.DocId, r => r.DocId,
                (f, r) => new
                {
                    repair = r,
                    flow = f
                })
                .Join(_context.Assets, r => r.repair.AssetNo, a => a.AssetNo,
                (r, a) => new
                {
                    repair = r.repair,
                    asset = a,
                    flow = r.flow
                })
                .Join(_context.RepairDtls, m => m.repair.DocId, d => d.DocId,
                (m, d) => new
                {
                    repair = m.repair,
                    asset = m.asset,
                    flow = m.flow,
                    repdtl = d
                })
                .Join(_context.Departments, j => j.repair.AccDpt, d => d.DptId,
                (j, d) => new
                {
                    repair = j.repair,
                    asset = j.asset,
                    flow = j.flow,
                    repdtl = j.repdtl,
                    dpt = d
                }).ToList()
                .ForEach(j => rv.Add(new RepairListVModel
                {
                    DocType = "請修",
                    DocId = j.repair.DocId,
                    ApplyDate = j.repair.ApplyDate,
                    AssetNo = j.repair.AssetNo,
                    AssetName = j.repair.AssetName,
                    Brand = j.asset.Brand,
                    PlaceLoc = j.repair.Area,
                    Type = j.asset.Type,
                    ApplyDpt = j.repair.DptId,
                    AccDpt = j.repair.AccDpt,
                    AccDptName = j.dpt.Name_C,
                    TroubleDes = j.repair.TroubleDes,
                    DealState = _context.DealStatuses.Find(j.repdtl.DealState).Title,
                    DealDes = j.repdtl.DealDes,
                    Cost = j.repdtl.Cost,
                    Days = DateTime.Now.Subtract(j.repair.ApplyDate).Days,
                    Flg = j.flow.Status,
                    FlowUid = j.flow.UserId,
                    FlowCls = j.flow.Cls
                }));

            return View(rv);
        }
    }
}
