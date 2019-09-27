using EDIS.Areas.BMED.Data;
using EDIS.Areas.BMED.Models.RepairModels;
using EDIS.Areas.BMED.Models.KeepModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Models.Identity;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Areas.BMED.Controllers
{
    [Area("BMED")]
    [Authorize]
    public class SearchController : Controller
    {
        private readonly BMEDDbContext _context;
        private readonly CustomRoleManager roleManager;

        public SearchController(BMEDDbContext context,
                                CustomRoleManager customRoleManager)
        {
            _context = context;
            roleManager = customRoleManager;
        }

        // GET: BMED/Search/RepIndex
        public IActionResult RepIndex()
        {
            List<SelectListItem> FlowlistItem = new List<SelectListItem>();
            FlowlistItem.Add(new SelectListItem { Text = "未結案", Value = "未結案" });
            FlowlistItem.Add(new SelectListItem { Text = "已結案", Value = "已結案" });
            ViewData["FLOWTYPE"] = new SelectList(FlowlistItem, "Value", "Text");

            /* 成本中心 & 申請部門的下拉選單資料 */
            var dptList = new[] { "K", "P", "C" };  //本院部門
            var departments = _context.Departments.Where(d => dptList.Contains(d.Loc)).ToList();
            List<SelectListItem> listItem = new List<SelectListItem>();
            foreach (var item in departments)
            {
                listItem.Add(new SelectListItem
                {
                    Text = item.Name_C + "(" + item.DptId + ")",    //show DptName(DptId)
                    Value = item.DptId
                });
            }

            ViewData["ACCDPT"] = new SelectList(listItem, "Value", "Text");
            ViewData["APPLYDPT"] = new SelectList(listItem, "Value", "Text");

            /* 處理狀態的下拉選單 */
            var dealStatuses = _context.BMEDDealStatuses.ToList();
            List<SelectListItem> listItem2 = new List<SelectListItem>();
            foreach (var item in dealStatuses)
            {
                listItem2.Add(new SelectListItem
                {
                    Text = item.Title,
                    Value = item.Id.ToString()
                });
            }
            ViewData["DealStatus"] = new SelectList(listItem2, "Value", "Text");

            /* 處理有無費用的下拉選單 */
            List<SelectListItem> listItem3 = new List<SelectListItem>();
            listItem3.Add(new SelectListItem { Text = "有", Value = "Y" });
            listItem3.Add(new SelectListItem { Text = "無", Value = "N" });
            ViewData["IsCharged"] = new SelectList(listItem3, "Value", "Text");

            /* 處理日期查詢的下拉選單 */
            List<SelectListItem> listItem4 = new List<SelectListItem>();
            listItem4.Add(new SelectListItem { Text = "申請日", Value = "申請日" });
            listItem4.Add(new SelectListItem { Text = "完工日", Value = "完工日" });
            listItem4.Add(new SelectListItem { Text = "結案日", Value = "結案日" });
            ViewData["DateType"] = new SelectList(listItem4, "Value", "Text", "申請日");

            /* 處理工程師查詢的下拉選單 */
            var engs = roleManager.GetUsersInRole("MedEngineer").ToList();
            List<SelectListItem> listItem5 = new List<SelectListItem>();
            foreach (string l in engs)
            {
                var u = _context.AppUsers.Where(ur => ur.UserName == l).FirstOrDefault();
                if (u != null)
                {
                    listItem5.Add(new SelectListItem
                    {
                        Text = u.FullName + "(" + u.UserName + ")",
                        Value = u.Id.ToString()
                    });
                }
            }
            ViewData["BMEDEngs"] = new SelectList(listItem5, "Value", "Text");

            QryRepListData data = new QryRepListData();

            return View(data);
        }

        // GET: BMED/Search/KeepIndex
        public IActionResult KeepIndex()
        {
            List<SelectListItem> FlowlistItem = new List<SelectListItem>();
            FlowlistItem.Add(new SelectListItem { Text = "未結案", Value = "未結案" });
            FlowlistItem.Add(new SelectListItem { Text = "已結案", Value = "已結案" });
            ViewData["FLOWTYPE"] = new SelectList(FlowlistItem, "Value", "Text");

            /* 成本中心 & 申請部門的下拉選單資料 */
            var dptList = new[] { "K", "P", "C" };  //本院部門
            var departments = _context.Departments.Where(d => dptList.Contains(d.Loc)).ToList();
            List<SelectListItem> listItem = new List<SelectListItem>();
            foreach (var item in departments)
            {
                listItem.Add(new SelectListItem
                {
                    Text = item.Name_C + "(" + item.DptId + ")",    //show DptName(DptId)
                    Value = item.DptId
                });
            }

            ViewData["ACCDPT"] = new SelectList(listItem, "Value", "Text");
            ViewData["APPLYDPT"] = new SelectList(listItem, "Value", "Text");

            /* 處理保養狀態的下拉選單 */
            var keepResults = _context.BMEDKeepResults.ToList();
            List<SelectListItem> listItem2 = new List<SelectListItem>();
            foreach (var item in keepResults)
            {
                listItem2.Add(new SelectListItem
                {
                    Text = item.Title,
                    Value = item.Id.ToString()
                });
            }
            ViewData["KeepResults"] = new SelectList(listItem2, "Value", "Text");

            /* 處理有無費用的下拉選單 */
            List<SelectListItem> listItem3 = new List<SelectListItem>();
            listItem3.Add(new SelectListItem { Text = "有", Value = "Y" });
            listItem3.Add(new SelectListItem { Text = "無", Value = "N" });
            ViewData["IsCharged"] = new SelectList(listItem3, "Value", "Text");

            /* 處理日期查詢的下拉選單 */
            List<SelectListItem> listItem4 = new List<SelectListItem>();
            listItem4.Add(new SelectListItem { Text = "送單日", Value = "送單日" });
            listItem4.Add(new SelectListItem { Text = "完工日", Value = "完工日" });
            listItem4.Add(new SelectListItem { Text = "結案日", Value = "結案日" });
            ViewData["DateType"] = new SelectList(listItem4, "Value", "Text", "申請日");

            /* 處理工程師查詢的下拉選單 */
            var engs = roleManager.GetUsersInRole("MedEngineer").ToList();
            List<SelectListItem> listItem5 = new List<SelectListItem>();
            foreach (string l in engs)
            {
                var u = _context.AppUsers.Where(ur => ur.UserName == l).FirstOrDefault();
                if (u != null)
                {
                    listItem5.Add(new SelectListItem
                    {
                        Text = u.FullName + "(" + u.UserName + ")",
                        Value = u.Id.ToString()
                    });
                }
            }
            ViewData["BMEDEngs"] = new SelectList(listItem5, "Value", "Text");

            QryKeepListData data = new QryKeepListData();

            return View(data);
        }

        // POST: BMED/Search/GetRepQryList
        [HttpPost]
        public IActionResult GetRepQryList(QryRepListData qdata)
        {
            string docid = qdata.BMEDqtyDOCID;
            string ano = qdata.BMEDqtyASSETNO;
            string acc = qdata.BMEDqtyACCDPT;
            string aname = qdata.BMEDqtyASSETNAME;
            string ftype = qdata.BMEDqtyFLOWTYPE;
            string dptid = qdata.BMEDqtyDPTID;
            string qtyDate1 = qdata.BMEDqtyApplyDateFrom;
            string qtyDate2 = qdata.BMEDqtyApplyDateTo;
            string qtyIsCharged = qdata.BMEDqtyIsCharged;
            string qtyDealStatus = qdata.BMEDqtyDealStatus;
            string qtyDateType = qdata.BMEDqtyDateType;
            string qtyEngCode = qdata.BMEDqtyEngCode;
            string qtyTicketNo = qdata.BMEDqtyTicketNo;
            string qtyVendor = qdata.BMEDqtyVendor;

            DateTime applyDateFrom = DateTime.Now;
            DateTime applyDateTo = DateTime.Now;
            /* Dealing search by date. */
            if (qtyDate1 != null && qtyDate2 != null)// If 2 date inputs have been insert, compare 2 dates.
            {
                DateTime date1 = DateTime.Parse(qtyDate1);
                DateTime date2 = DateTime.Parse(qtyDate2);
                int result = DateTime.Compare(date1, date2);
                if (result < 0)
                {
                    applyDateFrom = date1.Date;
                    applyDateTo = date2.Date;
                }
                else if (result == 0)
                {
                    applyDateFrom = date1.Date;
                    applyDateTo = date1.Date;
                }
                else
                {
                    applyDateFrom = date2.Date;
                    applyDateTo = date1.Date;
                }
            }
            else if (qtyDate1 == null && qtyDate2 != null)
            {
                applyDateFrom = DateTime.Parse(qtyDate2);
                applyDateTo = DateTime.Parse(qtyDate2);
            }
            else if (qtyDate1 != null && qtyDate2 == null)
            {
                applyDateFrom = DateTime.Parse(qtyDate1);
                applyDateTo = DateTime.Parse(qtyDate1);
            }


            List<RepairSearchListVModel> rv = new List<RepairSearchListVModel>();

            /* Querying data. */
            var rps = _context.BMEDRepairs.ToList();
            var repairFlows = _context.BMEDRepairFlows.ToList();
            var repairDtls = _context.BMEDRepairDtls.ToList();
            if (!string.IsNullOrEmpty(docid))   //表單編號
            {
                docid = docid.Trim();
                rps = rps.Where(v => v.DocId == docid).ToList();
            }
            if (!string.IsNullOrEmpty(ano))     //財產編號
            {
                rps = rps.Where(v => v.AssetNo == ano).ToList();
            }
            if (!string.IsNullOrEmpty(dptid))   //所屬部門編號
            {
                rps = rps.Where(v => v.DptId == dptid).ToList();
            }
            if (!string.IsNullOrEmpty(acc))     //成本中心
            {
                rps = rps.Where(v => v.AccDpt == acc).ToList();
            }
            if (!string.IsNullOrEmpty(aname))   //財產名稱
            {
                rps = rps.Where(v => v.AssetName != null)
                         .Where(v => v.AssetName.Contains(aname)).ToList();
            }
            if (!string.IsNullOrEmpty(qtyTicketNo))   //發票號碼
            {
                qtyTicketNo = qtyTicketNo.ToUpper();
                var resultDocIds = _context.BMEDRepairCosts.Include(rc => rc.TicketDtl)
                                                           .Where(rc => rc.TicketDtl.TicketDtlNo == qtyTicketNo)
                                                           .Select(rc => rc.DocId).Distinct();
                rps = (from r in rps
                       where resultDocIds.Any(val => r.DocId.Contains(val))
                       select r).ToList();
            }
            if (!string.IsNullOrEmpty(qtyVendor))   //廠商關鍵字
            {
                var resultDocIds = _context.BMEDRepairCosts.Include(rc => rc.TicketDtl)
                                                           .Where(rc => rc.VendorName.Contains(qtyVendor))
                                                           .Select(rc => rc.DocId).Distinct();
                rps = (from r in rps
                       where resultDocIds.Any(val => r.DocId.Contains(val))
                       select r).ToList();
            }
            if (!string.IsNullOrEmpty(qtyEngCode))     //負責工程師
            {
                rps = rps.Where(v => v.EngId == Convert.ToInt32(qtyEngCode)).ToList();
            }
            if (string.IsNullOrEmpty(qtyDate1) == false || string.IsNullOrEmpty(qtyDate2) == false)  //申請日
            {
                if (qtyDateType == "申請日")
                {
                    rps = rps.Where(v => v.ApplyDate >= applyDateFrom && v.ApplyDate <= applyDateTo).ToList();
                }
            }
            if (!string.IsNullOrEmpty(ftype))   //流程狀態
            {
                switch (ftype)
                {
                    case "未結案":
                        repairFlows = repairFlows.GroupBy(f => f.DocId).Where(group => group.Last().Status == "?")
                                                                       .Select(group => group.Last()).ToList();
                        break;
                    case "已結案":
                        repairFlows = repairFlows.GroupBy(f => f.DocId).Where(group => group.Last().Status == "2")
                                                                       .Select(group => group.Last()).ToList();
                        break;
                }
            }
            else
            {
                repairFlows = repairFlows.GroupBy(f => f.DocId).Where(group => group.Last().Status != "3")
                                                               .Select(group => group.Last()).ToList(); ;
            }
            if (!string.IsNullOrEmpty(qtyDealStatus))   //處理狀態
            {
                repairDtls = repairDtls.Where(r => r.DealState == Convert.ToInt32(qtyDealStatus)).ToList();
            }
            if (!string.IsNullOrEmpty(qtyIsCharged))    //有無費用
            {
                repairDtls = repairDtls.Where(r => r.IsCharged == qtyIsCharged).ToList();
            }

            /* If no search result. */
            if (rps.Count() == 0)
            {
                return View("RepQryList", rv);
            }

            rps.Join(repairFlows, r => r.DocId, f => f.DocId,
                (r, f) => new
                {
                    repair = r,
                    flow = f
                })
                .Join(repairDtls, m => m.repair.DocId, d => d.DocId,
                (m, d) => new
                {
                    repair = m.repair,
                    flow = m.flow,
                    repdtl = d
                })
                .Join(_context.Departments, j => j.repair.AccDpt, d => d.DptId,
                (j, d) => new
                {
                    repair = j.repair,
                    flow = j.flow,
                    repdtl = j.repdtl,
                    dpt = d
                })
                .ToList()
                .ForEach(j => rv.Add(new RepairSearchListVModel
                {
                    DocType = "醫工請修",
                    RepType = j.repair.RepType,
                    DocId = j.repair.DocId,
                    ApplyDate = j.repair.ApplyDate,
                    PlaceLoc = j.repair.PlaceLoc,
                    ApplyDpt = j.repair.DptId,
                    AccDpt = j.repair.AccDpt,
                    AccDptName = j.dpt.Name_C,
                    TroubleDes = j.repair.TroubleDes,
                    DealState = _context.BMEDDealStatuses.Find(j.repdtl.DealState).Title,
                    DealDes = j.repdtl.DealDes,
                    Cost = j.repdtl.Cost,
                    Days = DateTime.Now.Subtract(j.repair.ApplyDate).Days,
                    Flg = j.flow.Status,
                    FlowUid = j.flow.UserId,
                    FlowCls = j.flow.Cls,
                    FlowUidName = _context.AppUsers.Find(j.flow.UserId).FullName,
                    EndDate = j.repdtl.EndDate,
                    CloseDate = j.repdtl.CloseDate,
                    IsCharged = j.repdtl.IsCharged,
                    repdata = j.repair
                }));

            /* 設備編號"有"、"無"的對應，"有"讀取table相關data，"無"只顯示申請人輸入的設備名稱 */
            foreach (var item in rv)
            {
                var repairDoc = _context.BMEDRepairs.Find(item.DocId);
                if (repairDoc.AssetNo != null)
                {
                    var asset = _context.BMEDAssets.Where(a => a.AssetNo == repairDoc.AssetNo).FirstOrDefault();
                    if (asset != null)
                    {
                        item.AssetNo = asset.AssetNo;
                        item.AssetName = asset.Cname;
                        item.Brand = asset.Brand;
                        item.Type = asset.Type;
                    }
                }
                else
                {
                    item.AssetName = repairDoc.AssetName;
                }
            }

            /* Search date by DateType. */
            if (string.IsNullOrEmpty(qtyDate1) == false || string.IsNullOrEmpty(qtyDate2) == false)
            {
                if (qtyDateType == "結案日")
                {
                    rv = rv.Where(v => v.CloseDate >= applyDateFrom && v.CloseDate <= applyDateTo).ToList();
                }
                else if (qtyDateType == "完工日")
                {
                    rv = rv.Where(v => v.EndDate >= applyDateFrom && v.EndDate <= applyDateTo).ToList();
                }
            }

            /* Sorting search result. */
            if (rv.Count() != 0)
            {
                if (qtyDateType == "結案日")
                {
                    rv = rv.OrderByDescending(r => r.CloseDate).ThenByDescending(r => r.DocId).ToList();
                }
                else if (qtyDateType == "完工日")
                {
                    rv = rv.OrderByDescending(r => r.EndDate).ThenByDescending(r => r.DocId).ToList();
                }
                else
                {
                    rv = rv.OrderByDescending(r => r.ApplyDate).ThenByDescending(r => r.DocId).ToList();
                }
            }

            return View("RepQryList", rv);
        }

        // POST: BMED/Search/GetKeepQryList
        [HttpPost]
        public IActionResult GetKeepQryList(QryKeepListData qdata)
        {
            string docid = qdata.BMEDKqtyDOCID;
            string ano = qdata.BMEDKqtyASSETNO;
            string acc = qdata.BMEDKqtyACCDPT;
            string aname = qdata.BMEDKqtyASSETNAME;
            string ftype = qdata.BMEDKqtyFLOWTYPE;
            string dptid = qdata.BMEDKqtyDPTID;
            string qtyDate1 = qdata.BMEDKqtyApplyDateFrom;
            string qtyDate2 = qdata.BMEDKqtyApplyDateTo;
            string qtyKeepResult = qdata.BMEDKqtyKeepResult;
            string qtyIsCharged = qdata.BMEDKqtyIsCharged;
            string qtyDateType = qdata.BMEDKqtyDateType;
            bool searchAllDoc = qdata.BMEDKqtySearchAllDoc;
            string qtyEngCode = qdata.BMEDKqtyEngCode;
            string qtyTicketNo = qdata.BMEDKqtyTicketNo;
            string qtyVendor = qdata.BMEDKqtyVendor;

            DateTime applyDateFrom = DateTime.Now;
            DateTime applyDateTo = DateTime.Now;
            /* Dealing search by date. */
            if (qtyDate1 != null && qtyDate2 != null)// If 2 date inputs have been insert, compare 2 dates.
            {
                DateTime date1 = DateTime.Parse(qtyDate1);
                DateTime date2 = DateTime.Parse(qtyDate2);
                int result = DateTime.Compare(date1, date2);
                if (result < 0)
                {
                    applyDateFrom = date1.Date;
                    applyDateTo = date2.Date;
                }
                else if (result == 0)
                {
                    applyDateFrom = date1.Date;
                    applyDateTo = date1.Date;
                }
                else
                {
                    applyDateFrom = date2.Date;
                    applyDateTo = date1.Date;
                }
            }
            else if (qtyDate1 == null && qtyDate2 != null)
            {
                applyDateFrom = DateTime.Parse(qtyDate2);
                applyDateTo = DateTime.Parse(qtyDate2);
            }
            else if (qtyDate1 != null && qtyDate2 == null)
            {
                applyDateFrom = DateTime.Parse(qtyDate1);
                applyDateTo = DateTime.Parse(qtyDate1);
            }


            List<KeepSearchListViewModel> kv = new List<KeepSearchListViewModel>();

            /* Querying data. */
            var kps = _context.BMEDKeeps.ToList();
            var keepFlows = _context.BMEDKeepFlows.ToList();
            var keepDtls = _context.BMEDKeepDtls.ToList();
            if (!string.IsNullOrEmpty(docid))   //表單編號
            {
                docid = docid.Trim();
                kps = kps.Where(v => v.DocId == docid).ToList();
            }
            if (!string.IsNullOrEmpty(ano))     //財產編號
            {
                kps = kps.Where(v => v.AssetNo == ano).ToList();
            }
            if (!string.IsNullOrEmpty(dptid))   //所屬部門編號
            {
                kps = kps.Where(v => v.DptId == dptid).ToList();
            }
            if (!string.IsNullOrEmpty(acc))     //成本中心
            {
                kps = kps.Where(v => v.AccDpt == acc).ToList();
            }
            if (!string.IsNullOrEmpty(aname))   //財產名稱
            {
                kps = kps.Where(v => v.AssetName != null)
                         .Where(v => v.AssetName.Contains(aname))
                         .ToList();
            }
            if (!string.IsNullOrEmpty(qtyTicketNo))   //發票號碼
            {
                qtyTicketNo = qtyTicketNo.ToUpper();
                var resultDocIds = _context.BMEDKeepCosts.Include(kc => kc.TicketDtl)
                                                         .Where(kc => kc.TicketDtl.TicketDtlNo == qtyTicketNo)
                                                         .Select(kc => kc.DocId).Distinct();
                kps = (from k in kps
                       where resultDocIds.Any(val => k.DocId.Contains(val))
                       select k).ToList();
            }
            if (!string.IsNullOrEmpty(qtyVendor))   //廠商關鍵字
            {
                var resultDocIds = _context.BMEDKeepCosts.Include(kc => kc.TicketDtl)
                                                         .Where(kc => kc.VendorName.Contains(qtyVendor))
                                                         .Select(kc => kc.DocId).Distinct();
                kps = (from k in kps
                       where resultDocIds.Any(val => k.DocId.Contains(val))
                       select k).ToList();
            }
            if (!string.IsNullOrEmpty(qtyEngCode))     //負責工程師
            {
                kps = kps.Where(v => v.EngId == Convert.ToInt32(qtyEngCode)).ToList();
            }
            /* Search date by DateType.(ApplyDate) */
            if (string.IsNullOrEmpty(qtyDate1) == false || string.IsNullOrEmpty(qtyDate2) == false) //送單日
            {
                if (qtyDateType == "送單日")
                {
                    kps = kps.Where(v => v.SentDate >= applyDateFrom && v.SentDate <= applyDateTo).ToList();
                }
            }

            if (!string.IsNullOrEmpty(ftype))   //流程狀態
            {
                switch (ftype)
                {
                    case "未結案":
                        keepFlows = keepFlows.GroupBy(f => f.DocId).Where(group => group.Last().Status == "?")
                                                                   .Select(group => group.Last()).ToList();
                        break;
                    case "已結案":
                        keepFlows = keepFlows.GroupBy(f => f.DocId).Where(group => group.Last().Status == "2")
                                                                   .Select(group => group.Last()).ToList();
                        break;
                }
            }
            else
            {
                keepFlows = keepFlows.GroupBy(f => f.DocId).Where(group => group.Last().Status != "3")
                                                           .Select(group => group.Last()).ToList(); ;
            }

            /* If no search result. */
            if (kps.Count() == 0)
            {
                return View("KeepQryList", kv);
            }

            kps.Join(keepFlows, k => k.DocId, f => f.DocId,
                (k, f) => new
                {
                    keep = k,
                    flow = f
                })
                .Join(_context.BMEDAssets, k => k.keep.AssetNo, a => a.AssetNo,
                (k, a) => new
                {
                    keep = k.keep,
                    asset = a,
                    flow = k.flow
                })
                .Join(_context.BMEDKeepDtls, m => m.keep.DocId, d => d.DocId,
                (m, d) => new
                {
                    keep = m.keep,
                    flow = m.flow,
                    asset = m.asset,
                    keepdtl = d
                })
                .Join(_context.Departments, j => j.keep.AccDpt, d => d.DptId,
                (j, d) => new
                {
                    keep = j.keep,
                    flow = j.flow,
                    asset = j.asset,
                    keepdtl = j.keepdtl,
                    dpt = d
                }).ToList()
                .ForEach(j => kv.Add(new KeepSearchListViewModel
                {
                    DocType = "醫工保養",
                    DocId = j.keep.DocId,
                    AssetNo = j.keep.AssetNo,
                    AssetName = j.keep.AssetName,
                    Brand = j.asset.Brand,
                    Type = j.asset.Type,
                    PlaceLoc = j.keep.PlaceLoc,
                    ApplyDpt = j.keep.DptId,
                    AccDpt = j.keep.AccDpt,
                    AccDptName = j.dpt.Name_C,
                    Result = (j.keepdtl.Result == null || j.keepdtl.Result == 0) ? "" : _context.BMEDKeepResults.Find(j.keepdtl.Result).Title,
                    InOut = j.keepdtl.InOut == "0" ? "自行" :
                    j.keepdtl.InOut == "1" ? "委外" :
                    j.keepdtl.InOut == "2" ? "租賃" :
                    j.keepdtl.InOut == "3" ? "保固" : "",
                    Memo = j.keepdtl.Memo,
                    Cost = j.keepdtl.Cost,
                    Days = DateTime.Now.Subtract(j.keep.SentDate.GetValueOrDefault()).Days,
                    Flg = j.flow.Status,
                    FlowUid = j.flow.UserId,
                    FlowCls = j.flow.Cls,
                    FlowUidName = _context.AppUsers.Find(j.flow.UserId).FullName,
                    Src = j.keep.Src,
                    SentDate = j.keep.SentDate,
                    EndDate = j.keepdtl.EndDate,
                    CloseDate = j.keepdtl.CloseDate,
                    IsCharged = j.keepdtl.IsCharged,
                    keepdata = j.keep
                }));

            /* Search date by DateType. */
            if (string.IsNullOrEmpty(qtyDate1) == false || string.IsNullOrEmpty(qtyDate2) == false)
            {
                if (qtyDateType == "結案日")
                {
                    kv = kv.Where(v => v.CloseDate >= applyDateFrom && v.CloseDate <= applyDateTo).ToList();
                }
                else if (qtyDateType == "完工日")
                {
                    kv = kv.Where(v => v.EndDate >= applyDateFrom && v.EndDate <= applyDateTo).ToList();
                }
            }

            /* Sorting search result. */
            if (kv.Count() != 0)
            {
                if (qtyDateType == "結案日")
                {
                    kv = kv.OrderByDescending(r => r.CloseDate).ThenByDescending(r => r.DocId).ToList();
                }
                else if (qtyDateType == "完工日")
                {
                    kv = kv.OrderByDescending(r => r.EndDate).ThenByDescending(r => r.DocId).ToList();
                }
                else
                {
                    kv = kv.OrderByDescending(r => r.SentDate).ThenByDescending(r => r.DocId).ToList();
                }
            }

            /* Search KeepResults. */
            if (!string.IsNullOrEmpty(qtyKeepResult))
            {
                kv = kv.Where(r => r.Result == _context.BMEDKeepResults.Find(Convert.ToInt32(qtyKeepResult)).Title).ToList();
            }
            /* Search IsCharged. */
            if (!string.IsNullOrEmpty(qtyIsCharged))
            {
                kv = kv.Where(r => r.IsCharged == qtyIsCharged).ToList();
            }

            return View("KeepQryList", kv);
        }

    }
}