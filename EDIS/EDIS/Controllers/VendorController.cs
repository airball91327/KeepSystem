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

namespace EDIS.Controllers
{
    [Authorize]
    public class VendorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VendorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vendor
        public async Task<IActionResult> Index()
        {
            return View(await _context.Vendors.ToListAsync());
        }

        // GET: Vendor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendorModel = await _context.Vendors
                .SingleOrDefaultAsync(m => m.VendorId == id);
            if (vendorModel == null)
            {
                return NotFound();
            }

            return View(vendorModel);
        }

        // GET: Vendor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vendor/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VendorId,VendorName,Address,Tel,Fax,Email,UniteNo,TaxAddress,TaxZipCode,Contact,ContactTel,ContactEmail,StartDate,EndDate,Status,Kind")] VendorModel vendorModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vendorModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vendorModel);
        }

        // GET: Vendor/Create2
        public IActionResult Create2()
        {
            return PartialView();
        }

        // POST: Vendor/Create2
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create2([Bind("VendorId,VendorName,UniteNo")] VendorModel vendorModel)
        {
            if (ModelState.IsValid)
            {
                var nameExist = _context.Vendors.Where(v => v.VendorName == vendorModel.VendorName).FirstOrDefault();
                var uniteNoExist = _context.Vendors.Where(v => v.UniteNo == vendorModel.UniteNo).FirstOrDefault();
                var idExist = _context.Vendors.Where(v => v.VendorId == Convert.ToInt32(vendorModel.UniteNo)).FirstOrDefault();

                if (nameExist != null)
                {
                    ModelState.AddModelError("VendorName", "已有相同廠商名稱!");
                    return PartialView(vendorModel);
                }
                else if (uniteNoExist != null)
                {
                    ModelState.AddModelError("UniteNo", "已有相同統一編號!");
                    return PartialView(vendorModel);
                }
                else if (idExist != null)
                {
                    ModelState.AddModelError("UniteNo", "已有相同統一編號!");
                    return PartialView(vendorModel);
                }
                else
                {
                    vendorModel.VendorId = Convert.ToInt32(vendorModel.UniteNo);
                    _context.Add(vendorModel);
                    await _context.SaveChangesAsync();
                    return ViewComponent("QryVendor");
                }
            }
            return PartialView(vendorModel);
        }

        // GET: Vendor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendorModel = await _context.Vendors.SingleOrDefaultAsync(m => m.VendorId == id);
            if (vendorModel == null)
            {
                return NotFound();
            }
            return View(vendorModel);
        }

        // POST: Vendor/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VendorId,VendorName,Address,Tel,Fax,Email,UniteNo,TaxAddress,TaxZipCode,Contact,ContactTel,ContactEmail,StartDate,EndDate,Status,Kind")] VendorModel vendorModel)
        {
            if (id != vendorModel.VendorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vendorModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VendorModelExists(vendorModel.VendorId))
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
            return View(vendorModel);
        }

        // GET: Vendor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendorModel = await _context.Vendors
                .SingleOrDefaultAsync(m => m.VendorId == id);
            if (vendorModel == null)
            {
                return NotFound();
            }

            return View(vendorModel);
        }

        // POST: Vendor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vendorModel = await _context.Vendors.SingleOrDefaultAsync(m => m.VendorId == id);
            _context.Vendors.Remove(vendorModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult QryVendor(QryVendor qryVendor)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem() { Text = "", Value = "" });
            if (qryVendor.QryType == "關鍵字")
            {
                _context.Vendors.Where(v => v.VendorName.Contains(qryVendor.KeyWord.Trim()))
                        .ToList()
                        .ForEach(v => {
                            items.Add(new SelectListItem()
                            {
                                Text = v.VendorName,
                                Value = v.VendorId.ToString()
                            });
                        });
            }
            else if (qryVendor.QryType == "統一編號")
            {
                _context.Vendors.Where(v => v.UniteNo == qryVendor.UniteNo)
                        .ToList()
                        .ForEach(v => {
                            items.Add(new SelectListItem()
                            {
                                Text = v.VendorName,
                                Value = v.VendorId.ToString()
                            });
                        });
            }

            if (items.Count() == 1) //No search result.
            {
                items[0].Text = "查無廠商";
            }
            else if (items.Count() == 2) //Only 1 search result.
            {
                items.Remove(items[0]);
            }
            else
            {
                var countVendors = items.Count() - 1;
                items[0].Text = "符合項目共 " + countVendors + " 家，請選擇廠商";
            }

            qryVendor.VendorList = items;

            return PartialView(qryVendor);
        }

        private bool VendorModelExists(int id)
        {
            return _context.Vendors.Any(e => e.VendorId == id);
        }
    }
}
