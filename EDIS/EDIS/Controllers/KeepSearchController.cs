using EDIS.Data;
using EDIS.Models.KeepModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Controllers
{
    [Authorize]
    public class KeepSearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KeepSearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: KeepSearch/
        public IActionResult Index()
        {
            List<SelectListItem> FlowlistItem = new List<SelectListItem>();
            FlowlistItem.Add(new SelectListItem { Text = "未結案", Value = "未結案" });
            FlowlistItem.Add(new SelectListItem { Text = "已結案", Value = "已結案" });
            ViewData["KeepFlowType"] = new SelectList(FlowlistItem, "Value", "Text");

            /* 成本中心 & 申請部門的下拉選單資料 */
            var dptList = new[] { "K", "P", "C" };   //本院部門
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
            ViewData["KeepAccDpt"] = new SelectList(listItem, "Value", "Text");
            ViewData["KeepApplyDpt"] = new SelectList(listItem, "Value", "Text");

            /* 處理保養狀態的下拉選單 */
            var keepResults = _context.KeepResults.ToList();
            List<SelectListItem> listItem2 = new List<SelectListItem>();
            foreach (var item in keepResults)
            {
                listItem2.Add(new SelectListItem
                {
                    Text = item.Title,
                    Value = item.Id.ToString()
                });
            }
            ViewData["KeepResult"] = new SelectList(listItem2, "Value", "Text");

            /* 處理有無費用的下拉選單 */
            List<SelectListItem> listItem3 = new List<SelectListItem>();
            listItem3.Add(new SelectListItem { Text = "有", Value = "Y" });
            listItem3.Add(new SelectListItem { Text = "無", Value = "N" });
            ViewData["KeepIsCharged"] = new SelectList(listItem3, "Value", "Text");

            /* 處理日期查詢的下拉選單 */
            List<SelectListItem> listItem4 = new List<SelectListItem>();
            listItem4.Add(new SelectListItem { Text = "送單日", Value = "送單日" });
            listItem4.Add(new SelectListItem { Text = "完工日", Value = "完工日" });
            listItem4.Add(new SelectListItem { Text = "結案日", Value = "結案日" });
            ViewData["KeepDateType"] = new SelectList(listItem4, "Value", "Text", "送單日");

            QryKeepListData data = new QryKeepListData();

            return View(data);
        }

        // POST: KeepSearch/GetQueryList
        [HttpPost]
        public IActionResult GetQueryList(QryKeepListData qdata)
        {
            string docid = qdata.KqtyDOCID;
            string ano = qdata.KqtyASSETNO;
            string acc = qdata.KqtyACCDPT;
            string aname = qdata.KqtyASSETNAME;
            string ftype = qdata.KqtyFLOWTYPE;
            string dptid = qdata.KqtyDPTID;
            string qtyDate1 = qdata.KqtyApplyDateFrom;
            string qtyDate2 = qdata.KqtyApplyDateTo;
            string qtyKeepResult = qdata.KqtyKeepResult;
            string qtyIsCharged = qdata.KqtyIsCharged;
            string qtyDateType = qdata.KqtyDateType;
            string qtyTicketNo = qdata.KqtyTicketNo;
            string qtyVendor = qdata.KqtyVendor;
            string qtyOrderType = qdata.KqtyOrderType;

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
            var kps = _context.Keeps.ToList();
            var keepFlows = _context.KeepFlows.ToList();
            var keepDtls = _context.KeepDtls.ToList();
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
            if (!string.IsNullOrEmpty(aname))   //物品名稱(關鍵字)
            {
                kps = kps.Where(v => v.AssetName != null)
                         .Where(v => v.AssetName.Contains(aname))
                         .ToList();
            }
            if (!string.IsNullOrEmpty(qtyTicketNo))   //發票號碼
            {
                qtyTicketNo = qtyTicketNo.ToUpper();
                var resultDocIds = _context.KeepCosts.Include(kc => kc.TicketDtl)
                                                     .Where(kc => kc.TicketDtl.TicketDtlNo == qtyTicketNo)
                                                     .Select(kc => kc.DocId).Distinct();
                kps = (from k in kps
                       where resultDocIds.Any(val => k.DocId.Contains(val))
                       select k).ToList();
            }
            if (!string.IsNullOrEmpty(qtyVendor))   //廠商關鍵字
            {
                var resultDocIds = _context.KeepCosts.Include(kc => kc.TicketDtl)
                                                     .Where(kc => kc.VendorName.Contains(qtyVendor))
                                                     .Select(kc => kc.DocId).Distinct();
                kps = (from k in kps
                       where resultDocIds.Any(val => k.DocId.Contains(val))
                       select k).ToList();
            }
            if (string.IsNullOrEmpty(qtyDate1) == false || string.IsNullOrEmpty(qtyDate2) == false)  //送單日
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
                return View("SearchList", kv);
            }
            kps.Join(keepFlows, r => r.DocId, f => f.DocId,
                (k, f) => new
                {
                    keep = k,
                    flow = f
                })
                .Join(keepDtls, m => m.keep.DocId, d => d.DocId,
                (m, d) => new
                {
                    keep = m.keep,
                    flow = m.flow,
                    keepdtl = d
                })
                .Join(_context.Departments, j => j.keep.AccDpt, d => d.DptId,
                (j, d) => new
                {
                    keep = j.keep,
                    flow = j.flow,
                    keepdtl = j.keepdtl,
                    dpt = d
                })
                .ToList()
                .ForEach(j => kv.Add(new KeepSearchListViewModel
                {
                    DocType = "工務保養",
                    DocId = j.keep.DocId,
                    PlaceLoc = j.keep.PlaceLoc,
                    ApplyDpt = j.keep.DptId,
                    AccDpt = j.keep.AccDpt,
                    AccDptName = j.dpt.Name_C,
                    Result = (j.keepdtl.Result == null || j.keepdtl.Result == 0) ? "" : _context.KeepResults.Find(j.keepdtl.Result).Title,
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
                    IsCharged = j.keepdtl.IsCharged,
                    keepdata = j.keep
                }));

            /* 設備編號"有"、"無"的對應，"有"讀取table相關data，"無"只顯示申請人輸入的設備名稱 */
            foreach (var item in kv)
            {
                if (!string.IsNullOrEmpty(item.keepdata.AssetNo))
                {
                    var asset = _context.Assets.Where(a => a.AssetNo == item.keepdata.AssetNo).FirstOrDefault();
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
                    item.AssetName = item.keepdata.AssetName;
                }
            }

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
                if (qtyOrderType == "結案日")
                {
                    kv = kv.OrderByDescending(r => r.CloseDate).ThenByDescending(r => r.DocId).ToList();
                }
                else if (qtyOrderType == "完工日")
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
                kv = kv.Where(r => r.Result == _context.KeepResults.Find(Convert.ToInt32(qtyKeepResult)).Title).ToList();
            }
            /* Search IsCharged. */
            if (!string.IsNullOrEmpty(qtyIsCharged))
            {
                kv = kv.Where(r => r.IsCharged == qtyIsCharged).ToList();
            }

            return View("SearchList", kv);
        }

    }
}