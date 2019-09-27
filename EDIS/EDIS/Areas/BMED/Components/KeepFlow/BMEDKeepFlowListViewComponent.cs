using EDIS.Areas.BMED.Data;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.KeepModels;
using EDIS.Areas.BMED.Repositories;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Areas.BMED.Components.KeepFlow
{
    public class BMEDKeepFlowListViewComponent : ViewComponent
    {
        private readonly BMEDDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public BMEDKeepFlowListViewComponent(BMEDDbContext context,
                                             IRepository<AppUserModel, int> userRepo,
                                             CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            List<KeepFlowModel> keepFlow = new List<KeepFlowModel>();
            _context.BMEDKeepFlows.Where(f => f.DocId == id)
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
                    .ForEach(f => {
                        keepFlow.Add(new KeepFlowModel
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
            keepFlow = keepFlow.OrderBy(f => f.StepId).ToList();

            foreach (var item in keepFlow)
            {
                if (item.Status != "?")
                {
                    if (item.UserId != item.Rtp)
                    {
                        item.UserName += "(" + _context.AppUsers.Find(item.Rtp).FullName + "代替)";
                    }
                }
            }

            return View(keepFlow);
        }

    }
}
