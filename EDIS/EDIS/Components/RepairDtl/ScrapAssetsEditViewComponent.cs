using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.RepairModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.RepairDtl
{
    public class ScrapAssetsEditViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ScrapAssetsEditViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string docId)
        {
            ScrapAssetModel scrapAssetModel = new ScrapAssetModel();
            scrapAssetModel.DocId = docId;
            return View(scrapAssetModel);
        }
    }
}
