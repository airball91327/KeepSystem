using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.KeepModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Components.KeepCost
{
    public class KeepCostListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly CustomUserManager userManager;

        public KeepCostListViewComponent(ApplicationDbContext context, 
                                         CustomUserManager customUserManager)
        {
            _context = context;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id, string viewType)
        {
            List<KeepCostModel> kc = _context.KeepCosts.Include(r => r.TicketDtl)
                                                       .Where(c => c.DocId == id).ToList();
            kc.ForEach(r => {
                if (r.StockType == "0")
                    r.StockType = "庫存";
                else if (r.StockType == "2")
                    r.StockType = "發票(含收據)";
                else if (r.StockType == "4")
                    r.StockType = "零用金";
                else
                    r.StockType = "簽單";
            });

            /* Check the device's contract. */
            //var keepDtl = _context.KeepDtls.Find(id);
            //if (keepDtl.NotInExceptDevice == "Y") //該案件為統包
            //{
            //    ViewData["HideCost"] = "Y";
            //}
            //else
            //{
            //    ViewData["HideCost"] = "N";
            //}

            ViewData["HideCost"] = "N";
            if (viewType.Contains("Edit"))
            {
                return View(kc);
            }
            return View("List2", kc);
        }

    }
}
