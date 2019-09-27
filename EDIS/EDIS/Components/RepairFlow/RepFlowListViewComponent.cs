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

namespace EDIS.Components.RepairFlow
{
    public class RepFlowListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<RepairFlowModel, string[]> _repflowRepo;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public RepFlowListViewComponent(ApplicationDbContext context,
                                       IRepository<RepairFlowModel, string[]> repairflowRepo,
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

            _context.RepairFlows.Where(f => f.DocId == id)
                .Join(_context.AppUsers, f => f.UserId, a => a.Id,
                (f, a) => new
                {
                    DocId = f.DocId,
                    StepId = f.StepId,
                    UserName = a.FullName + " (" + a.UserName + ")",
                    Opinions = f.Opinions,
                    Role = f.Role,
                    Status = f.Status,
                    Rtt = f.Rtt,
                    Cls = f.Cls
                }).ToList()
                .ForEach(f =>
                {
                    flows.Add(new RepairFlowModel
                    {
                        DocId = f.DocId,
                        StepId = f.StepId,
                        UserName = f.UserName,
                        Opinions = f.Opinions,
                        Role = f.Role,
                        Status = f.Status,
                        Rtt = f.Rtt,
                        Cls = f.Cls
                    });
                });
            flows = flows.OrderBy(f => f.StepId).ToList();

            return View(flows);
        }
    }
}
