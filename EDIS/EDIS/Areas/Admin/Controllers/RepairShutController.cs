using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Data;
using EDIS.Models.RepairModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class RepairShutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RepairShutController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/RepairShut
        public IActionResult Index()
        {
            return View();
        }

        // POST: Admin/RepairShut
        [HttpPost]
        public IActionResult Index(IFormCollection fm)
        {
            string ticketno = fm["qtyTICKET"];
            string vendorname = fm["qtyVENDORNAME"];
            string vendorno = fm["qtyVENDORNO"];
            string docid = fm["qtyDOCID"];
            docid = docid.Trim();
            ticketno = ticketno.ToUpper();

            //string qtyDate1 = fm["qtyApplyDateFrom"];
            //string qtyDate2 = fm["qtyApplyDateTo"];

            //DateTime applyDateFrom = DateTime.Now;
            //DateTime applyDateTo = DateTime.Now;
            ///* Dealing search by date. */
            //if (qtyDate1 != "" && qtyDate2 != "")// If 2 date inputs have been insert, compare 2 dates.
            //{
            //    DateTime date1 = DateTime.Parse(qtyDate1);
            //    DateTime date2 = DateTime.Parse(qtyDate2);
            //    int result = DateTime.Compare(date1, date2);
            //    if (result < 0)
            //    {
            //        applyDateFrom = date1.Date;
            //        applyDateTo = date2.Date;
            //    }
            //    else if (result == 0)
            //    {
            //        applyDateFrom = date1.Date;
            //        applyDateTo = date1.Date;
            //    }
            //    else
            //    {
            //        applyDateFrom = date2.Date;
            //        applyDateTo = date1.Date;
            //    }
            //}
            //else if (qtyDate1 == "" && qtyDate2 != "")
            //{
            //    applyDateFrom = DateTime.Parse(qtyDate2);
            //    applyDateTo = DateTime.Parse(qtyDate2);
            //}
            //else if (qtyDate1 != "" && qtyDate2 == "")
            //{
            //    applyDateFrom = DateTime.Parse(qtyDate1);
            //    applyDateTo = DateTime.Parse(qtyDate1);
            //}

            List<TicketModel> ts = _context.Tickets.ToList();
            List<RepairCostModel> repairCost = _context.RepairCosts.Include(r => r.TicketDtl).ToList();
            List<RepairModel> repair = _context.Repairs.ToList();
            List<RepairListVModel> rv = new List<RepairListVModel>();

            if (!string.IsNullOrEmpty(ticketno))
            {
                repairCost = repairCost.Where(rc => rc.StockType != "3").ToList();  //篩選掉簽單的資料
                repairCost = repairCost.Where(rc => rc.TicketDtl.TicketDtlNo.ToUpper() == ticketno).ToList();
            }
            if (!string.IsNullOrEmpty(vendorname))
            {
                repairCost = repairCost.Where(rc => rc.VendorName != null && rc.VendorName != "")
                                       .Where(rc => rc.VendorName.Contains(vendorname)).ToList();
            }
            if (!string.IsNullOrEmpty(vendorno))
            {
                repairCost = repairCost.Where(rc => rc.VendorId != null)
                                       .Where(rc => rc.VendorId == Convert.ToInt32(vendorno)).ToList();
            }
            if (!string.IsNullOrEmpty(docid))
            {
                repair = repair.Where(r => r.DocId == docid).ToList();
            }

            var docIdList = repairCost.Select(rc => rc.DocId).Distinct();
            repair.Join(docIdList, r => r.DocId, li => li, (r, li) => r)
                  .Join(_context.RepairDtls, r => r.DocId, d => d.DocId,
                       (r, d) => new
                       {
                           repair = r,
                           repdtl = d
                       })
                       .Join(_context.Departments, j => j.repair.AccDpt, d => d.DptId,
                       (j, d) => new
                       {
                           repair = j.repair,
                           repdtl = j.repdtl,
                           dpt = d
                       })
                  .Where(r => r.repdtl.CloseDate != null)   //篩選已結案的請修單
                  .Where(r => r.repdtl.ShutDate == null)    //篩選尚未關帳的請修單
                  .ToList()
                  .ForEach(j => rv.Add(new RepairListVModel
                  {
                      DocType = "請修",
                      RepType = j.repair.RepType,
                      DocId = j.repair.DocId,
                      ApplyDate = j.repair.ApplyDate,
                      AssetNo = j.repair.AssetNo,
                      AssetName = j.repair.AssetName,
                      PlaceLoc = j.repair.LocType,
                      ApplyDpt = j.repair.DptId,
                      AccDpt = j.repair.AccDpt,
                      AccDptName = j.dpt.Name_C,
                      TroubleDes = j.repair.TroubleDes,
                      DealState = _context.DealStatuses.Find(j.repdtl.DealState).Title,
                      DealDes = j.repdtl.DealDes,
                      Cost = j.repdtl.Cost,
                      Days = DateTime.Now.Subtract(j.repair.ApplyDate).Days,
                      EndDate = j.repdtl.EndDate,
                      CloseDate = j.repdtl.CloseDate,
                      IsCharged = j.repdtl.IsCharged,
                      repdata = j.repair
                  }));


            /* Search date by Date. */
            //if (string.IsNullOrEmpty(qtyDate1) == false || string.IsNullOrEmpty(qtyDate2) == false)
            //{
            //    ts = ts.Where(t => t.ShutDate >= applyDateFrom && t.ShutDate <= applyDateTo).ToList();
            //}

            return PartialView("List", rv);
        }

        // GET: Admin/RepairShut/List
        public IActionResult List()
        {
            return PartialView(_context.Tickets.ToList());
        }

        // POST: Admin/RepairShut/ShutRep/5
        [HttpPost]
        public IActionResult ShutRep(string repairs)
        {
            // Get Login User's details.
            var ur = _context.AppUsers.Where(u => u.UserName == this.User.Identity.Name).FirstOrDefault();

            string[] s = repairs.Split(new char[] { ';' });
            RepairModel repair;
            RepairDtlModel repairDtl;
            RepairShutRecordsModel shutRecord = new RepairShutRecordsModel();
            foreach (string ss in s)
            {
                repair = _context.Repairs.Find(ss);
                if (repair != null)
                {
                    // Update shut time on RepairDtl.
                    repairDtl = _context.RepairDtls.Find(ss);
                    repairDtl.ShutDate = DateTime.Now;
                    _context.Entry(repairDtl).State = EntityState.Modified;
                    // Record the shut user on RepairShutRecords.
                    shutRecord.DocId = repairDtl.DocId;
                    shutRecord.Rtp = ur.Id;
                    shutRecord.Rtt = DateTime.Now;
                    _context.Add(shutRecord);
                    _context.SaveChanges();
                }
            }
            return new JsonResult(repairs)
            {
                Value = new { success = true, error = "" }
            };
        }
    }
}