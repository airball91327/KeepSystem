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
    public class RepCostPrintListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public RepCostPrintListViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string docId)
        {
            List<RepairCostModel> rc = _context.RepairCosts.Include(c => c.TicketDtl).Where(c => c.DocId == docId).ToList();
            return View(rc);
        }
    }
}
