using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EDIS.Areas.BMED.Data;
using EDIS.Areas.BMED.Models.KeepModels;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Areas.BMED.Controllers
{
    [Area("BMED")]
    [Authorize]
    public class KeepFormatDtlController : Controller
    {
        private readonly BMEDDbContext _context;

        public KeepFormatDtlController(BMEDDbContext context)
        {
            _context = context;
        }

        // GET: BMED/KeepFormatDtl
        public IActionResult Index()
        {
            return View();
        }

        // GET: BMED/KeepFormatDtl/Details/5
        public IActionResult Details(string id = null, int sno = 0)
        {
            KeepFormatDtlModel keepformat_dtl = _context.BMEDKeepFormatDtls.Find(id, sno);
            if (keepformat_dtl == null)
            {
                return StatusCode(404);
            }
            return View(keepformat_dtl);
        }

        // GET: BMED/KeepFormatDtl/Create
        public IActionResult Create(string id = null)
        {
            if (id != null)
            {
                KeepFormatDtlModel keepformat_dtl = _context.BMEDKeepFormatDtls.Where(d => d.FormatId == id)
                                                                               .OrderByDescending(d => d.Sno)
                                                                               .FirstOrDefault();
                if (keepformat_dtl != null)
                {
                    keepformat_dtl.Sno += 1;
                    keepformat_dtl.Descript = "";
                    return View(keepformat_dtl);
                }
                else
                {
                    keepformat_dtl = new KeepFormatDtlModel();
                    keepformat_dtl.FormatId = id;
                    keepformat_dtl.Sno = 1;
                    return View(keepformat_dtl);
                }
            }
            return View();
        }

        // POST: BMED/KeepFormatDtl/Create
        [HttpPost]
        public IActionResult Create(KeepFormatDtlModel keepformat_dtl)
        {
            if (ModelState.IsValid)
            {
                _context.BMEDKeepFormatDtls.Add(keepformat_dtl);
                _context.SaveChanges();
                return RedirectToAction("Edit", "KeepFormat", new { Area = "BMED", id = keepformat_dtl.FormatId });
            }
            return View(keepformat_dtl);
        }

        // GET: BMED/KeepFormatDtl/Edit/5
        public IActionResult Edit(string id = null, int sno = 0)
        {
            KeepFormatDtlModel keepformat_dtl = _context.BMEDKeepFormatDtls.Find(id, sno);
            if (keepformat_dtl == null)
            {
                return StatusCode(404);
            }
            return View(keepformat_dtl);
        }

        // POST: BMED/KeepFormatDtl/Edit/5
        [HttpPost]
        public IActionResult Edit(KeepFormatDtlModel keepformat_dtl)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(keepformat_dtl).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Edit", "KeepFormat", new { Area = "BMED", id = keepformat_dtl.FormatId });
            }
            return View(keepformat_dtl);
        }

        // GET: BMED/KeepFormatDtl/Delete/5
        public IActionResult Delete(string id = null, int sno = 0)
        {
            KeepFormatDtlModel keepformat_dtl = _context.BMEDKeepFormatDtls.Find(id, sno);
            if (keepformat_dtl == null)
            {
                return StatusCode(404);
            }
            keepformat_dtl.KeepFormat = _context.BMEDKeepFormats.Find(id);
            return View(keepformat_dtl);
        }

        // POST: BMED/KeepFormatDtl/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(KeepFormatDtlModel keepformat_dtl)
        {
            KeepFormatDtlModel kdtl = _context.BMEDKeepFormatDtls.Find(keepformat_dtl.FormatId, keepformat_dtl.Sno);
            if (kdtl != null)
            {
                _context.BMEDKeepFormatDtls.Remove(kdtl);
                _context.SaveChanges();
                return RedirectToAction("Edit", "KeepFormat", new { Area = "BMED", id = keepformat_dtl.FormatId });
            }
            return View(keepformat_dtl);
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
            base.Dispose(disposing);
        }
    }
}