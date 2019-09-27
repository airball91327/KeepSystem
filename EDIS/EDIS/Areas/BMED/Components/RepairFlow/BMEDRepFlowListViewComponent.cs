using EDIS.Areas.BMED.Data;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.RepairModels;
using EDIS.Areas.BMED.Repositories;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Areas.BMED.Components.RepairFlow
{
    public class BMEDRepFlowListViewComponent : ViewComponent
    {
        private readonly BMEDDbContext _context;
        private readonly BMEDIRepository<RepairFlowModel, string[]> _repflowRepo;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public BMEDRepFlowListViewComponent(BMEDDbContext context,
                                            BMEDIRepository<RepairFlowModel, string[]> repairflowRepo,
                                            IRepository<AppUserModel, int> userRepo,
                                            CustomUserManager customUserManager)
        {
            _context = context;
            _repflowRepo = repairflowRepo;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            RepairFlowModel fw = new RepairFlowModel();
            List<RepairFlowModel> flows = new List<RepairFlowModel>();

            _context.BMEDRepairFlows.Where(f => f.DocId == id)
                .Join(_context.AppUsers, f => f.UserId, a => a.Id,
                (f, a) => new
                {
                    DocId = f.DocId,
                    StepId = f.StepId,
                    UserId = f.UserId,
                    UserName = a.FullName + " (" + a.UserName + ")",
                    Opinions = f.Opinions,
                    Role = f.Role,
                    Status = f.Status,
                    Rtt = f.Rtt,
                    Rtp = f.Rtp,
                    Cls = f.Cls
                }).ToList()
                .ForEach(f =>
                {
                    flows.Add(new RepairFlowModel
                    {
                        DocId = f.DocId,
                        StepId = f.StepId,
                        UserId = f.UserId,
                        UserName = f.UserName,
                        Opinions = f.Opinions,
                        Role = f.Role,
                        Status = f.Status,
                        Rtt = f.Rtt,
                        Rtp = f.Rtp,
                        Cls = f.Cls
                    });
                });
            flows = flows.OrderBy(f => f.StepId).ToList();

            foreach(var item in flows)
            {
                if (item.Status != "?")
                {
                    if (item.UserId != item.Rtp)
                    {
                        item.UserName += "(" + _context.AppUsers.Find(item.Rtp).FullName + "代替)";
                    }
                }
            }

            return View(flows);
        }
    }
}
