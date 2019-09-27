using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using EDIS.Data;

namespace EDIS.Components.DeptStock
{
    public class DeptStockIndexViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public DeptStockIndexViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? stockClsId, int? stockItemId)
        {
            ViewBag.StockClsId = new SelectList(_context.DeptStockClasses, "StockClsId", "StockClsName", stockClsId);
            ViewBag.StockItemId = stockItemId;

            return View();
        }
    }
}
