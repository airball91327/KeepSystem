using EDIS.Areas.BMED.Data;
using EDIS.Areas.BMED.Models.RepairModels;
using EDIS.Models.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Areas.BMED.Components.RepairCost
{
    public class BMEDRepCostListViewComponent : ViewComponent
    {
        private readonly BMEDDbContext _context;
        private readonly CustomUserManager userManager;

        public BMEDRepCostListViewComponent(BMEDDbContext context,
                                            CustomUserManager customUserManager)
        {
            _context = context;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id, string viewType)
        {
            List<RepairCostModel> rc = _context.BMEDRepairCosts.Include(r => r.TicketDtl)
                                                               .Where(c => c.DocId == id).ToList();
            rc.ForEach(r => {
                if (r.StockType == "0")
                    r.StockType = "庫存";
                else if (r.StockType == "2")
                    r.StockType = "發票";
                else
                    r.StockType = "簽單";
            });

            /* Check the device's contract. */
            var repairDtl = _context.BMEDRepairDtls.Find(id);
            if (repairDtl.NotInExceptDevice == "Y") //該案件為統包
            {
                ViewData["HideCost"] = "Y";
            }
            else
            {
                ViewData["HideCost"] = "N";
            }

            if (viewType.Contains("Edit"))
            {
                return View(rc);
            }
            return View("List2", rc);
        }
    }
}
