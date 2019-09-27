using EDIS.Data;
using EDIS.Models.RepairModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Controllers
{
    [Authorize]
    public class RepairSearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RepairSearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RepairSearch/
        public IActionResult Index()
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
            var dealStatuses = _context.DealStatuses.ToList();
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

            /* 有無費用 的下拉選單 */
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

            /* 請修類別 的下拉選單 */
            List<SelectListItem> listItem5 = new List<SelectListItem>();
            listItem5.Add(new SelectListItem { Text = "請修", Value = "請修" });
            listItem5.Add(new SelectListItem { Text = "送修", Value = "送修" });
            listItem5.Add(new SelectListItem { Text = "增設", Value = "增設" });
            ViewData["qtyRepType"] = new SelectList(listItem5, "Value", "Text");

            QryRepListData data = new QryRepListData();

            return View(data);
        }

        // POST: RepairSearch/GetQueryList
        [HttpPost]
        public ActionResult GetQueryList(QryRepListData qdata)
        {
            string docid = qdata.qtyDOCID;
            string ano = qdata.qtyASSETNO;
            string acc = qdata.qtyACCDPT;
            string aname = qdata.qtyASSETNAME;
            string ftype = qdata.qtyFLOWTYPE;
            string dptid = qdata.qtyDPTID;
            string qtyDate1 = qdata.qtyApplyDateFrom;
            string qtyDate2 = qdata.qtyApplyDateTo;
            string qtyDealStatus = qdata.qtyDealStatus;
            string qtyIsCharged = qdata.qtyIsCharged;
            string qtyDateType = qdata.qtyDateType;
            string qtyRepType = qdata.qtyRepType;
            string qtyTroubleDes = qdata.qtyTroubleDes;

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
            var rps = _context.Repairs.ToList();
            var repairFlows = _context.RepairFlows.ToList();
            var repairDtls = _context.RepairDtls.ToList();
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
            if (!string.IsNullOrEmpty(aname))   //物品名稱(關鍵字)
            {
                rps = rps.Where(v => v.AssetName != null)
                         .Where(v => v.AssetName.Contains(aname)).ToList();
            }
            if (!string.IsNullOrEmpty(qtyRepType))  //請修類別
            {
                rps = rps.Where(v => v.RepType == qtyRepType).ToList();
            }
            if (!string.IsNullOrEmpty(qtyTroubleDes))   //錯誤描述(關鍵字)
            {
                rps = rps.Where(v => v.TroubleDes.Contains(qtyTroubleDes)).ToList();
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
                switch(ftype)
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
            /* Search IsCharged. */
            if (!string.IsNullOrEmpty(qtyIsCharged))    //有無費用
            {
                repairDtls = repairDtls.Where(r => r.IsCharged == qtyIsCharged).ToList();
            }

            /* If no search result. */
            if (rps.Count() == 0)
            {
                return View("SearchList", rv);
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
                    DocType = "請修",
                    RepType = j.repair.RepType,
                    DocId = j.repair.DocId,
                    ApplyDate = j.repair.ApplyDate,
                    PlaceLoc = j.repair.LocType,
                    ApplyDpt = j.repair.DptId,
                    AccDpt = j.repair.AccDpt,
                    AccDptName = j.dpt.Name_C,
                    TroubleDes = j.repair.TroubleDes,
                    DealState = _context.DealStatuses.Find(j.repdtl.DealState).Title,
                    DealDes = j.repdtl.DealDes,
                    Cost = j.repdtl.Cost,
                    Days = DateTime.Now.Subtract(j.repair.ApplyDate).Days,
                    Flg = j.flow.Status,
                    FlowUid = j.flow.UserId,
                    FlowCls = j.flow.Cls,
                    FlowUidName = _context.AppUsers.Find(j.flow.UserId).FullName,
                    EndDate = j.repdtl.EndDate,
                    CloseDate = j.repdtl.CloseDate,
                    repdata = j.repair
                }));

            /* 設備編號"有"、"無"的對應，"有"讀取table相關data，"無"只顯示申請人輸入的設備名稱 */
            foreach (var item in rv)
            {
                var repairDoc = _context.Repairs.Find(item.DocId);
                if (repairDoc.AssetNo != null)
                {
                    var asset = _context.Assets.Where(a => a.AssetNo == repairDoc.AssetNo).FirstOrDefault();
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

            return View("SearchList", rv);
        }
    }
}