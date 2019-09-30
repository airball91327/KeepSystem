using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EDIS.Data;
using EDIS.Models.KeepModels;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Controllers
{
    [Authorize]
    public class KeepFormatDtlController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KeepFormatDtlController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: KeepFormatDtl
        public IActionResult Index()
        {
            return View();
        }

        // GET: KeepFormatDtl/Details/5
        public IActionResult Details(string id = null, int sno = 0)
        {
            KeepFormatDtlModel keepformat_dtl = _context.KeepFormatDtls.Find(id, sno);
            if (keepformat_dtl == null)
            {
                return StatusCode(404);
            }
            return View(keepformat_dtl);
        }

        // GET: KeepFormatDtl/Create
        public IActionResult Create(string id = null)
        {
            if (id != null)
            {
                KeepFormatDtlModel keepformat_dtl = _context.KeepFormatDtls.Where(d => d.FormatId == id)
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

        // POST: KeepFormatDtl/Create
        [HttpPost]
        public IActionResult Create(KeepFormatDtlModel keepformat_dtl)
        {
            if (ModelState.IsValid)
            {
                _context.KeepFormatDtls.Add(keepformat_dtl);
                _context.SaveChanges();
                return RedirectToAction("Edit", "KeepFormat", new { id = keepformat_dtl.FormatId });
            }
            return View(keepformat_dtl);
        }

        // GET: KeepFormatDtl/Edit/5
        public IActionResult Edit(string id = null, int sno = 0)
        {
            KeepFormatDtlModel keepformat_dtl = _context.KeepFormatDtls.Find(id, sno);
            if (keepformat_dtl == null)
            {
                return StatusCode(404);
            }
            return View(keepformat_dtl);
        }

        // POST: KeepFormatDtl/Edit/5
        [HttpPost]
        public IActionResult Edit(KeepFormatDtlModel keepformat_dtl)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(keepformat_dtl).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Edit", "KeepFormat", new { id = keepformat_dtl.FormatId });
            }
            return View(keepformat_dtl);
        }

        // GET: KeepFormatDtl/Delete/5
        public IActionResult Delete(string id = null, int sno = 0)
        {
            KeepFormatDtlModel keepformat_dtl = _context.KeepFormatDtls.Find(id, sno);
            if (keepformat_dtl == null)
            {
                return StatusCode(404);
            }
            keepformat_dtl.KeepFormat = _context.KeepFormats.Find(id);
            return View(keepformat_dtl);
        }

        // POST: KeepFormatDtl/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(KeepFormatDtlModel keepformat_dtl)
        {
            KeepFormatDtlModel kdtl = _context.KeepFormatDtls.Find(keepformat_dtl.FormatId, keepformat_dtl.Sno);
            if (kdtl != null)
            {
                _context.KeepFormatDtls.Remove(kdtl);
                _context.SaveChanges();
                return RedirectToAction("Edit", "KeepFormat", new { id = keepformat_dtl.FormatId });
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