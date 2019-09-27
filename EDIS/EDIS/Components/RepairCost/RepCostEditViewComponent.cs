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
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EDIS.Components.RepairCost
{
    public class RepCostEditViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<RepairFlowModel, string[]> _repflowRepo;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public RepCostEditViewComponent(ApplicationDbContext context,
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
            RepairCostModel cost = new RepairCostModel();
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();

            int seqno = _context.RepairCosts.Where(c => c.DocId == id)
                                            .Select(c => c.SeqNo).DefaultIfEmpty().Max();
            cost.DocId = id;
            cost.SeqNo = seqno + 1;
            RepairFlowModel rf = _context.RepairFlows.Where(f => f.DocId == id)
                                                     .Where(f => f.Status == "?").FirstOrDefault();
            var isEngineer = _context.UsersInRoles.Where(u => u.AppRoles.RoleName == "RepEngineer" &&
                                                              u.UserId == ur.Id).FirstOrDefault();
            /* 查無處理中流程 => 已結案 或 已廢除 */
            if (rf == null)
            {
                /* Role => 工務經辦 or Admin */
                if (userManager.IsInRole(this.UserClaimsPrincipal, "RepToDo") == true ||
                    userManager.IsInRole(this.UserClaimsPrincipal, "Admin") == true ||
                    userManager.IsInRole(this.UserClaimsPrincipal, "RepAdmin"))
                {
                    return View(cost);
                }
            }
            if (!(rf.Cls.Contains("工程師") && rf.UserId == ur.Id))    /* 流程 => 其他 */
            {

                if (rf.Cls.Contains("工程師") && isEngineer != null)   /* 流程 => 工程師，Login User => 非負責之工程師 */
                {
                    return View(cost);
                }
                if (userManager.IsInRole(this.UserClaimsPrincipal, "RepToDo") == true ||
                    userManager.IsInRole(this.UserClaimsPrincipal, "Admin") == true ||
                    userManager.IsInRole(this.UserClaimsPrincipal, "RepAdmin")) /* 腳色 => 工務經辦 or Admin */
                {
                    return View(cost);
                }
                List<RepairCostModel> t = _context.RepairCosts.Include(r => r.TicketDtl)
                                                              .Where(c => c.DocId == id).ToList();
                t.ForEach(r => {
                   if (r.StockType == "0")
                       r.StockType = "庫存";
                   else if (r.StockType == "2")
                       r.StockType = "發票(含收據)";
                   else if (r.StockType == "4")
                       r.StockType = "零用金";
                   else
                       r.StockType = "簽單";
                });
                return View("Print", t);
            }
            /* 流程 => 工程師，Login User => 負責之工程師 */
            return View(cost);  
        }
    }
}
