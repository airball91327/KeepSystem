using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS.Data;
using EDIS.Models.RepairModels;
using Microsoft.AspNetCore.Http;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;

namespace EDIS.Controllers
{
    [Authorize]
    public class DeptStockController : Controller
    {
        private readonly ApplicationDbContext _context;
        private int pageSize = 100;

        public DeptStockController(ApplicationDbContext context)
        {
            _context = context;
        }

        public ActionResult Index(IFormCollection fm, int page = 1)
        {
            string stockClsId = fm["StockClsId"];
            string stockItemId = fm["StockItemId"];
            string stockno = fm["qtySTOCKNO"];
            string dname = fm["qtyDEPTNAME"];
            string brand = fm["qtyBRAND"];
            List<DeptStockModel> dv = _context.DeptStocks.ToList();
            if (!string.IsNullOrEmpty(stockClsId))
            {
                int clsId = Convert.ToInt32(stockClsId);
                dv = dv.Where(d => d.StockClsId == clsId).ToList();
            }
            if (!string.IsNullOrEmpty(stockItemId))
            {
                int itemId = Convert.ToInt32(stockItemId);
                dv = dv.Where(d => d.StockItemId == itemId).ToList();
            }
            if (!string.IsNullOrEmpty(stockno))
            {
                dv = dv.Where(d => d.StockNo.Contains(stockno)).ToList();
            }
            if (!string.IsNullOrEmpty(dname))
            {
                dv = dv.Where(d => d.StockName.Contains(dname)).ToList();
            }
            if (!string.IsNullOrEmpty(brand))
            {
                dv = dv.Where(d => d.Brand == brand.ToUpper()).ToList();
            }
            if (dv.ToPagedList(page, pageSize).Count <= 0)
                return PartialView("List", dv.ToPagedList(1, pageSize));

            return PartialView("List", dv.ToPagedList(page, pageSize));
        }

        // GET: DeptStock/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deptStock = await _context.DeptStocks
                .SingleOrDefaultAsync(m => m.StockId == id);
            if (deptStock == null)
            {
                return NotFound();
            }

            return View(deptStock);
        }

        // GET: DeptStock/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DeptStock/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StokId,StokCls,StokNo,StokNam,Unite,Price,Qty,SafeQty,Loc,Standard,Brand,Status,Rtp,Rtt,CustOrgan_CustId")] DeptStockModel deptStockModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(deptStockModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(deptStockModel);
        }

        // GET: DeptStock/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deptStockModel = await _context.DeptStocks.SingleOrDefaultAsync(m => m.StockId == id);
            if (deptStockModel == null)
            {
                return NotFound();
            }
            return View(deptStockModel);
        }

        // POST: DeptStock/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StokId,StokCls,StokNo,StokNam,Unite,Price,Qty,SafeQty,Loc,Standard,Brand,Status,Rtp,Rtt,CustOrgan_CustId")] DeptStockModel deptStockModel)
        {
            if (id != deptStockModel.StockId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deptStockModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeptStockModelExists(deptStockModel.StockId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(deptStockModel);
        }

        // GET: DeptStock/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deptStockModel = await _context.DeptStocks
                .SingleOrDefaultAsync(m => m.StockId == id);
            if (deptStockModel == null)
            {
                return NotFound();
            }

            return View(deptStockModel);
        }

        // POST: DeptStock/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deptStockModel = await _context.DeptStocks.SingleOrDefaultAsync(m => m.StockId == id);
            _context.DeptStocks.Remove(deptStockModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: DeptStock/GetItems/5
        public JsonResult GetItems(int stockClsId)
        {
            var itemsOfClass = _context.DeptStockItems.Where(d => d.StockClsId == stockClsId);
            var items = itemsOfClass.Select(i => new
            {
                stockItemId = i.StockItemId,
                stockItemName = i.StockItemName
            });
            return Json(items);
        }

        public ActionResult List()
        {
            return PartialView();
        }

        private bool DeptStockModelExists(int id)
        {
            return _context.DeptStocks.Any(e => e.StockId == id);
        }
    }
}
