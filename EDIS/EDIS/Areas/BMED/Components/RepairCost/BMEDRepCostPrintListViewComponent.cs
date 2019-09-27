using EDIS.Areas.BMED.Data;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.RepairModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Areas.BMED.Components.RepairCost
{
    public class BMEDRepCostPrintListViewComponent : ViewComponent
    {
        private readonly BMEDDbContext _context;

        public BMEDRepCostPrintListViewComponent(BMEDDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string docId)
        {
            List<RepairCostModel> rc = _context.BMEDRepairCosts.Include(c => c.TicketDtl).Where(c => c.DocId == docId).ToList();
            return View(rc);
        }
    }
}
