using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EDIS.Data;
using EDIS.Models.KeepModels;
using EDIS.Models.Identity;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Controllers
{
    [Authorize]
    public class KeepFormatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KeepFormatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: KeepFormat
        public IActionResult Index()
        {
            return View(_context.KeepFormats.ToList());
        }

        // GET: KeepFormat/Details/5
        public IActionResult Details(string id = null)
        {
            KeepFormatModel keepformat = _context.KeepFormats.Find(System.Net.WebUtility.HtmlDecode(id));
            if (keepformat == null)
            {
                return StatusCode(404);
            }
            return View(keepformat);
        }

        // GET: KeepFormat/Create
        public IActionResult Create()
        {
            KeepFormatModel kf = new KeepFormatModel()
            {
                Plants = "廠牌型號：; 名稱：;"
            };
            return View(kf);
        }

        // POST: KeepFormat/Create
        [HttpPost]
        public IActionResult Create(KeepFormatModel keepformat)
        {
            AppUserModel ur = _context.AppUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            keepformat.FormatId = keepformat.FormatId.Trim();
            KeepFormatModel k = _context.KeepFormats.Find(keepformat.FormatId);
            if (k != null)
            {
                ModelState.AddModelError("", "保養格式代號重複!!");
                return View(keepformat);
            }
            if (ModelState.IsValid)
            {
                keepformat.Rtp = ur.Id;
                keepformat.Rtt = DateTime.Now;
                _context.KeepFormats.Add(keepformat);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(keepformat);
        }

        // GET: KeepFormat/Edit/5
        public IActionResult Edit(string id = null)
        {
            KeepFormatModel keepformat = _context.KeepFormats.Find(System.Net.WebUtility.HtmlDecode(id));
            if (keepformat == null)
            {
                return StatusCode(404);
            }
            return View(keepformat);
        }

        // POST: KeepFormat/Edit/5
        [HttpPost]
        public IActionResult Edit(KeepFormatModel keepformat)
        {
            AppUserModel ur = _context.AppUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (ModelState.IsValid)
            {
                keepformat.Rtp = ur.Id;
                keepformat.Rtt = DateTime.Now;
                _context.Entry(keepformat).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(keepformat);
        }

        // GET: KeepFormat/GetPlants
        public IActionResult GetPlants(string id = null)
        {
            if (id != null)
                return Content(_context.KeepFormats.Find(id).Plants);
            return Content("");
        }

        // GET: KeepFormat/Delete/5
        public IActionResult Delete(string id = null)
        {
            KeepFormatModel keepformat = _context.KeepFormats.Find(id);
            if (keepformat == null)
            {
                return StatusCode(404);
            }
            return View(keepformat);
        }

        // POST: KeepFormat/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(string id)
        {
            List<KeepFormatDtlModel> dtls = _context.KeepFormatDtls.Where(d => d.FormatId == id).ToList();
            _context.KeepFormatDtls.RemoveRange(dtls);
            //
            KeepFormatModel keepformat = _context.KeepFormats.Find(id);
            _context.KeepFormats.Remove(keepformat);
            //
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
            base.Dispose(disposing);
        }
    }
}