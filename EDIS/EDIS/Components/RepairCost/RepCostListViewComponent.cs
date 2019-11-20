using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.RepairModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.RepairCost
{
    public class RepCostListViewComponent: ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public RepCostListViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            List<RepairCostModel> rc = _context.RepairCosts.Include(r => r.TicketDtl)
                                                           .Where(c => c.DocId == id).ToList();
            rc.ForEach(r => {
                if (r.VendorId != 0)
                {
                    r.VendorUniteNo = _context.Vendors.Find(r.VendorId).UniteNo;
                }
                if (r.StockType == "0")
                    r.StockType = "庫存";
                else if (r.StockType == "2")
                    r.StockType = "發票(含收據)";
                else if (r.StockType == "4")
                    r.StockType = "零用金";
                else
                    r.StockType = "簽單";
            });
            return View(rc);
        }
    }
}
