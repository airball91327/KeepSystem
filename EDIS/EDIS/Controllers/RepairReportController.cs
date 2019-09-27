using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using EDIS.Data;
using EDIS.Models;
using EDIS.Models.Identity;
using EDIS.Models.LocationModels;
using EDIS.Models.RepairModels;
using EDIS.Repositories;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Controllers
{
    [Authorize]
    public class RepairReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CustomUserManager userManager;
        private readonly CustomRoleManager roleManager;

        public RepairReportController(ApplicationDbContext context,
                                      CustomUserManager customUserManager,
                                      CustomRoleManager customRoleManager)
        {
            _context = context;
            userManager = customUserManager;
            roleManager = customRoleManager;
        }

        // GET: /RepairReport/Index
        public ActionResult Index()
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = "工務部(8410)", Value = "8410" });
            listItem.Add(new SelectListItem { Text = "工務一課(8411)", Value = "8411" });
            listItem.Add(new SelectListItem { Text = "工務二課(8412)", Value = "8412" });
            listItem.Add(new SelectListItem { Text = "工務三課－中華工務組(8413)", Value = "8413" });
            listItem.Add(new SelectListItem { Text = "工務三課－教研工務組(8414)", Value = "8414" });
            listItem.Add(new SelectListItem { Text = "外包人員", Value = "0000" });
            ViewData["DPTID"] = new SelectList(listItem, "Value", "Text", "");

            return View();
        }

        // GET: /RepairReport/ExportToExcel
        public IActionResult ExportToExcel(DateTime qtyMonth, string qtyDptId)
        {
            DateTime reportMonth = qtyMonth;

            var repairFlows = _context.RepairFlows.OrderBy(rf => rf.StepId).GroupBy(rf => rf.DocId)
                                                  .Select(g => g.LastOrDefault()).ToList();
            //剔除廢除的單
            repairFlows = repairFlows.Where(rf => rf.Status != "3").ToList();

            //申請日為該年當月的所有案件
            var qtyRepairs = _context.Repairs.Where(r => r.ApplyDate.Month == reportMonth.Month &&
                                                         r.ApplyDate.Year == reportMonth.Year)
                                             .Join(_context.RepairDtls, r => r.DocId, d => d.DocId,
                                             (r, d) => new
                                             {
                                                 repair = r,
                                                 repdtl = d
                                             })
                                             .Join(repairFlows, r => r.repair.DocId, rf => rf.DocId,
                                             (r, rf) => new
                                             {
                                                 repair = r.repair,
                                                 repdtl = r.repdtl,
                                                 repflow = rf
                                             }).ToList();

            //找出各案件的負責工程師(流程最後一位工程師)
            foreach (var item in qtyRepairs)
            {
                var tempEng = _context.RepairFlows.Where(rf => rf.DocId == item.repair.DocId)
                                                  .Where(rf => rf.Cls.Contains("工程師"))
                                                  .OrderByDescending(rf => rf.StepId).FirstOrDefault();
                if (tempEng != null)
                {
                    item.repair.EngId = tempEng.UserId;
                    // EngName暫存Engineer所屬部門代號
                    var eng = _context.AppUsers.Find(tempEng.UserId);
                    if (eng.FullName.Contains("外包") == true)
                    {
                        item.repair.EngName = "0000";
                    }
                    else if(eng.UserName == "344027")
                    {
                        item.repair.EngName = "7084";
                    }
                    else
                    {
                        item.repair.EngName = eng.DptId;
                    }
                }
                else
                {
                    // EngName暫存Engineer所屬部門代號
                    var eng = _context.AppUsers.Find(item.repair.EngId);
                    if (eng.FullName.Contains("外包") == true)
                    {
                        item.repair.EngName = "0000";
                    }
                    else if (eng.UserName == "344027")
                    {
                        item.repair.EngName = "7084";
                    }
                    else
                    {
                        item.repair.EngName = eng.DptId;
                    }
                }
            }

            //申請日為當年度的所有案件
            var qtyYearRepairs = _context.Repairs.Where(r => r.ApplyDate.Year == reportMonth.Year)
                                                 .Join(_context.RepairDtls, r => r.DocId, d => d.DocId,
                                                 (r, d) => new
                                                 {
                                                     repair = r,
                                                     repdtl = d
                                                 })
                                                 .Join(repairFlows, r => r.repair.DocId, rf => rf.DocId,
                                                 (r, rf) => new
                                                 {
                                                     repair = r.repair,
                                                     repdtl = r.repdtl,
                                                     repflow = rf
                                                 }).ToList();

            //找出各案件的負責工程師(年度)(流程最後一位工程師)
            foreach (var item in qtyYearRepairs)
            {
                var tempEng = _context.RepairFlows.Where(rf => rf.DocId == item.repair.DocId)
                                                  .Where(rf => rf.Cls.Contains("工程師"))
                                                  .OrderByDescending(rf => rf.StepId).FirstOrDefault();
                if (tempEng != null)
                {
                    item.repair.EngId = tempEng.UserId;
                    // EngName暫存Engineer所屬部門代號
                    var eng = _context.AppUsers.Find(tempEng.UserId);
                    if (eng.FullName.Contains("外包") == true)
                    {
                        item.repair.EngName = "0000";
                    }
                    else if (eng.UserName == "344027")
                    {
                        item.repair.EngName = "7084";
                    }
                    else
                    {
                        item.repair.EngName = eng.DptId;
                    }
                }
                else
                {
                    // EngName暫存Engineer所屬部門代號
                    var eng = _context.AppUsers.Find(item.repair.EngId);
                    if (eng.FullName.Contains("外包") == true)
                    {
                        item.repair.EngName = "0000";
                    }
                    else if (eng.UserName == "344027")
                    {
                        item.repair.EngName = "7084";
                    }
                    else
                    {
                        item.repair.EngName = eng.DptId;
                    }
                }
            }

            // 依照搜尋部門篩選資料
            var qtyRepairs2 = qtyRepairs.ToList();
            var qtyYearRepairs2 = qtyYearRepairs.ToList();
            if (qtyDptId == "8410")
            {
                qtyRepairs2 = qtyRepairs2.Where(r => r.repair.EngName == "8410").ToList();
                qtyYearRepairs2 = qtyYearRepairs2.Where(r => r.repair.EngName == "8410").ToList();
            }
            else if (qtyDptId == "8411")
            {
                qtyRepairs2 = qtyRepairs2.Where(r => r.repair.EngName == "8411").ToList();
                qtyYearRepairs2 = qtyYearRepairs2.Where(r => r.repair.EngName == "8411").ToList();
            }
            else if (qtyDptId == "8412")
            {
                qtyRepairs2 = qtyRepairs2.Where(r => r.repair.EngName == "8412").ToList();
                qtyYearRepairs2 = qtyYearRepairs2.Where(r => r.repair.EngName == "8412").ToList();
            }
            else if (qtyDptId == "8413")
            {
                qtyRepairs2 = qtyRepairs2.Where(r => r.repair.EngName == "8413").ToList();
                qtyYearRepairs2 = qtyYearRepairs2.Where(r => r.repair.EngName == "8413").ToList();
            }
            else if (qtyDptId == "8414")
            {
                qtyRepairs2 = qtyRepairs2.Where(r => r.repair.EngName == "8414").ToList();
                qtyYearRepairs2 = qtyYearRepairs2.Where(r => r.repair.EngName == "8414").ToList();
            }
            else if (qtyDptId == "0000")    //外包人員
            {
                qtyRepairs2 = qtyRepairs2.Where(r => r.repair.EngName == "0000").ToList();
                qtyYearRepairs2 = qtyYearRepairs2.Where(r => r.repair.EngName == "0000").ToList();
            }
            else
            {
                qtyRepairs2 = qtyRepairs2.Where(r => r.repair.EngName == "8410" || r.repair.EngName == "8411" ||
                                                     r.repair.EngName == "8412" || r.repair.EngName == "8413" ||
                                                     r.repair.EngName == "8414").ToList();
                qtyYearRepairs2 = qtyYearRepairs2.Where(r => r.repair.EngName == "8410" || r.repair.EngName == "8411" ||
                                                             r.repair.EngName == "8412" || r.repair.EngName == "8413" ||
                                                             r.repair.EngName == "8414").ToList();
            }

            // 增設(內修、外修、內外修)、維修(內修、外修、內外修)、報廢件數(月為單位)
            var repAdds = qtyRepairs2.Where(r => r.repair.RepType == "增設");
            var repAddIns = qtyRepairs2.Where(r => r.repair.RepType == "增設" && r.repdtl.InOut == "內修");
            var repAddOuts = qtyRepairs2.Where(r => r.repair.RepType == "增設" && r.repdtl.InOut == "外修");
            var repAddInOuts = qtyRepairs2.Where(r => r.repair.RepType == "增設" && r.repdtl.InOut == "內外修");
            var repIns = qtyRepairs2.Where(r => r.repair.RepType != "增設" && r.repdtl.InOut == "內修" && r.repdtl.DealState != 4);
            var repOuts = qtyRepairs2.Where(r => r.repair.RepType != "增設" && r.repdtl.InOut == "外修" && r.repdtl.DealState != 4);
            var repInOuts = qtyRepairs2.Where(r => r.repair.RepType != "增設" && r.repdtl.InOut == "內外修" && r.repdtl.DealState != 4);
            var repScraps = qtyRepairs2.Where(r => r.repair.RepType != "增設" && r.repdtl.DealState == 4);

            //ClosedXML的用法 先new一個Excel Workbook
            using (XLWorkbook workbook = new XLWorkbook())
            {

                //一個workbook內至少會有一個worksheet,並將資料Insert至這個位於A1這個位置上
                //WorkSheet1
                var ws = workbook.Worksheets.Add("綜合月指標", 1);
                //Style
                ws.ColumnWidth = 15;

                //Title1  【2019年各月統計】
                ws.Cell(1, 1).Value = "【" + qtyMonth.Year + "年" + "各月統計】";
                ws.Cell(3, 1).Value = "增設";
                ws.Cell(3, 2).Value = "已完工件數";
                ws.Cell(4, 2).Value = "未完工件數";
                ws.Cell(5, 2).Value = "已結案件數";
                ws.Cell(6, 2).Value = "未結案件數";
                ws.Cell(7, 2).Value = "完工率";
                ws.Cell(8, 2).Value = "結案率";
                ws.Cell(9, 1).Value = "維修";
                ws.Cell(9, 2).Value = "已完工件數";
                ws.Cell(10, 2).Value = "未完工件數";
                ws.Cell(11, 2).Value = "已結案件數";
                ws.Cell(12, 2).Value = "未結案件數";
                ws.Cell(13, 2).Value = "完工率";
                ws.Cell(14, 2).Value = "結案率";

                //Data1

                for (int month = 1; month <= 12; month++)
                {
                    var monthRepAdds = qtyYearRepairs2.Where(r => r.repair.ApplyDate.Month == month)
                                                      .Where(r => r.repair.RepType == "增設").ToList();
                    var monthReps = qtyYearRepairs2.Where(r => r.repair.ApplyDate.Month == month)
                                                   .Where(r => r.repair.RepType != "增設" && r.repdtl.DealState != 4).ToList();

                    ws.Cell(2, (month + 2)).SetValue(month + "月");
                    //增設
                    ws.Cell(3, (month + 2)).Value = monthRepAdds.Where(r => r.repdtl.EndDate != null).Count();
                    ws.Cell(4, (month + 2)).Value = monthRepAdds.Where(r => r.repdtl.EndDate == null).Count();
                    ws.Cell(5, (month + 2)).Value = monthRepAdds.Where(r => r.repdtl.CloseDate != null).Count();
                    ws.Cell(6, (month + 2)).Value = monthRepAdds.Where(r => r.repdtl.CloseDate == null).Count();
                    ws.Cell(7, (month + 2)).Value = monthRepAdds.Count() != 0 ? (Convert.ToDecimal(monthRepAdds.Where(r => r.repdtl.EndDate != null).Count()) / Convert.ToDecimal(monthRepAdds.Count())).ToString("P") : "0.00%";
                    ws.Cell(8, (month + 2)).Value = monthRepAdds.Count() != 0 ? (Convert.ToDecimal(monthRepAdds.Where(r => r.repdtl.CloseDate != null).Count()) / Convert.ToDecimal(monthRepAdds.Count())).ToString("P") : "0.00%";
                    //Style
                    ws.Cell(7, (month + 2)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(8, (month + 2)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //維修
                    ws.Cell(9, (month + 2)).Value = monthReps.Where(r => r.repdtl.EndDate != null).Count();
                    ws.Cell(10, (month + 2)).Value = monthReps.Where(r => r.repdtl.EndDate == null).Count();
                    ws.Cell(11, (month + 2)).Value = monthReps.Where(r => r.repdtl.CloseDate != null).Count();
                    ws.Cell(12, (month + 2)).Value = monthReps.Where(r => r.repdtl.CloseDate == null).Count();
                    ws.Cell(13, (month + 2)).Value = monthReps.Count() != 0 ? (Convert.ToDecimal(monthReps.Where(r => r.repdtl.EndDate != null).Count()) / Convert.ToDecimal(monthReps.Count())).ToString("P") : "0.00%";
                    ws.Cell(14, (month + 2)).Value = monthReps.Count() != 0 ? (Convert.ToDecimal(monthReps.Where(r => r.repdtl.CloseDate != null).Count()) / Convert.ToDecimal(monthReps.Count())).ToString("P") : "0.00%";
                    //Style
                    ws.Cell(13, (month + 2)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(14, (month + 2)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }

                //Title2  【每月維修件數(申請日為該月的案件)】
                ws.Cell(17, 1).Value = "【" + qtyMonth.Year + "年" + qtyMonth.Month + "月維修件數】";
                ws.Cell(18, 1).Value = "維修(內修)";
                ws.Cell(18, 2).Value = "維修(外修)";
                ws.Cell(18, 3).Value = "維修(內外修)";
                ws.Cell(18, 4).Value = "報廢";
                ws.Cell(18, 5).Value = "維修尚未處理";
                ws.Cell(18, 6).Value = "維修總件數";

                //Data2
                ws.Cell(19, 1).Value = repIns.Count();
                ws.Cell(19, 2).Value = repOuts.Count();
                ws.Cell(19, 3).Value = repInOuts.Count();
                ws.Cell(19, 4).Value = repScraps.Count();
                ws.Cell(19, 5).Value = qtyRepairs2.Where(r => r.repair.RepType != "增設" && r.repdtl.InOut == null).Count();
                ws.Cell(19, 6).Value = qtyRepairs2.Count(r => r.repair.RepType != "增設");

                //Title3  【每月增設件數(申請日為該月的案件)】
                ws.Cell(21, 1).Value = "【" + qtyMonth.Year + "年" + qtyMonth.Month + "月增設件數】";
                ws.Cell(22, 1).Value = "增設(內修)";
                ws.Cell(22, 2).Value = "增設(外修)";
                ws.Cell(22, 3).Value = "增設(內外修)";
                ws.Cell(22, 4).Value = "增設尚未處理";
                ws.Cell(22, 5).Value = "增設總件數";

                //Data3
                ws.Cell(23, 1).Value = repAddIns.Count();
                ws.Cell(23, 2).Value = repAddOuts.Count();
                ws.Cell(23, 3).Value = repAddInOuts.Count();
                ws.Cell(23, 4).Value = qtyRepairs2.Where(r => r.repair.RepType == "增設" && r.repdtl.InOut == null).Count();
                ws.Cell(23, 5).Value = qtyRepairs2.Count(r => r.repair.RepType == "增設");

                //Title4  【每月件數(申請日為該月的案件)】
                ws.Cell(25, 1).Value = "【" + qtyMonth.Year + "年" + qtyMonth.Month + "月總件數】";
                ws.Cell(26, 1).Value = "增設";
                ws.Cell(26, 2).Value = "維修";
                ws.Cell(26, 3).Value = "報廢";
                ws.Cell(26, 4).Value = "總件數";

                //Data4
                ws.Cell(27, 1).Value = qtyRepairs2.Count(r => r.repair.RepType == "增設");
                ws.Cell(27, 2).Value = qtyRepairs2.Count(r => r.repair.RepType != "增設" && r.repdtl.DealState != 4);
                ws.Cell(27, 3).Value = repScraps.Count();
                ws.Cell(27, 4).Value = qtyRepairs2.Count();

                //Title2    【維修完成、結案率 (該月申請且已完成或已結案案件 / 該月申請各相對總件數)】
                //ws.Cell(5, 1).Value = "【維修完成率】";
                //ws.Cell(5, 5).Value = "【維修結案率】";
                //ws.Cell(5, 9).Value = "【未結案率】";
                //ws.Cell(6, 1).Value = "增設";
                //ws.Cell(6, 2).Value = "維修(內修)";
                //ws.Cell(6, 3).Value = "維修(外修)";
                //ws.Cell(6, 4).Value = "維修(內外修)";
                //ws.Cell(6, 5).Value = "增設";
                //ws.Cell(6, 6).Value = "維修(內修)";
                //ws.Cell(6, 7).Value = "維修(外修)";
                //ws.Cell(6, 8).Value = "維修(內外修)";
                //ws.Cell(6, 9).Value = "增設";
                //ws.Cell(6, 10).Value = "維修(內修)";
                //ws.Cell(6, 11).Value = "維修(外修)";
                //ws.Cell(6, 12).Value = "維修(內外修)";

                ////Data2         //.ToString("P")轉為百分比顯示的字串
                //ws.Cell(7, 1).Value = repAdds.Count() != 0 ? (Convert.ToDecimal(repAdds.Where(r => r.repdtl.EndDate != null).Count()) / Convert.ToDecimal(repAdds.Count())).ToString("P") : "0.00%";
                //ws.Cell(7, 2).Value = repIns.Count() != 0 ? (Convert.ToDecimal(repIns.Where(r => r.repdtl.EndDate != null).Count()) / Convert.ToDecimal(repIns.Count())).ToString("P") : "0.00%";
                //ws.Cell(7, 3).Value = repOuts.Count() != 0 ? (Convert.ToDecimal(repOuts.Where(r => r.repdtl.EndDate != null).Count()) / Convert.ToDecimal(repOuts.Count())).ToString("P") : "0.00%";
                //ws.Cell(7, 4).Value = repInOuts.Count() != 0 ? (Convert.ToDecimal(repInOuts.Where(r => r.repdtl.EndDate != null).Count()) / Convert.ToDecimal(repInOuts.Count())).ToString("P") : "0.00%";
                //ws.Cell(7, 5).Value = repAdds.Count() != 0 ? (Convert.ToDecimal(repAdds.Where(r => r.repflow.Status == "2").Count()) / Convert.ToDecimal(repAdds.Count())).ToString("P") : "0.00%";
                //ws.Cell(7, 6).Value = repIns.Count() != 0 ? (Convert.ToDecimal(repIns.Where(r => r.repflow.Status == "2").Count()) / Convert.ToDecimal(repIns.Count())).ToString("P") : "0.00%";
                //ws.Cell(7, 7).Value = repOuts.Count() != 0 ? (Convert.ToDecimal(repOuts.Where(r => r.repflow.Status == "2").Count()) / Convert.ToDecimal(repOuts.Count())).ToString("P") : "0.00%";
                //ws.Cell(7, 8).Value = repInOuts.Count() != 0 ? (Convert.ToDecimal(repInOuts.Where(r => r.repflow.Status == "2").Count()) / Convert.ToDecimal(repInOuts.Count())).ToString("P") : "0.00%";
                //ws.Cell(7, 9).Value = repAdds.Count() != 0 ? (Convert.ToDecimal(repAdds.Where(r => r.repflow.Status != "2").Count()) / Convert.ToDecimal(repAdds.Count())).ToString("P") : "0.00%";
                //ws.Cell(7, 10).Value = repIns.Count() != 0 ? (Convert.ToDecimal(repIns.Where(r => r.repflow.Status != "2").Count()) / Convert.ToDecimal(repIns.Count())).ToString("P") : "0.00%";
                //ws.Cell(7, 11).Value = repOuts.Count() != 0 ? (Convert.ToDecimal(repOuts.Where(r => r.repflow.Status != "2").Count()) / Convert.ToDecimal(repOuts.Count())).ToString("P") : "0.00%";
                //ws.Cell(7, 12).Value = repInOuts.Count() != 0 ? (Convert.ToDecimal(repInOuts.Where(r => r.repflow.Status != "2").Count()) / Convert.ToDecimal(repInOuts.Count())).ToString("P") : "0.00%";

                //Title5    【維修且內修完成率(該月申請的內修且已完成的案件)】
                ws.Cell(29, 1).Value = "【維修且內修案件完成率】";
                ws.Cell(30, 1).Value = "3日內";
                ws.Cell(30, 2).Value = "4 - 7日";
                ws.Cell(30, 3).Value = "8日以上";

                var hasEndDateRepIns = repIns.Where(r => r.repdtl.EndDate != null);
                decimal count1 = 0, count2 = 0, count3 = 0;
                if(hasEndDateRepIns.Count() != 0)
                {
                    foreach (var item in hasEndDateRepIns)
                    {
                        //計算時間差(天為單位)
                        var result = new TimeSpan(item.repdtl.EndDate.Value.Ticks - item.repair.ApplyDate.Ticks).Days;
                        if (result <= 3)
                        {
                            count1++;
                        }
                        else if (result > 3 && result < 8)
                        {
                            count2++;
                        }
                        else
                        {
                            count3++;
                        }
                    }
                }

                //Data5
                ws.Cell(31, 1).Value = repIns.Count() != 0 ? (count1 / Convert.ToDecimal(repIns.Count())).ToString("P") : "0.00%";
                ws.Cell(31, 2).Value = repIns.Count() != 0 ? (count2 / Convert.ToDecimal(repIns.Count())).ToString("P") : "0.00%";
                ws.Cell(31, 3).Value = repIns.Count() != 0 ? (count3 / Convert.ToDecimal(repIns.Count())).ToString("P") : "0.00%";
                //Style
                ws.Row(31).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                //Title6    【維修且外修+內外修完成率(該月申請的外修、內外修且已完成的案件)】
                ws.Cell(33, 1).Value = "【維修且外修、內外修案件完成率】";
                ws.Cell(34, 1).Value = "15日內";
                ws.Cell(34, 2).Value = "16 - 30日";
                ws.Cell(34, 3).Value = "31日以上";

                //聯集外修及內外修案件
                var unionReps = repOuts.Union(repInOuts).ToList();
                var hasEndDateReps = unionReps.Where(r => r.repdtl.EndDate != null).ToList();
                count1 = 0; count2 = 0; count3 = 0;
                if (hasEndDateReps.Count() != 0)
                {
                    foreach (var item in hasEndDateReps)
                    {
                        //計算時間差(天為單位)
                        var result = new TimeSpan(item.repdtl.EndDate.Value.Ticks - item.repair.ApplyDate.Ticks).Days;
                        if (result <= 15)
                        {
                            count1++;
                        }
                        else if (result > 15 && result < 31)
                        {
                            count2++;
                        }
                        else
                        {
                            count3++;
                        }
                    }
                }

                //Data6
                ws.Cell(35, 1).Value = unionReps.Count() != 0 ? (count1 / Convert.ToDecimal(unionReps.Count())).ToString("P") : "0.00%";
                ws.Cell(35, 2).Value = unionReps.Count() != 0 ? (count2 / Convert.ToDecimal(unionReps.Count())).ToString("P") : "0.00%";
                ws.Cell(35, 3).Value = unionReps.Count() != 0 ? (count3 / Convert.ToDecimal(unionReps.Count())).ToString("P") : "0.00%";
                //Style
                ws.Row(35).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                //Title7    【增設案件】
                ws.Cell(37, 1).Value = "【增設案件完成率】";
                ws.Cell(38, 1).Value = "15日內";
                ws.Cell(38, 2).Value = "16 - 30日";
                ws.Cell(38, 3).Value = "31日以上";

                var hasEndDateRepAdds = repAdds.Where(r => r.repdtl.EndDate != null);
                count1 = 0; count2 = 0; count3 = 0;
                if (hasEndDateRepAdds.Count() != 0)
                {
                    foreach (var item in hasEndDateRepAdds)
                    {
                        //計算時間差(天為單位)
                        var result = new TimeSpan(item.repdtl.EndDate.Value.Ticks - item.repair.ApplyDate.Ticks).Days;
                        if (result <= 15)
                        {
                            count1++;
                        }
                        else if (result > 15 && result < 31)
                        {
                            count2++;
                        }
                        else
                        {
                            count3++;
                        }
                    }
                }              

                //Data7
                ws.Cell(39, 1).Value = repAdds.Count() != 0 ? (count1 / Convert.ToDecimal(repAdds.Count())).ToString("P") : "0.00%";
                ws.Cell(39, 2).Value = repAdds.Count() != 0 ? (count2 / Convert.ToDecimal(repAdds.Count())).ToString("P") : "0.00%";
                ws.Cell(39, 3).Value = repAdds.Count() != 0 ? (count3 / Convert.ToDecimal(repAdds.Count())).ToString("P") : "0.00%";
                //Style
                ws.Row(39).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;


                //Title8    【增設案件累進完成率(累積至當月的所有完工案件 / 累積至當月的所有案件)】
                ws.Cell(41, 1).Value = "【增設案件累進完成率】";
                ws.Cell(43, 1).Value = "15日內";
                ws.Cell(44, 1).Value = "16~30日";
                ws.Cell(45, 1).Value = "31日以上";

                //年度增設的案件
                var yearEndDateRepAdds = qtyYearRepairs2.Where(r => r.repair.RepType == "增設").ToList();
                for(int month = 1; month <= 12; month++)
                {
                    //從1月~N月的增設案件
                    var repAddsInMonths = yearEndDateRepAdds.Where(r => r.repair.ApplyDate.Month >= 1)
                                                            .Where(r => r.repair.ApplyDate.Month <= month).ToList();
                    //已完工的案件
                    var progressiveRepAdds = repAddsInMonths.Where(r => r.repdtl.EndDate != null).ToList();

                    count1 = 0; count2 = 0; count3 = 0;
                    if (progressiveRepAdds.Count() != 0)
                    {
                        foreach (var item in progressiveRepAdds)
                        {
                            //計算時間差(天為單位)
                            var result = new TimeSpan(item.repdtl.EndDate.Value.Ticks - item.repair.ApplyDate.Ticks).Days;
                            if (result <= 15)
                            {
                                count1++;
                            }
                            else if (result > 15 && result < 31)
                            {
                                count2++;
                            }
                            else
                            {
                                count3++;
                            }
                        }
                    }

                    ws.Cell(42, month + 1).SetValue(month + "月");
                    //Data8
                    ws.Cell(43, month + 1).Value = repAddsInMonths.Count() != 0 ? (count1 / Convert.ToDecimal(repAddsInMonths.Count())).ToString("P") : "0.00%";
                    ws.Cell(44, month + 1).Value = repAddsInMonths.Count() != 0 ? (count2 / Convert.ToDecimal(repAddsInMonths.Count())).ToString("P") : "0.00%";
                    ws.Cell(45, month + 1).Value = repAddsInMonths.Count() != 0 ? (count3 / Convert.ToDecimal(repAddsInMonths.Count())).ToString("P") : "0.00%";
                    //Style
                    ws.Cell(43, month + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(44, month + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(45, month + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                }

                //Title9    【有費用及無費用案件數(已完工案件)】
                ws.Cell(47, 1).Value = "【有費用及無費用案件數(已完工案件，報廢案件除外)】";
                ws.Cell(48, 1).Value = "有費用";
                ws.Cell(48, 2).Value = "無費用";

                //已有完工日之有、無費用的案件(無計算報廢案件)
                var hasCostReps = qtyRepairs2.Where(r => r.repdtl.IsCharged == "Y" && r.repdtl.EndDate != null)
                                             .Where(r => r.repdtl.DealState != 4);
                var noCostReps = qtyRepairs2.Where(r => r.repdtl.IsCharged == "N" && r.repdtl.EndDate != null)
                                             .Where(r => r.repdtl.DealState != 4);;

                //Data9
                ws.Cell(49, 1).Value = hasCostReps.Count();
                ws.Cell(49, 2).Value = noCostReps.Count();
                //Style
                ws.Row(49).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                //Title6    【有費用件數(已完工案件)(總費用/有費用件數)】
                ws.Cell(51, 1).Value = "【有費用件數(已完工案件)】";
                ws.Cell(52, 1).Value = "總費用";
                ws.Cell(52, 2).Value = "平均每件維修費用";

                //Data6
                int costReps = hasCostReps.Count();
                decimal totalCosts = hasCostReps.Select(rd => rd.repdtl.Cost).DefaultIfEmpty(0).Sum();
                decimal avgCosts = costReps != 0 ? totalCosts / costReps : 0;
                ws.Cell(53, 1).Value = String.Format("{0:N0}", totalCosts); 
                ws.Cell(53, 2).Value = String.Format("{0:N0}", avgCosts);
                //Style
                ws.Row(53).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                //WorkSheet2
                var ws2 = workbook.Worksheets.Add("個人月指標", 2);
                //Style
                ws2.ColumnWidth = 15;
                ws2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws2.Column(1).Width = 18;
                ws2.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws2.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws2.Columns(2, 11).Width = 11;
                ws2.Columns(15, 17).Width = 20;
                ws2.Columns(18, 20).Width = 20;
                ws2.Row(1).Style.Alignment.WrapText = true;
                ws2.Columns(12, 13).Hide();

                //Title
                ws2.Cell(1, 1).Value = "姓名";
                ws2.Cell(1, 2).Value = "增設件數\n(內修)";
                ws2.Cell(1, 3).Value = "增設件數\n(外修)";
                ws2.Cell(1, 4).Value = "增設件數\n(內外修)";
                ws2.Cell(1, 5).Value = "增設件數\n(未處理)";
                ws2.Cell(1, 6).Value = "維修件數\n(內修)";
                ws2.Cell(1, 7).Value = "維修件數\n(外修)";
                ws2.Cell(1, 8).Value = "維修件數\n(內外修)";
                ws2.Cell(1, 9).Value = "維修件數\n(未處理)";
                ws2.Cell(1, 10).Value = "報廢件數";
                ws2.Cell(1, 11).Value = "總件數";
                ws2.Cell(1, 12).Value = "平均每人件數";
                ws2.Cell(1, 13).Value = "維修完工率";
                ws2.Cell(1, 14).Value = "維修(內修)\n3日內完成率";
                ws2.Cell(1, 15).Value = "維修(內修)\n4 - 7日完成率";
                ws2.Cell(1, 16).Value = "維修(內修)\n8日以上完成率";
                ws2.Cell(1, 17).Value = "維修(外修、內外修)\n15日內完成率";
                ws2.Cell(1, 18).Value = "維修(外修、內外修)\n16 - 30日完成率";
                ws2.Cell(1, 19).Value = "維修(外修、內外修)\n31日以上完成率";
                ws2.Cell(1, 20).Value = "增設\n15日內完成率";
                ws2.Cell(1, 21).Value = "增設\n16 - 30日完成率";
                ws2.Cell(1, 22).Value = "增設\n31日以上完成率";
                ws2.Cell(1, 23).Value = "有費用件數";
                ws2.Cell(1, 24).Value = "無費用件數";
                ws2.Cell(1, 25).Value = "總費用";
                ws2.Cell(1, 26).Value = "平均每件\n維修費用";

                //Data 整理及統計

                // Get all engineers.
                var engs = roleManager.GetUsersInRole("RepEngineer").Where(m => m != "344027").ToList();
                List<RepairReportListVModel> rvData = new List<RepairReportListVModel>();
                AppUserModel ur;

                //移除非工務部工程師
                var engsTemp = engs.ToList();
                foreach (string l in engsTemp)
                {
                    ur = _context.AppUsers.Where(u => u.UserName == l).FirstOrDefault();
                    if (ur != null)
                    {
                        if (qtyDptId == "0000") 
                        {
                            if (ur.FullName.Contains("外包") != true)  //篩選外包人員
                            {
                                engs.Remove(l);
                            }
                        }
                        else
                        {
                            if (ur.FullName.Contains("外包") == true)  
                            {
                                engs.Remove(l);
                            }
                            if (!(ur.DptId == "8410" || ur.DptId == "8411" || ur.DptId == "8412" ||
                                  ur.DptId == "8413" || ur.DptId == "8414"))
                            {
                                engs.Remove(l);
                            }
                        }
                    }
                }

                // 根據搜尋部門篩選工程師
                if (qtyDptId != null && qtyDptId != "0000")
                {
                    var engsTemp2 = engs.ToList();
                    foreach (string l in engsTemp2)
                    {
                        ur = _context.AppUsers.Where(u => u.UserName == l).FirstOrDefault();
                        if (ur != null)
                        {
                            if(ur.DptId != qtyDptId)
                            {
                                engs.Remove(l);
                            }
                        }
                    }
                }

                foreach (string l in engs)
                {
                    ur = _context.AppUsers.Where(u => u.UserName == l).FirstOrDefault();
                    if (ur != null)
                    {
                        // 個人總案件數
                        var qtyEngRepairs = qtyRepairs.Where(r => r.repair.EngId == ur.Id);
                        // 各案件總數
                        var engRepAdds = qtyEngRepairs.Where(r => r.repair.RepType == "增設");
                        var engRepAddIns = qtyEngRepairs.Where(r => r.repair.RepType == "增設" && r.repdtl.InOut == "內修");
                        var engRepAddOuts = qtyEngRepairs.Where(r => r.repair.RepType == "增設" && r.repdtl.InOut == "外修");
                        var engRepAddInOuts = qtyEngRepairs.Where(r => r.repair.RepType == "增設" && r.repdtl.InOut == "內外修");
                        var engRepAddNoDeals = qtyEngRepairs.Where(r => r.repair.RepType == "增設" && r.repdtl.InOut == null);
                        var engRepIns = qtyEngRepairs.Where(r => r.repair.RepType != "增設" && r.repdtl.InOut == "內修" && r.repdtl.DealState != 4);
                        var engRepOuts = qtyEngRepairs.Where(r => r.repair.RepType != "增設" && r.repdtl.InOut == "外修" && r.repdtl.DealState != 4);
                        var engRepInOuts = qtyEngRepairs.Where(r => r.repair.RepType != "增設" && r.repdtl.InOut == "內外修" && r.repdtl.DealState != 4);
                        var engRepNoDeals = qtyEngRepairs.Where(r => r.repair.RepType != "增設" && r.repdtl.InOut == null && r.repdtl.DealState != 4);
                        var engRepScraps = qtyEngRepairs.Where(r => r.repair.RepType != "增設" && r.repdtl.DealState == 4);
                        var engRepOutUnion = engRepOuts.Union(engRepInOuts);

                        // 總花費 & 平均 (已完工案件)(非報廢案件)
                        int engCostRepairs = qtyEngRepairs.Where(r => r.repdtl.EndDate != null && r.repdtl.DealState != 4)
                                                          .Where(r => r.repdtl.IsCharged == "Y").Count();
                        decimal engTotalCosts = qtyEngRepairs.Where(r => r.repdtl.EndDate != null && r.repdtl.DealState != 4)
                                                             .Where(r => r.repdtl.IsCharged == "Y")
                                                             .Select(rd => rd.repdtl.Cost).DefaultIfEmpty(0).Sum();
                        decimal engAvgCosts = engCostRepairs != 0 ? engTotalCosts / engCostRepairs : 0;

                        // 增設、維修(內修)、維修(外修、內外修) 各完工案件
                        var endEngRepAdds = engRepAdds.Where(r => r.repdtl.EndDate != null);
                        decimal addCount1 = 0, addCount2 = 0, addCount3 = 0;
                        var endEngRepIns = engRepIns.Where(r => r.repdtl.EndDate != null);
                        decimal inCount1 = 0, inCount2 = 0, inCount3 = 0;
                        var endEngRepOuts = engRepOutUnion.Where(r => r.repdtl.EndDate != null);
                        decimal outCount1 = 0, outCount2 = 0, outCount3 = 0;
                        // 維修(內修)日期區間完成率
                        if (endEngRepIns.Count() != 0)
                        {
                            foreach (var item in endEngRepIns)
                            {
                                //計算時間差(天為單位)
                                var result = new TimeSpan(item.repdtl.EndDate.Value.Ticks - item.repair.ApplyDate.Ticks).Days;
                                if (result <= 3)
                                {
                                    inCount1++;
                                }
                                else if (result > 3 && result < 8)
                                {
                                    inCount2++;
                                }
                                else
                                {
                                    inCount3++;
                                }
                            }
                        }
                        // 維修(外修、內外修)日期區間完成率
                        if (endEngRepOuts.Count() != 0)
                        {
                            foreach (var item in endEngRepOuts)
                            {
                                //計算時間差(天為單位)
                                var result = new TimeSpan(item.repdtl.EndDate.Value.Ticks - item.repair.ApplyDate.Ticks).Days;
                                if (result <= 15)
                                {
                                    outCount1++;
                                }
                                else if (result > 15 && result < 31)
                                {
                                    outCount2++;
                                }
                                else
                                {
                                    outCount3++;
                                }
                            }
                        }
                        // 增設日期區間完成率
                        if (endEngRepAdds.Count() != 0)
                        {
                            foreach (var item in endEngRepAdds)
                            {
                                //計算時間差(天為單位)
                                var result = new TimeSpan(item.repdtl.EndDate.Value.Ticks - item.repair.ApplyDate.Ticks).Days;
                                if (result <= 15)
                                {
                                    addCount1++;
                                }
                                else if (result > 15 && result < 31)
                                {
                                    addCount2++;
                                }
                                else
                                {
                                    addCount3++;
                                }
                            }
                        }

                        rvData.Add(new RepairReportListVModel
                        {
                            FullName = ur.FullName + "(" + ur.UserName + ")",
                            RepAddIns = engRepAddIns.Count(),
                            RepAddOuts = engRepAddOuts.Count(),
                            RepAddInOuts = engRepAddInOuts.Count(),
                            RepAddNoDeals = engRepAddNoDeals.Count(),
                            RepIns = engRepIns.Count(),
                            RepOuts = engRepOuts.Count(),
                            RepInOuts = engRepInOuts.Count(),
                            RepNoDeals = engRepNoDeals.Count(),
                            RepScraps = engRepScraps.Count(),
                            RepTotals = qtyEngRepairs.Count(),      
                            RepInEndRate1 = engRepIns.Count() != 0 ? (inCount1 / Convert.ToDecimal(engRepIns.Count())).ToString("P") : "0.00%",
                            RepInEndRate2 = engRepIns.Count() != 0 ? (inCount2 / Convert.ToDecimal(engRepIns.Count())).ToString("P") : "0.00%",
                            RepInEndRate3 = engRepIns.Count() != 0 ? (inCount3 / Convert.ToDecimal(engRepIns.Count())).ToString("P") : "0.00%",
                            RepOutEndRate1 = engRepOutUnion.Count() != 0 ? (outCount1 / Convert.ToDecimal(engRepOutUnion.Count())).ToString("P") : "0.00%",
                            RepOutEndRate2 = engRepOutUnion.Count() != 0 ? (outCount2 / Convert.ToDecimal(engRepOutUnion.Count())).ToString("P") : "0.00%",
                            RepOutEndRate3 = engRepOutUnion.Count() != 0 ? (outCount3 / Convert.ToDecimal(engRepOutUnion.Count())).ToString("P") : "0.00%",
                            RepAddEndRate1 = engRepAdds.Count() != 0 ? (addCount1 / Convert.ToDecimal(engRepAdds.Count())).ToString("P") : "0.00%",
                            RepAddEndRate2 = engRepAdds.Count() != 0 ? (addCount2 / Convert.ToDecimal(engRepAdds.Count())).ToString("P") : "0.00%",
                            RepAddEndRate3 = engRepAdds.Count() != 0 ? (addCount3 / Convert.ToDecimal(engRepAdds.Count())).ToString("P") : "0.00%",
                            IsChargedReps = qtyEngRepairs.Where(r => r.repdtl.EndDate != null && r.repdtl.DealState != 4).Where(r => r.repdtl.IsCharged == "Y").Count(),
                            NoChargedReps = qtyEngRepairs.Where(r => r.repdtl.EndDate != null && r.repdtl.DealState != 4).Where(r => r.repdtl.IsCharged == "N").Count(),
                            TotalCosts = engTotalCosts,
                            AvgCosts = engAvgCosts,
                        });
                    }

                }

                //如果是要塞入Query後的資料該資料一定要變成是data.AsEnumerable()
                ws2.Cell(2, 1).InsertData(rvData.AsEnumerable());

                //WorkSheet3
                var ws3 = workbook.Worksheets.Add("個人各月份統計", 3);
                //Style
                ws3.Column(1).Width = 18;
                ws3.Column(3).Width = 14;

                //Title1  【2019年各月統計】
                ws3.Cell(1, 1).Value = "【" + qtyMonth.Year + "年" + "個人各月份統計】";

                //Data1
                int pStart = 3; // Set Default value.
                foreach (string l in engs)
                {
                    ur = _context.AppUsers.Where(u => u.UserName == l).FirstOrDefault();
                    if (ur != null)
                    {
                        // 個人年度總案件數
                        var engYearRepairs = qtyYearRepairs.Where(r => r.repair.EngId == ur.Id);

                        //Style
                        ws3.Row(pStart).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 204);
                        ws3.Row(pStart).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        ws3.Row(pStart).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws3.Row(pStart).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws3.Row(pStart).Style.Border.TopBorder = XLBorderStyleValues.Thin;

                        //Title
                        ws3.Cell(pStart, 1).Value = ur.FullName + "(" + ur.UserName + ")";
                        ws3.Cell(pStart + 1, 2).Value = "增設";
                        ws3.Cell(pStart + 1, 3).Value = "已完工件數";
                        ws3.Cell(pStart + 2, 3).Value = "未完工件數";
                        ws3.Cell(pStart + 3, 3).Value = "已結案件數";
                        ws3.Cell(pStart + 4, 3).Value = "未結案件數";
                        ws3.Cell(pStart + 5, 3).Value = "完工率";
                        ws3.Cell(pStart + 6, 3).Value = "結案率";
                        ws3.Cell(pStart + 7, 2).Value = "維修";
                        ws3.Cell(pStart + 7, 3).Value = "已完工件數";
                        ws3.Cell(pStart + 8, 3).Value = "未完工件數";
                        ws3.Cell(pStart + 9, 3).Value = "已結案件數";
                        ws3.Cell(pStart + 10, 3).Value = "未結案件數";
                        ws3.Cell(pStart + 11, 3).Value = "完工率";
                        ws3.Cell(pStart + 12, 3).Value = "結案率";

                        for (int month = 1; month <= 12; month++)
                        {
                            var monthRepAdds = engYearRepairs.Where(r => r.repair.ApplyDate.Month == month)
                                                             .Where(r => r.repair.RepType == "增設").ToList();
                            var monthReps = engYearRepairs.Where(r => r.repair.ApplyDate.Month == month)
                                                          .Where(r => r.repair.RepType != "增設" && r.repdtl.DealState != 4).ToList();

                            ws3.Cell(pStart, (month + 3)).SetValue(month + "月");
                            //增設
                            ws3.Cell(pStart + 1, (month + 3)).Value = monthRepAdds.Where(r => r.repdtl.EndDate != null).Count();
                            ws3.Cell(pStart + 2, (month + 3)).Value = monthRepAdds.Where(r => r.repdtl.EndDate == null).Count();
                            ws3.Cell(pStart + 3, (month + 3)).Value = monthRepAdds.Where(r => r.repdtl.CloseDate != null).Count();
                            ws3.Cell(pStart + 4, (month + 3)).Value = monthRepAdds.Where(r => r.repdtl.CloseDate == null).Count();
                            ws3.Cell(pStart + 5, (month + 3)).Value = monthRepAdds.Count() != 0 ? (Convert.ToDecimal(monthRepAdds.Where(r => r.repdtl.EndDate != null).Count()) / Convert.ToDecimal(monthRepAdds.Count())).ToString("P") : "0.00%";
                            ws3.Cell(pStart + 6, (month + 3)).Value = monthRepAdds.Count() != 0 ? (Convert.ToDecimal(monthRepAdds.Where(r => r.repdtl.CloseDate != null).Count()) / Convert.ToDecimal(monthRepAdds.Count())).ToString("P") : "0.00%";
                            //Style
                            ws3.Cell(pStart + 5, (month + 3)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            ws3.Cell(pStart + 6, (month + 3)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            //維修
                            ws3.Cell(pStart + 7, (month + 3)).Value = monthReps.Where(r => r.repdtl.EndDate != null).Count();
                            ws3.Cell(pStart + 8, (month + 3)).Value = monthReps.Where(r => r.repdtl.EndDate == null).Count();
                            ws3.Cell(pStart + 9, (month + 3)).Value = monthReps.Where(r => r.repdtl.CloseDate != null).Count();
                            ws3.Cell(pStart + 10, (month + 3)).Value = monthReps.Where(r => r.repdtl.CloseDate == null).Count();
                            ws3.Cell(pStart + 11, (month + 3)).Value = monthReps.Count() != 0 ? (Convert.ToDecimal(monthReps.Where(r => r.repdtl.EndDate != null).Count()) / Convert.ToDecimal(monthReps.Count())).ToString("P") : "0.00%";
                            ws3.Cell(pStart + 12, (month + 3)).Value = monthReps.Count() != 0 ? (Convert.ToDecimal(monthReps.Where(r => r.repdtl.CloseDate != null).Count()) / Convert.ToDecimal(monthReps.Count())).ToString("P") : "0.00%";
                            //Style
                            ws3.Cell(pStart + 11, (month + 3)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            ws3.Cell(pStart + 12, (month + 3)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
                    }
                    pStart += 13;
                }

                //WorkSheet4
                var ws4 = workbook.Worksheets.Add("各課及部門月指標", 4);
                //Style
                ws4.ColumnWidth = 15;
                ws4.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws4.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws4.Column(1).Width = 24;
                ws4.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws4.Columns(2, 11).Width = 11;
                ws4.Columns(17, 19).Width = 20;
                ws4.Columns(20, 22).Width = 20;
                ws4.Row(1).Style.Alignment.WrapText = true;

                //Title
                ws4.Cell(1, 1).Value = "";
                ws4.Cell(1, 2).Value = "增設件數\n(內修)";
                ws4.Cell(1, 3).Value = "增設件數\n(外修)";
                ws4.Cell(1, 4).Value = "增設件數\n(內外修)";
                ws4.Cell(1, 5).Value = "增設件數\n(未處理)";
                ws4.Cell(1, 6).Value = "維修件數\n(內修)";
                ws4.Cell(1, 7).Value = "維修件數\n(外修)";
                ws4.Cell(1, 8).Value = "維修件數\n(內外修)";
                ws4.Cell(1, 9).Value = "維修件數\n(未處理)";
                ws4.Cell(1, 10).Value = "報廢件數";
                ws4.Cell(1, 11).Value = "總件數";
                ws4.Cell(1, 12).Value = "平均每人件數";
                ws4.Cell(1, 13).Value = "維修完工率";
                ws4.Cell(1, 14).Value = "維修(內修)\n3日內完成率";
                ws4.Cell(1, 15).Value = "維修(內修)\n4 - 7日完成率";
                ws4.Cell(1, 16).Value = "維修(內修)\n8日以上完成率";
                ws4.Cell(1, 17).Value = "維修(外修、內外修)\n15日內完成率";
                ws4.Cell(1, 18).Value = "維修(外修、內外修)\n16 - 30日完成率";
                ws4.Cell(1, 19).Value = "維修(外修、內外修)\n31日以上完成率";
                ws4.Cell(1, 20).Value = "增設\n15日內完成率";
                ws4.Cell(1, 21).Value = "增設\n16 - 30日完成率";
                ws4.Cell(1, 22).Value = "增設\n31日以上完成率";
                ws4.Cell(1, 23).Value = "有費用件數";
                ws4.Cell(1, 24).Value = "無費用件數";
                ws4.Cell(1, 25).Value = "總費用";
                ws4.Cell(1, 26).Value = "平均每件\n維修費用";

                // Data 整理及統計
                // 8410工務部  8411工務一課    8412工務二課    8413 8414 工務三課  0000外包人員
                var qtyRepairs8410 = qtyRepairs.Where(r => r.repair.EngName == "8410").ToList();
                var qtyRepairs8411 = qtyRepairs.Where(r => r.repair.EngName == "8411").ToList();
                var qtyRepairs8412 = qtyRepairs.Where(r => r.repair.EngName == "8412").ToList();
                var qtyRepairs8413 = qtyRepairs.Where(r => r.repair.EngName == "8413").ToList();
                var qtyRepairs8414 = qtyRepairs.Where(r => r.repair.EngName == "8414").ToList();
                var qtyRepairsDpt = qtyRepairs.Where(r => r.repair.EngName == "8410" || r.repair.EngName == "8411" ||
                                                          r.repair.EngName == "8412" || r.repair.EngName == "8413" ||
                                                          r.repair.EngName == "8414").ToList();
                var qtyRepairs0000 = qtyRepairs.Where(r => r.repair.EngName == "0000").ToList();

                // 各課Id list    0000為外包人員
                List<string> dpts = new List<string> { "8410", "8411", "8412", "8413", "8414", "allDpts", "0000" };
                List<RepairReportListVModel> dptData = new List<RepairReportListVModel>();

                foreach (string id in dpts)
                {
                    var dptRepairs = qtyRepairs;    // Default value.
                    if (id == "8410")
                    {
                        dptRepairs = qtyRepairs8410.ToList();
                    }
                    if (id == "8411")
                    {
                        dptRepairs = qtyRepairs8411.ToList();
                    }
                    if (id == "8412")
                    {
                        dptRepairs = qtyRepairs8412.ToList();
                    }
                    if (id == "8413")
                    {
                        dptRepairs = qtyRepairs8413.ToList();
                    }
                    if (id == "8414")
                    {
                        dptRepairs = qtyRepairs8414.ToList();
                    }
                    if (id == "allDpts")
                    {
                        dptRepairs = qtyRepairsDpt.ToList();
                    }
                    if (id == "0000")
                    {
                        dptRepairs = qtyRepairs0000.ToList();
                    }

                    // 各案件總數
                    var dptRepAdds = dptRepairs.Where(r => r.repair.RepType == "增設");
                    var dptRepAddIns = dptRepairs.Where(r => r.repair.RepType == "增設" && r.repdtl.InOut == "內修");
                    var dptRepAddOuts = dptRepairs.Where(r => r.repair.RepType == "增設" && r.repdtl.InOut == "外修");
                    var dptRepAddInOuts = dptRepairs.Where(r => r.repair.RepType == "增設" && r.repdtl.InOut == "內外修");
                    var dptRepAddNoDeals = dptRepairs.Where(r => r.repair.RepType == "增設" && r.repdtl.InOut == null);
                    var dptRepIns = dptRepairs.Where(r => r.repair.RepType != "增設" && r.repdtl.InOut == "內修" && r.repdtl.DealState != 4);
                    var dptRepOuts = dptRepairs.Where(r => r.repair.RepType != "增設" && r.repdtl.InOut == "外修" && r.repdtl.DealState != 4);
                    var dptRepInOuts = dptRepairs.Where(r => r.repair.RepType != "增設" && r.repdtl.InOut == "內外修" && r.repdtl.DealState != 4);
                    var dptRepNoDeals = dptRepairs.Where(r => r.repair.RepType != "增設" && r.repdtl.InOut == null && r.repdtl.DealState != 4);
                    var dptRepScraps = dptRepairs.Where(r => r.repair.RepType != "增設" && r.repdtl.DealState == 4);
                    var dptRepOutUnion = dptRepOuts.Union(dptRepInOuts);
                    var dptReps = dptRepairs.Where(r => r.repair.RepType != "增設" && r.repdtl.DealState != 4);

                    var dptEngs = roleManager.GetUsersInRole("RepEngineer").Where(m => m != "344027").ToList();
                    int countEngs = 0;
                    if (id == "0000")
                    {
                        foreach (string l in dptEngs)
                        {
                            AppUserModel usr = _context.AppUsers.Where(u => u.UserName == l).FirstOrDefault();
                            if (usr != null)
                            {
                                if (usr.FullName.Contains("外包") == true)
                                {
                                    countEngs++;
                                }
                            }
                        }
                    }
                    else if (id == "allDpts")
                    {
                        foreach (string l in dptEngs)
                        {
                            AppUserModel usr = _context.AppUsers.Where(u => u.UserName == l).FirstOrDefault();
                            if (usr != null)
                            {
                                if (usr.FullName.Contains("外包") == false && 
                                    usr.FullName.Contains("值班") == false && usr.DptId != "8430")
                                {
                                    countEngs++;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (string l in dptEngs)
                        {
                            AppUserModel usr = _context.AppUsers.Where(u => u.UserName == l).FirstOrDefault();
                            if (usr != null)
                            {
                                if (usr.DptId == id && usr.FullName.Contains("值班") == false &&
                                    usr.FullName.Contains("外包") == false)
                                {
                                    countEngs++;
                                }
                            }
                        }
                    }

                    // 總花費 & 平均 (已完工)(非報廢案件)
                    int dptCostRepairs = dptRepairs.Where(r => r.repdtl.EndDate != null && r.repdtl.DealState != 4)
                                                   .Where(r => r.repdtl.IsCharged == "Y").Count();
                    decimal dptTotalCosts = dptRepairs.Where(r => r.repdtl.EndDate != null && r.repdtl.DealState != 4)
                                                      .Where(r => r.repdtl.IsCharged == "Y")
                                                      .Select(rd => rd.repdtl.Cost).DefaultIfEmpty(0).Sum();
                    decimal dptAvgCosts = dptCostRepairs != 0 ? dptTotalCosts / dptCostRepairs : 0;

                    var endDptRepAdds = dptRepAdds.Where(r => r.repdtl.EndDate != null);
                    decimal addCount1 = 0, addCount2 = 0, addCount3 = 0;
                    var endDptRepIns = dptRepIns.Where(r => r.repdtl.EndDate != null);
                    decimal inCount1 = 0, inCount2 = 0, inCount3 = 0;
                    var endDptRepOuts = dptRepOutUnion.Where(r => r.repdtl.EndDate != null);
                    decimal outCount1 = 0, outCount2 = 0, outCount3 = 0;
                    // 維修(內修)日期區間完成率
                    if (endDptRepIns.Count() != 0)
                    {
                        foreach (var item in endDptRepIns)
                        {
                            //計算時間差(天為單位)
                            var result = new TimeSpan(item.repdtl.EndDate.Value.Ticks - item.repair.ApplyDate.Ticks).Days;
                            if (result <= 3)
                            {
                                inCount1++;
                            }
                            else if (result > 3 && result < 8)
                            {
                                inCount2++;
                            }
                            else
                            {
                                inCount3++;
                            }
                        }
                    }
                    // 維修(外修、內外修)日期區間完成率
                    if (endDptRepOuts.Count() != 0)
                    {
                        foreach (var item in endDptRepOuts)
                        {
                            //計算時間差(天為單位)
                            var result = new TimeSpan(item.repdtl.EndDate.Value.Ticks - item.repair.ApplyDate.Ticks).Days;
                            if (result <= 15)
                            {
                                outCount1++;
                            }
                            else if (result > 15 && result < 31)
                            {
                                outCount2++;
                            }
                            else
                            {
                                outCount3++;
                            }
                        }
                    }
                    // 增設日期區間完成率
                    if (endDptRepAdds.Count() != 0)
                    {
                        foreach (var item in endDptRepAdds)
                        {
                            //計算時間差(天為單位)
                            var result = new TimeSpan(item.repdtl.EndDate.Value.Ticks - item.repair.ApplyDate.Ticks).Days;
                            if (result <= 15)
                            {
                                addCount1++;
                            }
                            else if (result > 15 && result < 31)
                            {
                                addCount2++;
                            }
                            else
                            {
                                addCount3++;
                            }
                        }
                    }

                    string dptName;
                    if (id == "allDpts")
                    {
                        dptName = "工務部(8410 - 8414)";
                    }
                    else if (id == "0000")
                    {
                        dptName = "外包人員";
                    }
                    else
                    {
                        dptName = _context.Departments.Where(d => d.DptId == id).FirstOrDefault().Name_C;
                    }

                    dptData.Add(new RepairReportListVModel
                    {
                        FullName = dptName,
                        RepAddIns = dptRepAddIns.Count(),
                        RepAddOuts = dptRepAddOuts.Count(),
                        RepAddInOuts = dptRepAddInOuts.Count(),
                        RepAddNoDeals = dptRepAddNoDeals.Count(),
                        RepIns = dptRepIns.Count(),
                        RepOuts = dptRepOuts.Count(),
                        RepInOuts = dptRepInOuts.Count(),
                        RepNoDeals = dptRepNoDeals.Count(),
                        RepScraps = dptRepScraps.Count(),
                        RepTotals = dptRepairs.Count(),
                        RepAvgPerPerson = Math.Round(Convert.ToDecimal(dptRepairs.Count()) / countEngs, 2),
                        RepEndRate = dptReps.Count() != 0 ? (dptReps.Where(r => r.repdtl.EndDate != null).Count() / Convert.ToDecimal(dptReps.Count())).ToString("P") : "0.00%",
                        RepInEndRate1 = dptRepIns.Count() != 0 ? (inCount1 / Convert.ToDecimal(dptRepIns.Count())).ToString("P") : "0.00%",
                        RepInEndRate2 = dptRepIns.Count() != 0 ? (inCount2 / Convert.ToDecimal(dptRepIns.Count())).ToString("P") : "0.00%",
                        RepInEndRate3 = dptRepIns.Count() != 0 ? (inCount3 / Convert.ToDecimal(dptRepIns.Count())).ToString("P") : "0.00%",
                        RepOutEndRate1 = dptRepOutUnion.Count() != 0 ? (outCount1 / Convert.ToDecimal(dptRepOutUnion.Count())).ToString("P") : "0.00%",
                        RepOutEndRate2 = dptRepOutUnion.Count() != 0 ? (outCount2 / Convert.ToDecimal(dptRepOutUnion.Count())).ToString("P") : "0.00%",
                        RepOutEndRate3 = dptRepOutUnion.Count() != 0 ? (outCount3 / Convert.ToDecimal(dptRepOutUnion.Count())).ToString("P") : "0.00%",
                        RepAddEndRate1 = dptRepAdds.Count() != 0 ? (addCount1 / Convert.ToDecimal(dptRepAdds.Count())).ToString("P") : "0.00%",
                        RepAddEndRate2 = dptRepAdds.Count() != 0 ? (addCount2 / Convert.ToDecimal(dptRepAdds.Count())).ToString("P") : "0.00%",
                        RepAddEndRate3 = dptRepAdds.Count() != 0 ? (addCount3 / Convert.ToDecimal(dptRepAdds.Count())).ToString("P") : "0.00%",
                        IsChargedReps = dptRepairs.Where(r => r.repdtl.EndDate != null && r.repdtl.DealState != 4).Where(r => r.repdtl.IsCharged == "Y").Count(),
                        NoChargedReps = dptRepairs.Where(r => r.repdtl.EndDate != null && r.repdtl.DealState != 4).Where(r => r.repdtl.IsCharged == "N").Count(),
                        TotalCosts = dptTotalCosts,
                        AvgCosts = dptAvgCosts,
                    });
                }

                // Insert data
                ws4.Cell(2, 1).InsertData(dptData);

                //WorkSheet5
                var ws5 = workbook.Worksheets.Add("各課及部門各月份統計", 5);
                //Style
                ws5.Column(1).Width = 24;
                ws5.Column(3).Width = 14;

                //Title1  【2019年各課及部門各月統計】
                ws5.Cell(1, 1).Value = "【" + qtyMonth.Year + "年" + "各課及部門各月統計】";

                //Data1
                int printStart = 3;
                foreach (string id in dpts)
                {
                    var dptRepairs = qtyYearRepairs;
                    if (id == "8410")
                    {
                        dptRepairs = qtyYearRepairs.Where(r => r.repair.EngName == "8410").ToList();
                        printStart = 3;
                        ws5.Cell(printStart, 1).Value = _context.Departments.Where(d => d.DptId == id).FirstOrDefault().Name_C;
                    }
                    if (id == "8411")
                    {
                        dptRepairs = qtyYearRepairs.Where(r => r.repair.EngName == "8411").ToList();
                        printStart += 13;
                        ws5.Cell(printStart, 1).Value = _context.Departments.Where(d => d.DptId == id).FirstOrDefault().Name_C;
                    }
                    if (id == "8412")
                    {
                        dptRepairs = qtyYearRepairs.Where(r => r.repair.EngName == "8412").ToList();
                        printStart += 13;
                        ws5.Cell(printStart, 1).Value = _context.Departments.Where(d => d.DptId == id).FirstOrDefault().Name_C;
                    }
                    if (id == "8413")
                    {
                        dptRepairs = qtyYearRepairs.Where(r => r.repair.EngName == "8413").ToList();
                        printStart += 13;
                        ws5.Cell(printStart, 1).Value = _context.Departments.Where(d => d.DptId == id).FirstOrDefault().Name_C;
                    }
                    if (id == "8414")
                    {
                        dptRepairs = qtyYearRepairs.Where(r => r.repair.EngName == "8414").ToList();
                        printStart += 13;
                        ws5.Cell(printStart, 1).Value = _context.Departments.Where(d => d.DptId == id).FirstOrDefault().Name_C;
                    }
                    if (id == "allDpts")
                    {
                        dptRepairs = qtyYearRepairs.Where(r => r.repair.EngName == "8410" || r.repair.EngName == "8411" ||
                                                               r.repair.EngName == "8412" || r.repair.EngName == "8413" ||
                                                               r.repair.EngName == "8414").ToList();
                        printStart += 13;
                        ws5.Cell(printStart, 1).Value = "工務部(8410 - 8414)";
                    }
                    if (id == "0000")
                    {
                        dptRepairs = qtyYearRepairs.Where(r => r.repair.EngName == "0000").ToList();
                        printStart += 13;
                        ws5.Cell(printStart, 1).Value = "外包人員";
                    }

                    //Style
                    ws5.Row(printStart).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 204);
                    ws5.Row(printStart).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    ws5.Row(printStart).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws5.Row(printStart).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws5.Row(printStart).Style.Border.TopBorder = XLBorderStyleValues.Thin;

                    //Title
                    ws5.Cell(printStart + 1, 2).Value = "增設";
                    ws5.Cell(printStart + 1, 3).Value = "已完工件數";
                    ws5.Cell(printStart + 2, 3).Value = "未完工件數";
                    ws5.Cell(printStart + 3, 3).Value = "已結案件數";
                    ws5.Cell(printStart + 4, 3).Value = "未結案件數";
                    ws5.Cell(printStart + 5, 3).Value = "完工率";
                    ws5.Cell(printStart + 6, 3).Value = "結案率";
                    ws5.Cell(printStart + 7, 2).Value = "維修";
                    ws5.Cell(printStart + 7, 3).Value = "已完工件數";
                    ws5.Cell(printStart + 8, 3).Value = "未完工件數";
                    ws5.Cell(printStart + 9, 3).Value = "已結案件數";
                    ws5.Cell(printStart + 10, 3).Value = "未結案件數";
                    ws5.Cell(printStart + 11, 3).Value = "完工率";
                    ws5.Cell(printStart + 12, 3).Value = "結案率";

                    for (int month = 1; month <= 12; month++)
                    {
                        var monthRepAdds = dptRepairs.Where(r => r.repair.ApplyDate.Month == month)
                                                     .Where(r => r.repair.RepType == "增設").ToList();
                        var monthReps = dptRepairs.Where(r => r.repair.ApplyDate.Month == month)
                                                  .Where(r => r.repair.RepType != "增設" && r.repdtl.DealState != 4).ToList();

                        ws5.Cell(printStart, (month + 3)).SetValue(month + "月");
                        //增設
                        ws5.Cell(printStart + 1, (month + 3)).Value = monthRepAdds.Where(r => r.repdtl.EndDate != null).Count();
                        ws5.Cell(printStart + 2, (month + 3)).Value = monthRepAdds.Where(r => r.repdtl.EndDate == null).Count();
                        ws5.Cell(printStart + 3, (month + 3)).Value = monthRepAdds.Where(r => r.repdtl.CloseDate != null).Count();
                        ws5.Cell(printStart + 4, (month + 3)).Value = monthRepAdds.Where(r => r.repdtl.CloseDate == null).Count();
                        ws5.Cell(printStart + 5, (month + 3)).Value = monthRepAdds.Count() != 0 ? (Convert.ToDecimal(monthRepAdds.Where(r => r.repdtl.EndDate != null).Count()) / Convert.ToDecimal(monthRepAdds.Count())).ToString("P") : "0.00%";
                        ws5.Cell(printStart + 6, (month + 3)).Value = monthRepAdds.Count() != 0 ? (Convert.ToDecimal(monthRepAdds.Where(r => r.repdtl.CloseDate != null).Count()) / Convert.ToDecimal(monthRepAdds.Count())).ToString("P") : "0.00%";
                        //Style
                        ws5.Cell(printStart + 5, (month + 3)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        ws5.Cell(printStart + 6, (month + 3)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        //維修
                        ws5.Cell(printStart + 7, (month + 3)).Value = monthReps.Where(r => r.repdtl.EndDate != null).Count();
                        ws5.Cell(printStart + 8, (month + 3)).Value = monthReps.Where(r => r.repdtl.EndDate == null).Count();
                        ws5.Cell(printStart + 9, (month + 3)).Value = monthReps.Where(r => r.repdtl.CloseDate != null).Count();
                        ws5.Cell(printStart + 10, (month + 3)).Value = monthReps.Where(r => r.repdtl.CloseDate == null).Count();
                        ws5.Cell(printStart + 11, (month + 3)).Value = monthReps.Count() != 0 ? (Convert.ToDecimal(monthReps.Where(r => r.repdtl.EndDate != null).Count()) / Convert.ToDecimal(monthReps.Count())).ToString("P") : "0.00%";
                        ws5.Cell(printStart + 12, (month + 3)).Value = monthReps.Count() != 0 ? (Convert.ToDecimal(monthReps.Where(r => r.repdtl.CloseDate != null).Count()) / Convert.ToDecimal(monthReps.Count())).ToString("P") : "0.00%";
                        //Style
                        ws5.Cell(printStart + 11, (month + 3)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        ws5.Cell(printStart + 12, (month + 3)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }
                }

                //WorkSheet6
                var ws6 = workbook.Worksheets.Add("各指標計算公式", 6);
                //Style
                ws6.Column(1).Width = 52;
                ws6.Column(2).Width = 120;

                //Data6
                //計算公式
                ws6.Cell(1, 1).Value = "【各指標計算公式】";
                ws6.Cell(3, 1).Value = "【完工率】";
                ws6.Cell(3, 2).Value = "【案件為當月申請且已完工之(增設/維修)件數 / 當月申請(增設/維修)總件數】";
                ws6.Cell(4, 1).Value = "【結案率】";
                ws6.Cell(4, 2).Value = "【案件為當月申請且已結案之(增設/維修)件數 / 當月申請(增設/維修)總件數】";
                ws6.Cell(5, 1).Value = "【維修且內修案件完成率】";
                ws6.Cell(5, 2).Value = "【案件為當月申請且於N日內完成的內修件數 / 當月申請內修之總件數】";
                ws6.Cell(6, 1).Value = "【維修且外修、內外修案件完成率】";
                ws6.Cell(6, 2).Value = "【案件為當月申請且於N日內完成的(外修 + 內外修)總件數 / 當月申請(外修 + 內外修)之總件數】";
                ws6.Cell(7, 1).Value = "【增設案件完成率】";
                ws6.Cell(7, 2).Value = "【案件為當月申請且於N日內完成的增設件數 / 當月申請增設之總件數】";
                ws6.Cell(7, 1).Value = "【增設案件累進完成率】";
                ws6.Cell(7, 2).Value = "【案件為當年度1月 ~ N月前且於N日內完成之增設案件總數 / 當年度1月 ~ N月前申請增設之總件數】";
                ws6.Cell(8, 1).Value = "【平均每件維修費用(已完工案件，報廢案件除外)】";
                ws6.Cell(8, 2).Value = "【總費用 / 有費用件數】";

                //因為是用Query的方式,這個地方要用串流的方式來存檔
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    //請注意 一定要加入這行,不然Excel會是空檔
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    //注意Excel的ContentType,是要用這個"application/vnd.ms-excel"
                    string fileName = "月報表_" + qtyMonth.ToString("yyyy-MM") + ".xlsx";
                    return this.File(memoryStream.ToArray(), "application/vnd.ms-excel", fileName);
                }
            }
        }

        // GET: /RepairReport/EndDateReport
        public IActionResult EndDateReport(DateTime QtyMonth)
        {
            DateTime reportMonth = QtyMonth;
            int qryYear = reportMonth.Year;
            int qryMonth = reportMonth.Month;

            //完工日為當月的所有案件
            var repairDtls = _context.RepairDtls.Where(rd => rd.EndDate.Value.Year == qryYear && 
                                                             rd.EndDate.Value.Month == qryMonth).ToList();
            var qtyResults = _context.Repairs.Join(repairDtls, r => r.DocId, d => d.DocId,
                                             (r, d) => new
                                             {
                                                 repair = r,
                                                 repdtl = d
                                             }).Join(_context.RepairEmps, r => r.repair.DocId, emp => emp.DocId,
                                             (r, emp) => new
                                             {
                                                 repair = r.repair,
                                                 repdtl = r.repdtl,
                                                 repemp = emp
                                             }).Join(_context.AppUsers, r => r.repemp.UserId, u => u.Id,
                                             (r, u) => new
                                             {
                                                 repair = r.repair,
                                                 repdtl = r.repdtl,
                                                 repemp = r.repemp,
                                                 appuser = u
                                             }).Join(_context.Departments, r => r.repair.DptId, d => d.DptId,
                                             (r, d) => new
                                             {
                                                 repair = r.repair,
                                                 repdtl = r.repdtl,
                                                 repemp = r.repemp,
                                                 appuser = r.appuser,
                                                 dpt = d
                                             }).Join(_context.Departments, r => r.repair.AccDpt, d => d.DptId,
                                             (r, acc) => new
                                             {
                                                 repair = r.repair,
                                                 repdtl = r.repdtl,
                                                 repemp = r.repemp,
                                                 appuser = r.appuser,
                                                 dpt = r.dpt,
                                                 accdpt = acc
                                             }).Join(_context.Buildings, r => Convert.ToInt32(r.repair.Building), b => b.BuildingId,
                                             (r, b) => new
                                             {
                                                 repair = r.repair,
                                                 repdtl = r.repdtl,
                                                 repemp = r.repemp,
                                                 appuser = r.appuser,
                                                 dpt = r.dpt,
                                                 accdpt = r.accdpt,
                                                 building = b
                                             }).Join(_context.Floors, r => new { BuildingId =  Convert.ToInt32(r.repair.Building), FloorId = r.repair.Floor },
                                                                      f => new { f.BuildingId, f.FloorId },
                                             (r, f) => new
                                             {
                                                 repair = r.repair,
                                                 repdtl = r.repdtl,
                                                 repemp = r.repemp,
                                                 appuser = r.appuser,
                                                 dpt = r.dpt,
                                                 accdpt = r.accdpt,
                                                 building = r.building,
                                                 floor = f
                                             }).Join(_context.Places, r => new { BuildingId = Convert.ToInt32(r.repair.Building), FloorId = r.repair.Floor, PlaceId = r.repair.Area },
                                                                      p => new { p.BuildingId, p.FloorId, p.PlaceId },
                                             (r, p) => new
                                             {
                                                 //repair = r.repair,
                                                 //repdtl = r.repdtl,
                                                 //repemp = r.repemp,
                                                 //appuser = r.appuser,
                                                 //dpt = r.dpt,
                                                 //accdpt = r.accdpt,
                                                 //building = r.building,
                                                 //floor = r.floor,
                                                 //place = p,
                                                 DocId = r.repair.DocId,
                                                 ApplyDate = r.repair.ApplyDate,
                                                 RepType = r.repair.RepType,
                                                 UserName = r.repair.UserName,
                                                 DptId = r.dpt.Name_C,
                                                 AssetNo = r.repair.AssetNo,
                                                 AssetName = r.repair.AssetName,
                                                 TroubleDes = r.repair.TroubleDes,
                                                 AccDpt = r.repair.AccDpt,
                                                 AccDptName = r.accdpt.Name_C,
                                                 Building = r.building.BuildingName,
                                                 Floor = r.floor.FloorName,
                                                 Area = p.PlaceName,
                                                 EndDate = r.repdtl.EndDate,
                                                 Hour = r.repdtl.Hour,
                                                 EngId = r.appuser.UserName,
                                                 EngName = r.appuser.FullName,
                                                 EngDpt = r.appuser.DptId,
                                             }).ToList();

            //ClosedXML的用法 先new一個Excel Workbook
            using (XLWorkbook workbook = new XLWorkbook())
            {

                //一個workbook內至少會有一個worksheet,並將資料Insert至這個位於A1這個位置上
                //WorkSheet1
                var ws = workbook.Worksheets.Add("請修完工日資料", 1);
                //Style
                ws.ColumnWidth = 15;
                ws.Column(5).Width = 25;
                ws.Column(6).Width = 50;
                ws.Column(8).Width = 200;
                ws.Columns(10, 13).Width = 25;
                ws.Column(12).Width = 10;

                //Title1  【2019年各月統計】
                ws.Cell(1, 1).Value = "表單編號";
                ws.Cell(1, 2).Value = "申請日期";
                ws.Cell(1, 3).Value = "請修類別";
                ws.Cell(1, 4).Value = "申請人";
                ws.Cell(1, 5).Value = "申請部門";
                ws.Cell(1, 6).Value = "財產編號";
                ws.Cell(1, 7).Value = "設備名稱";
                ws.Cell(1, 8).Value = "故障原因";
                ws.Cell(1, 9).Value = "成本中心";
                ws.Cell(1, 10).Value = "成本中心名稱";
                ws.Cell(1, 11).Value = "期別";
                ws.Cell(1, 12).Value = "樓層";
                ws.Cell(1, 13).Value = "區域地點";
                ws.Cell(1, 14).Value = "完工日";
                ws.Cell(1, 15).Value = "工時";
                ws.Cell(1, 16).Value = "工程師代號";
                ws.Cell(1, 17).Value = "工程師名稱";
                ws.Cell(1, 18).Value = "工程師部門";


                //Data1
                //如果是要塞入Query後的資料該資料一定要變成是data.AsEnumerable()
                ws.Cell(2, 1).InsertData(qtyResults.AsEnumerable());

                //因為是用Query的方式,這個地方要用串流的方式來存檔
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    //請注意 一定要加入這行,不然Excel會是空檔
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    //注意Excel的ContentType,是要用這個"application/vnd.ms-excel"
                    string fileName = "月報表_" + QtyMonth.ToString("yyyy-MM") + "月請修完工日資料" + ".xlsx";
                    return this.File(memoryStream.ToArray(), "application/vnd.ms-excel", fileName);
                }
            }
        }
    }
}
