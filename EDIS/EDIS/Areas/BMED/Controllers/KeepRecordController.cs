using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Areas.BMED.Data;
using EDIS.Areas.BMED.Models.KeepModels;
using EDIS.Areas.BMED.Models.RepairModels;
using EDIS.Models;
using EDIS.Models.Identity;
using EDIS.Repositories;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Areas.BMED.Controllers
{
    [Area("BMED")]
    [Authorize]
    public class KeepRecordController : Controller
    {
        private readonly BMEDDbContext _context;

        public KeepRecordController(BMEDDbContext context)
        {
            _context = context;
        }

        // POST: BMED/KeepRecord/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormCollection vmodel)
        {
            if (ModelState.IsValid)
            {
                KeepRecordModel r;
                KeepRecordModel r2;
                int i = vmodel["item.Sno"].Count();
                for (int j = 0; j < i; j++)
                {
                    r = new KeepRecordModel();
                    r.DocId = vmodel["item.DocId"][j];
                    r.FormatId = vmodel["item.FormatId"][j];
                    r.Sno = Convert.ToInt32(vmodel["item.Sno"][j]);
                    r.Descript = vmodel["item.Descript"][j];
                    r.KeepDes = vmodel["item.KeepDes"][j];
                    r2 = _context.BMEDKeepRecords.Find(r.DocId, r.FormatId, r.Sno);
                    if (r2 != null)
                    {
                        r2.KeepDes = r.KeepDes;
                        _context.Entry(r2).State = EntityState.Modified;
                    }
                    else
                    {
                        _context.BMEDKeepRecords.Add(r);
                    }
                }
                try
                {
                    _context.SaveChanges();
                    return new JsonResult(vmodel)
                    {
                        Value = new { success = true, error = "" }
                    };
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                string msg = "";
                foreach (var error in ViewData.ModelState.Values.SelectMany(modelState => modelState.Errors))
                {
                    msg += error.ErrorMessage + Environment.NewLine;
                }
                throw new Exception(msg);
            }
        }
    }
}
