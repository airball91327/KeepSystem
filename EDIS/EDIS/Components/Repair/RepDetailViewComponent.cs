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

namespace EDIS.Components.RepairDtl
{
    public class RepDetailViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public RepDetailViewComponent(ApplicationDbContext context,
                                      IRepository<AppUserModel, int> userRepo,
                                      CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            RepairDtlModel repairDtl = _context.RepairDtls.Find(id);
            var ur = _userRepo.Find(us => us.UserName == this.User.Identity.Name).FirstOrDefault();

            repairDtl.DealStateTitle = _context.DealStatuses.Find(repairDtl.DealState).Title;
            if(repairDtl.FailFactor == 0)
            {
                repairDtl.FailFactorTitle = "尚未處理";
            }
            else
            {
                repairDtl.FailFactorTitle = _context.FailFactors.Find(repairDtl.FailFactor).Title;
            }
            return View(repairDtl);
        }
    }
}
