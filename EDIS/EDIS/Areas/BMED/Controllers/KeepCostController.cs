using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Areas.BMED.Data;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.KeepModels;
using EDIS.Areas.BMED.Repositories;
using EDIS.Repositories;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS.Areas.BMED.Models.RepairModels;

namespace EDIS.Areas.BMED.Controllers
{
    [Area("BMED")]
    [Authorize]
    public class KeepCostController : Controller
    {
        private readonly BMEDDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly IRepository<DepartmentModel, string> _dptRepo;
        private readonly CustomUserManager userManager;

        public KeepCostController(BMEDDbContext context,
                                  IRepository<AppUserModel, int> userRepo,
                                  IRepository<DepartmentModel, string> dptRepo,
                                  CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            _dptRepo = dptRepo;
            userManager = customUserManager;
        }

        [HttpPost]
        public IActionResult Edit(KeepCostModel keepCost)
        {
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();

            /* Change to UpperCase.*/
            if (keepCost.TicketDtl.TicketDtlNo != null)
            {
                keepCost.TicketDtl.TicketDtlNo = keepCost.TicketDtl.TicketDtlNo.ToUpper();
            }
            if (keepCost.SignNo != null)
            {
                keepCost.SignNo = keepCost.SignNo.ToUpper();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (keepCost.StockType == "2")
                    {
                        var dupData = _context.BMEDKeepCosts.Include(c => c.TicketDtl)
                                                            .Where(c => c.DocId == keepCost.DocId &&
                                                                        c.PartName == keepCost.PartName &&
                                                                        c.Standard == keepCost.Standard &&
                                                                        c.TicketDtl.TicketDtlNo == keepCost.TicketDtl.TicketDtlNo).FirstOrDefault();
                        if (dupData != null)
                        {
                            string msg = "資料重複儲存!!";
                            return BadRequest(msg);
                        }
                    }
                    else
                    {
                        var dupData = _context.BMEDKeepCosts.Include(c => c.TicketDtl)
                                                            .Where(c => c.DocId == keepCost.DocId &&
                                                                        c.PartName == keepCost.PartName &&
                                                                        c.Standard == keepCost.Standard &&
                                                                        c.SignNo == keepCost.SignNo).FirstOrDefault();
                        if (dupData != null)
                        {
                            string msg = "資料重複儲存!!";
                            return BadRequest(msg);
                        }
                    }

                    int seqno = _context.BMEDKeepCosts.Where(c => c.DocId == keepCost.DocId)
                                                      .Select(c => c.SeqNo).DefaultIfEmpty().Max();
                    keepCost.SeqNo = seqno + 1;
                    if (keepCost.StockType == "2")
                    {
                        if (string.IsNullOrEmpty(keepCost.TicketDtl.TicketDtlNo))
                        {
                            //throw new Exception("發票號碼不可空白!!");
                            string msg = "發票號碼不可空白!!";
                            return BadRequest(msg);
                        }
                        if (keepCost.AccountDate == null)
                        {
                            //throw new Exception("發票日期不可空白!!");
                            string msg = "發票日期不可空白!!";
                            return BadRequest(msg);
                        }
                        int i = _context.BMEDTicketDtls.Where(d => d.TicketDtlNo == keepCost.TicketDtl.TicketDtlNo)
                                                       .Select(d => d.SeqNo).DefaultIfEmpty().Max();
                        keepCost.TicketDtl.SeqNo = i + 1;
                        keepCost.TicketDtl.ObjName = keepCost.PartName;
                        keepCost.TicketDtl.Qty = keepCost.Qty;
                        keepCost.TicketDtl.Unite = keepCost.Unite;
                        keepCost.TicketDtl.Price = keepCost.Price;
                        keepCost.TicketDtl.Cost = keepCost.TotalCost;
                        TicketModel t = _context.BMEDTickets.Find(keepCost.TicketDtl.TicketDtlNo);
                        if (t == null)
                        {
                            t = new TicketModel();
                            t.TicketNo = keepCost.TicketDtl.TicketDtlNo;
                            t.TicDate = keepCost.AccountDate;
                            t.ApplyDate = null;
                            t.CancelDate = null;
                            t.VendorId = keepCost.VendorId;
                            t.VendorName = keepCost.VendorName;
                            keepCost.TicketDtl.Ticket = t;
                            _context.BMEDTickets.Add(t);
                        }
                        _context.BMEDTicketDtls.Add(keepCost.TicketDtl);
                    }
                    else
                    {
                        keepCost.AccountDate = keepCost.AccountDate == null ? DateTime.Now.Date : keepCost.AccountDate;
                        keepCost.TicketDtl = null;
                    }
                    keepCost.Rtp = ur.Id;
                    keepCost.Rtt = DateTime.Now;
                    if (keepCost.StockType != "0")
                        keepCost.PartNo = "";

                    _context.BMEDKeepCosts.Add(keepCost);
                    _context.SaveChanges();
                    //
                    KeepDtlModel dtl = _context.BMEDKeepDtls.Where(d => d.DocId == keepCost.DocId)
                                                            .FirstOrDefault();
                    if (dtl != null)
                    {
                        dtl.Cost = _context.BMEDKeepCosts.Where(k => k.DocId == keepCost.DocId)
                                                         .Select(k => k.TotalCost)
                                                         .DefaultIfEmpty(0).Sum();
                        _context.Entry(dtl).State = EntityState.Modified;
                        _context.SaveChanges();
                    }

                    return ViewComponent("BMEDKeepCostList", new { id = keepCost.DocId, viewType = "Edit" });
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

        public IActionResult PrintStockDetails(string docId, int seqNo)
        {
            var stockDetails = _context.BMEDKeepCosts.Find(docId, seqNo);
            return View(stockDetails);
        }

        public IActionResult Delete(string docid, string seqno)
        {
            try
            {
                KeepCostModel keepCost = _context.BMEDKeepCosts.Find(docid, Convert.ToInt32(seqno));
                _context.BMEDKeepCosts.Remove(keepCost);
                _context.SaveChanges();
                //
                KeepDtlModel dtl = _context.BMEDKeepDtls.Where(d => d.DocId == keepCost.DocId)
                                                        .FirstOrDefault();
                if (dtl != null)
                {
                    dtl.Cost = _context.BMEDKeepCosts.Where(k => k.DocId == keepCost.DocId)
                                                     .Select(k => k.TotalCost)
                                                     .DefaultIfEmpty(0).Sum();
                    _context.Entry(dtl).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                return new JsonResult(keepCost)
                {
                    Value = new { success = true, error = "" }
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }
    }
}