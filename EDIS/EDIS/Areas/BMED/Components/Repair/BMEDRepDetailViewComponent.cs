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

namespace EDIS.Areas.BMED.Components.Repair
{
    public class BMEDRepDetailViewComponent : ViewComponent
    {
        private readonly BMEDDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public BMEDRepDetailViewComponent(BMEDDbContext context,
                                          IRepository<AppUserModel, int> userRepo,
                                          CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            RepairDtlModel repairDtl = _context.BMEDRepairDtls.Find(id);
            var ur = _userRepo.Find(us => us.UserName == this.User.Identity.Name).FirstOrDefault();

            /* Get CheckerName from Repair table. */
            var checkerId = _context.BMEDRepairs.Find(id).CheckerId;
            repairDtl.CheckerName = checkerId == 0 ? "" : _context.AppUsers.Find(checkerId).FullName;

            repairDtl.DealStateTitle = _context.BMEDDealStatuses.Find(repairDtl.DealState).Title;
            if (repairDtl.FailFactor == 0)
            {
                repairDtl.FailFactorTitle = "尚未處理";
            }
            else
            {
                repairDtl.FailFactorTitle = _context.BMEDFailFactors.Find(repairDtl.FailFactor).Title;
            }
            return View(repairDtl);
        }
    }
}
