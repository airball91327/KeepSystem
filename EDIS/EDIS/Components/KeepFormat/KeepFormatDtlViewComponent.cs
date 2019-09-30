using EDIS.Data;
using EDIS.Models.KeepModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.KeepFormat
{
    public class KeepFormatDtlViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public KeepFormatDtlViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id = null)
        {
            if (id != null)
            {
                ViewData["fid"] = id;
                return View(_context.KeepFormatDtls.Where(d => d.FormatId == id).ToList());
            }
            return View(_context.KeepFormatDtls.ToList());
        }
    }
}
