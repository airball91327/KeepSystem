using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS.Data;
using EDIS.Models.RepairModels;
using Microsoft.AspNetCore.Authorization;

namespace EDIS.Areas.Mobile.Controllers
{
    [Area("Mobile")]
    [Authorize]
    public class RepairDtlController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RepairDtlController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Mobile/RepairDtl
        public async Task<IActionResult> Index()
        {
            return View(await _context.RepairDtls.ToListAsync());
        }

        // POST: Mobile/RepairDtl/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RepairDtlModel repairDtl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(repairDtl).State = EntityState.Modified;
                    _context.SaveChanges();

                    return new JsonResult(repairDtl)
                    {
                        Value = new { success = true, error = "" }
                    };
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
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

        // GET: RepairDtl/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var repairDtlModel = await _context.RepairDtls
                .SingleOrDefaultAsync(m => m.DocId == id);
            if (repairDtlModel == null)
            {
                return NotFound();
            }

            return View(repairDtlModel);
        }

        // POST: RepairDtl/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var repairDtlModel = await _context.RepairDtls.SingleOrDefaultAsync(m => m.DocId == id);
            _context.RepairDtls.Remove(repairDtlModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RepairDtlModelExists(string id)
        {
            return _context.RepairDtls.Any(e => e.DocId == id);
        }
    }
}
