using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using EDIS.Models.RepairModels;
using EDIS.Data;
using X.PagedList;

namespace EDIS.Components.DeptStock
{
    public class DeptStockListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private int pageSize = 100;

        public DeptStockListViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int page = 1)
        {
            List<DeptStockModel> dv = _context.DeptStocks.ToList();

            if (dv.ToPagedList(page, pageSize).Count <= 0)
                return View(dv.ToPagedList(1, pageSize));

            return View(dv.ToPagedList(page, pageSize));
        }
    }
}
