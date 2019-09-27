using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS.Areas.BMED.Data;
using EDIS.Areas.BMED.Models.RepairModels;
using Microsoft.AspNetCore.Authorization;

namespace EDIS.Areas.BMED.Controllers
{
    [Area("BMED")]
    [Authorize]
    public class RepairDtlController : Controller
    {
        private readonly BMEDDbContext _context;

        public RepairDtlController(BMEDDbContext context)
        {
            _context = context;
        }

        // GET: RepairDtl
        public async Task<IActionResult> Index()
        {
            return View(await _context.BMEDRepairDtls.ToListAsync());
        }

        // GET: RepairDtl/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var repairDtlModel = await _context.BMEDRepairDtls
                .SingleOrDefaultAsync(m => m.DocId == id);
            if (repairDtlModel == null)
            {
                return NotFound();
            }

            return View(repairDtlModel);
        }

        // GET: RepairDtl/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RepairDtl/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DocId,DealState,DealDes,DealState2,FailFactor,FailFactor2,InOut,Hour,IsCharged,Cost,EndDate,CloseDate,ShutDate")] RepairDtlModel repairDtlModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(repairDtlModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(repairDtlModel);
        }

        // POST: RepairDtl/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RepairDtlModel repairDtl)
        {
            if (ModelState.IsValid)
            {
                //if (string.IsNullOrEmpty(repairDtl.DealDes))
                //{
                //    throw new Exception("請輸入[處理描述]!!");
                //}
                try
                {
                    if (repairDtl.IsCharged == "N")
                    {
                        _context.BMEDRepairCosts.RemoveRange(_context.BMEDRepairCosts.Where(c => c.DocId == repairDtl.DocId));
                        repairDtl.Cost = 0;
                    }
                    else
                    {
                        repairDtl.Cost = _context.BMEDRepairCosts.Where(k => k.DocId == repairDtl.DocId)
                                                                 .Select(k => k.TotalCost)
                                                                 .DefaultIfEmpty(0).Sum();
                        int hr = _context.BMEDRepairEmps.Where(p => p.DocId == repairDtl.DocId)
                                                        .Select(p => p.Hour)
                                                        .DefaultIfEmpty(0).Sum();
                        decimal min = _context.BMEDRepairEmps.Where(p => p.DocId == repairDtl.DocId)
                                                             .Select(p => p.Minute)
                                                             .DefaultIfEmpty(0).Sum();
                        repairDtl.Hour = hr + Decimal.Round(min / 60m, 2);
                    }
                    _context.Entry(repairDtl).State = EntityState.Modified;

                    /* Edit AssetNo if No exist. */
                    var repairModel = _context.BMEDRepairs.Find(repairDtl.DocId);
                    repairModel.AssetNo = repairDtl.AssetNo;
                    var tempAsset = _context.BMEDAssets.Where(a => a.AssetNo == repairDtl.AssetNo).FirstOrDefault();
                    if (tempAsset != null)
                    {
                        repairModel.AssetName = tempAsset.Cname;
                    }
                    _context.Entry(repairModel).State = EntityState.Modified;

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

            var repairDtlModel = await _context.BMEDRepairDtls
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
            var repairDtlModel = await _context.BMEDRepairDtls.SingleOrDefaultAsync(m => m.DocId == id);
            _context.BMEDRepairDtls.Remove(repairDtlModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: RepairDtl/PrintRepairDtl
        public IActionResult PrintRepairDtl(string docId)
        {
            return RedirectToAction("PrintRepairDoc", "Repair", new { id = docId });
        }

        private bool RepairDtlModelExists(string id)
        {
            return _context.BMEDRepairDtls.Any(e => e.DocId == id);
        }
    }
}
