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

namespace EDIS.Components.RepairCost
{
    public class RepCostPrintViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<RepairFlowModel, string[]> _repflowRepo;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public RepCostPrintViewComponent(ApplicationDbContext context,
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
            List<RepairCostModel> rc = _context.RepairCosts.Where(c => c.DocId == id).ToList();
            rc.ForEach(r => {
                if (r.StockType == "0")
                    r.StockType = "庫存";
                else
                    r.StockType = "發票";
            });
            return View(rc);
        }
    }
}
