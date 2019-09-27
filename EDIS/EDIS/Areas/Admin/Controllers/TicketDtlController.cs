using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Data;
using EDIS.Models.RepairModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class TicketDtlController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketDtlController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: Admin/TicketDtl/Details/5
        public IActionResult Details(string id, int seq)
        {
            if (id == null)
            {
                return BadRequest();
            }
            TicketDtlModel ticketDtl = _context.TicketDtls.Find(id, seq);
            if (ticketDtl == null)
            {
                return StatusCode(404);
            }
            var rc = _context.RepairCosts.Where(r => r.TicketDtl.TicketDtlNo == ticketDtl.TicketDtlNo &&
                                                     r.TicketDtl.SeqNo == ticketDtl.SeqNo).FirstOrDefault();
            if (rc != null)
            {
                ticketDtl.DocId = rc.DocId;
            }
            return View(ticketDtl);
        }

        // GET: Admin/TicketDtl/Create
        public IActionResult Create(string id)
        {
            var lastDtl = _context.TicketDtls.Where(t => t.TicketDtlNo == id).OrderBy(t => t.SeqNo).LastOrDefault();
            TicketDtlModel ticketDtl = new TicketDtlModel();
            if (lastDtl != null)
            {
                ticketDtl.TicketDtlNo = lastDtl.TicketDtlNo;
                ticketDtl.SeqNo = lastDtl.SeqNo + 1;
            }
            else
            {
                ticketDtl.TicketDtlNo = id;
                ticketDtl.SeqNo = 1;
            }

            return View(ticketDtl);
        }

        // GET: Admin/TicketDtl/Create2
        public IActionResult Create2(string id)
        {
            var lastDtl = _context.TicketDtls.Where(t => t.TicketDtlNo == id).OrderBy(t => t.SeqNo).LastOrDefault();
            TicketDtlModel ticketDtl = new TicketDtlModel();
            if (lastDtl != null)
            {
                ticketDtl.TicketDtlNo = lastDtl.TicketDtlNo;
                ticketDtl.SeqNo = lastDtl.SeqNo + 1;
            }
            else
            {
                ticketDtl.TicketDtlNo = id;
                ticketDtl.SeqNo = 1;
            }

            return View(ticketDtl);
        }

        // POST: Admin/TicketDtl/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("TicketDtlNo,SeqNo,ObjName,Qty,Unite,Price,Cost")] TicketDtlModel ticketDtl)
        {
            if (ModelState.IsValid)
            {
                _context.TicketDtls.Add(ticketDtl);
                _context.SaveChanges();
                return RedirectToAction("Edit", "Ticket", new { id = ticketDtl.TicketDtlNo });
            }

            return View(ticketDtl);
        }

        // POST: Admin/TicketDtl/Create2
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create2([Bind("TicketDtlNo,SeqNo,ObjName,Qty,Unite,Price,Cost")] TicketDtlModel ticketDtl)
        {
            if (ModelState.IsValid)
            {
                _context.TicketDtls.Add(ticketDtl);
                _context.SaveChanges();
                return Ok(ticketDtl);
            }
            return BadRequest();
        }

        // GET: Admin/TicketDtl/Edit/5
        public IActionResult Edit(string id, int seq)
        {
            if (id == null)
            {
                return BadRequest();
            }
            TicketDtlModel ticketDtl = _context.TicketDtls.Find(id, seq);
            if (ticketDtl == null)
            {
                return StatusCode(404);
            }
            return View(ticketDtl);
        }

        // GET: Admin/TicketDtl/Edit2/5
        public IActionResult Edit2(string id, int seq)
        {
            if (id == null)
            {
                return BadRequest();
            }
            TicketDtlModel ticketDtl = _context.TicketDtls.Find(id, seq);
            if (ticketDtl == null)
            {
                return StatusCode(404);
            }
            return View(ticketDtl);
        }

        // POST: Admin/TicketDtl/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind("TicketDtlNo,SeqNo,ObjName,Qty,Unite,Price,Cost")] TicketDtlModel ticketDtl)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(ticketDtl).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Edit", "Ticket", new { id = ticketDtl.TicketDtlNo });
            }
            return View(ticketDtl);
        }

        // POST: Admin/TicketDtl/Edit2/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit2([Bind("TicketDtlNo,SeqNo,ObjName,Qty,Unite,Price,Cost")] TicketDtlModel ticketDtl)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(ticketDtl).State = EntityState.Modified;
                _context.SaveChanges();
                return Ok(ticketDtl);
            }
            return BadRequest();
        }

        // GET: Admin/TicketDtl/Delete/5
        public IActionResult Delete(string id, int seq)
        {
            if (id == null)
            {
                return BadRequest();
            }
            TicketDtlModel ticketDtl = _context.TicketDtls.Find(id, seq);
            if (ticketDtl == null)
            {
                return StatusCode(404);
            }
            return View(ticketDtl);
        }

        // GET: Admin/TicketDtl/Delete2/5
        public IActionResult Delete2(string id, int seq)
        {
            if (id == null)
            {
                return BadRequest();
            }
            TicketDtlModel ticketDtl = _context.TicketDtls.Find(id, seq);
            if (ticketDtl == null)
            {
                return StatusCode(404);
            }
            return View(ticketDtl);
        }

        // POST: Admin/TicketDtl/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id, int seq)
        {
            TicketDtlModel ticketDtl = _context.TicketDtls.Find(id, seq);
            _context.TicketDtls.Remove(ticketDtl);
            _context.SaveChanges();
            return RedirectToAction("Edit", "Ticket", new { id = ticketDtl.TicketDtlNo });
        }

        // POST: Admin/TicketDtl/Delete2/5
        [HttpPost, ActionName("Delete2")]
        public IActionResult DeleteConfirmed2(string id, int seq)
        {
            TicketDtlModel ticketDtl = _context.TicketDtls.Find(id, seq);
            _context.TicketDtls.Remove(ticketDtl);
            _context.SaveChanges();
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}