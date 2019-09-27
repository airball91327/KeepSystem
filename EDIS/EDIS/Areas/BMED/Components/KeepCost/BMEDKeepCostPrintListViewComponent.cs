using EDIS.Areas.BMED.Data;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.KeepModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Areas.BMED.Components.KeepCost
{
    public class BMEDKeepCostPrintListViewComponent : ViewComponent
    {
        private readonly BMEDDbContext _context;

        public BMEDKeepCostPrintListViewComponent(BMEDDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string docId)
        {
            List<KeepCostModel> kc = _context.BMEDKeepCosts.Include(c => c.TicketDtl).Where(c => c.DocId == docId).ToList();
            return View(kc);
        }

    }
}
