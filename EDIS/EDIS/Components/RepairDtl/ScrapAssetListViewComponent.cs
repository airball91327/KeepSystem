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

namespace EDIS.Components.RepairDtl
{
    public class ScrapAssetListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ScrapAssetListViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id, int? type)
        {
            List<ScrapAssetModel> scrapAssets = _context.ScrapAssets.Include(s => s.Assets)
                                                                    .Where(s => s.DocId == id).ToList();
            if(type == 2)
            {
                return View("List2", scrapAssets);
            }
            return View(scrapAssets);
        }
    }
}
