using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.KeepModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.KeepCost
{
    public class KeepCostPrintListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public KeepCostPrintListViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string docId)
        {
            List<KeepCostModel> kc = _context.KeepCosts.Include(c => c.TicketDtl).Where(c => c.DocId == docId).ToList();
            return View(kc);
        }

    }
}
