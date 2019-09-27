using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Areas.BMED.Data;
using EDIS.Areas.BMED.Models.RepairModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Areas.BMED.Controllers
{
    [Area("BMED")]
    [Authorize]
    public class TicketController : Controller
    {
        private readonly BMEDDbContext _context;

        public TicketController(BMEDDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string GetTicketSeq()
        {
            string result = "";
            int seq = 0;
            string yyymm = Convert.ToString((DateTime.Now.Year - 1911) * 100 + DateTime.Now.Month);
            int cnt = _context.BMEDTicket_seq_tmps.Where(t => t.YYYMM == yyymm).Count();
            Ticket_seq_tmpModel tmp;
            if (cnt > 0)
            {
                tmp = _context.BMEDTicket_seq_tmps.Find(yyymm);
                seq = Convert.ToInt32(tmp.TICKET_SEQ) + 1;
                result = Convert.ToString(seq);
                tmp.TICKET_SEQ = Convert.ToString(seq);
                _context.Entry(tmp).State = EntityState.Modified;
            }
            else
            {
                tmp = new Ticket_seq_tmpModel();
                seq = Convert.ToInt32(yyymm) * 1000 + 1;
                tmp.YYYMM = Convert.ToString(yyymm);
                tmp.TICKET_SEQ = Convert.ToString(seq);
                result = Convert.ToString(seq);
                _context.BMEDTicket_seq_tmps.Add(tmp);
            }
            _context.SaveChanges();

            return result;
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