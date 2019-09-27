using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Areas.BMED.Data;
using EDIS.Areas.BMED.Models.KeepModels;
using EDIS.Areas.BMED.Models.RepairModels;
using EDIS.Models;
using EDIS.Models.Identity;
using EDIS.Repositories;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Areas.BMED.Controllers
{
    [Area("BMED")]
    [Authorize]
    public class KeepController : Controller
    {
        private readonly BMEDDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly IRepository<DepartmentModel, string> _dptRepo;
        private readonly IRepository<DocIdStore, string[]> _dsRepo;
        private readonly IEmailSender _emailSender;
        private readonly CustomUserManager userManager;
        private readonly CustomRoleManager roleManager;

        public KeepController(BMEDDbContext context,
                              IRepository<AppUserModel, int> userRepo,
                              IRepository<DepartmentModel, string> dptRepo,
                              IRepository<DocIdStore, string[]> dsRepo,
                              IEmailSender emailSender,
                              CustomUserManager customUserManager,
                              CustomRoleManager customRoleManager)
        {
            _context = context;
            _userRepo = userRepo;
            _dptRepo = dptRepo;
            _dsRepo = dsRepo;
            _emailSender = emailSender;
            userManager = customUserManager;
            roleManager = customRoleManager;
        }

        // GET: BMED/Keep/Create
        public IActionResult Create()
        {
            KeepModel r = new KeepModel();
            AppUserModel ur = _context.AppUsers.Where(u => u.UserName == this.User.Identity.Name).FirstOrDefault();

            r.Email = ur.Email == null ? "" : ur.Email;
            DepartmentModel d = _context.Departments.Find(ur.DptId);
            r.DocId = GetID();
            r.UserId = ur.Id;
            r.UserName = ur.FullName;
            r.UserAccount = ur.UserName;
            r.SentDate = DateTime.Now;
            r.DptId = d == null ? "" : d.DptId;
            r.Company = d == null ? "" : d.Name_C;
            r.AccDpt = d == null ? "" : d.DptId;
            r.AccDptName = d == null ? "" : d.Name_C;
            r.Ext = ur.Ext == null ? "" : ur.Ext;
            r.CheckerId = ur.Id;
            //
            _context.BMEDKeeps.Add(r);
            _context.SaveChanges();

            List<SelectListItem> listItem = new List<SelectListItem>();
            List<SelectListItem> AccDpt = new List<SelectListItem>();
            _context.Departments.ToList()
                .ForEach(dp =>
                {
                    AccDpt.Add(new SelectListItem
                    {
                        Value = dp.DptId,
                        Text = dp.Name_C,
                        Selected = false
                    });
                });
            ViewData["AccDpt"] = AccDpt;

            //
            List<AssetModel> alist = null;
            if (ur.DptId != null)
                alist = _context.BMEDAssets.Where(at => at.AccDpt == ur.DptId)
                                           .Where(at => at.DisposeKind != "報廢").ToList();
            else if (ur.VendorId > 0)
            {
                string s = Convert.ToString(ur.VendorId);
                alist = _context.BMEDAssets.Where(at => at.AccDpt == s)
                                           .Where(at => at.DisposeKind != "報廢").ToList();
            }

            /* 擷取該使用者單位底下所有人員 */
            var dptUsers = _context.AppUsers.Where(a => a.DptId == ur.DptId).ToList();
            List<SelectListItem> dptMemberList = new List<SelectListItem>();
            foreach (var item in dptUsers)
            {
                dptMemberList.Add(new SelectListItem
                {
                    Text = item.FullName,
                    Value = item.Id.ToString()
                });
            }
            ViewData["DptMembers"] = new SelectList(dptMemberList, "Value", "Text");

            return View(r);
        }

        // POST: BMED/Keep/Create
        [HttpPost]
        public IActionResult Create(KeepModel keep)
        {
            AppUserModel ur = _context.AppUsers.Where(u => u.UserName == this.User.Identity.Name).FirstOrDefault();

            if (string.IsNullOrEmpty(keep.AssetNo))
            {
                throw new Exception("財產編號不可空白!!");
            }
            string msg = "";
            try
            {
                if (ModelState.IsValid)
                {

                    //更新申請人的Email
                    if (string.IsNullOrEmpty(keep.Email))
                    {
                        throw new Exception("電子信箱不可空白!!");
                    }
                    AppUserModel a = _context.AppUsers.Find(keep.UserId);
                    a.Email = keep.Email;
                    _context.Entry(a).State = EntityState.Modified;
                    _context.SaveChanges();
                    //
                    AssetKeepModel kp = _context.BMEDAssetKeeps.Find(keep.AssetNo);
                    AssetModel at = _context.BMEDAssets.Find(keep.AssetNo);
                    //
                    keep.AssetName = _context.BMEDAssets.Find(keep.AssetNo).Cname;
                    keep.EngId = kp.KeepEngId;
                    //keep.AccDpt = at.AccDpt;
                    keep.SentDate = DateTime.Now;
                    keep.Cycle = kp == null ? 0 : (kp.Cycle == null ? 0 : kp.Cycle.Value);
                    keep.Src = "M";
                    _context.Entry(keep).State = EntityState.Modified;

                    //
                    KeepDtlModel dl = new KeepDtlModel();
                    var notInExceptDevice = _context.ExceptDevice.Find(keep.AssetNo);
                    /* If can find data in ExceptDevice table, the device is "not" 統包. 
                     * It means if value is "Y", the device is 統包
                     */
                    if (notInExceptDevice == null)
                    {
                        dl.NotInExceptDevice = "Y";
                    }
                    else
                    {
                        dl.NotInExceptDevice = "N";
                    }
                    dl.DocId = keep.DocId;
                    switch (kp == null ? "自行" : kp.InOut)
                    {
                        case "自行":
                            dl.InOut = "0";
                            break;
                        case "委外":
                            dl.InOut = "1";
                            break;
                        case "租賃":
                            dl.InOut = "2";
                            break;
                        case "保固":
                            dl.InOut = "3";
                            break;
                        default:
                            dl.InOut = "0";
                            break;
                    }
                    _context.BMEDKeepDtls.Add(dl);
                    _context.SaveChanges();
                    //
                    KeepFlowModel rf = new KeepFlowModel();
                    rf.DocId = keep.DocId;
                    rf.StepId = 1;
                    rf.UserId = ur.Id;
                    rf.Status = "1";
                    //rf.Role = Roles.GetRolesForUser().FirstOrDefault();
                    rf.Rtp = ur.Id;
                    rf.Rdt = null;
                    rf.Rtt = DateTime.Now;
                    rf.Cls = "申請者";
                    _context.BMEDKeepFlows.Add(rf);
                    //
                    rf = new KeepFlowModel();
                    rf.DocId = keep.DocId;
                    rf.StepId = 2;
                    rf.UserId = kp == null ? ur.Id : kp.KeepEngId;
                    rf.Status = "?";
                    AppUserModel u = _context.AppUsers.Find(rf.UserId);
                    if (u == null)
                    {
                        throw new Exception("無工程師資料!!");
                    }
                    //rf.Role = Roles.GetRolesForUser(u.UserName).FirstOrDefault();
                    rf.Rtp = null;
                    rf.Rdt = null;
                    rf.Rtt = DateTime.Now;
                    rf.Cls = "設備工程師";
                    _context.BMEDKeepFlows.Add(rf);
                    _context.SaveChanges();
                    //send mail
                    Tmail mail = new Tmail();
                    string body = "";
                    u = _context.AppUsers.Find(ur.Id);
                    mail.from = new System.Net.Mail.MailAddress(u.Email); //u.Email
                    //u = _context.AppUsers.Find(kp.KeepEngId);
                    mail.to = new System.Net.Mail.MailAddress(u.Email); //u.Email
                    mail.message.Subject = "醫工工務智能保修系統[醫工保養案]：設備名稱： " + keep.AssetName;
                    body += "<p>表單編號：" + keep.DocId + "</p>";
                    body += "<p>送單日期：" + keep.SentDate.Value.ToString("yyyy/MM/dd") + "</p>";
                    body += "<p>申請人：" + keep.UserName + "</p>";
                    body += "<p>財產編號：" + keep.AssetNo + "</p>";
                    body += "<p>設備名稱：" + keep.AssetName + "</p>";
                    body += "<p>放置地點：" + keep.PlaceLoc + "</p>";
                    body += "<p><a href='http://dms.cch.org.tw/EDIS/Account/Login'" + "?docId=" + keep.DocId + "&dealType=BMEDKeepEdit" + ">處理案件</a></p>";
                    body += "<br/>";
                    body += "<h3>此封信件為系統通知郵件，請勿回覆。</h3>";
                    mail.message.Body = body;
                    mail.message.IsBodyHtml = true;
                    mail.SendMail();

                    return Ok(keep);
                }
                else
                {
                    msg = "";
                    foreach (var error in ViewData.ModelState.Values.SelectMany(modelState => modelState.Errors))
                    {
                        msg += error.ErrorMessage + Environment.NewLine;
                    }
                    throw new Exception(msg);
                }
            }
            catch(Exception ex)
            {
                msg = ex.Message;
            }
            return BadRequest(msg);
        }

        public string GetID()
        {
            string s = _context.BMEDKeeps.Select(r => r.DocId).Max();
            string did = "";
            int yymm = (System.DateTime.Now.Year - 1911) * 100 + System.DateTime.Now.Month;
            if (!string.IsNullOrEmpty(s))
            {
                did = s;
            }
            if (did != "")
            {
                if (Convert.ToInt64(did) / 10000 == yymm)
                    did = Convert.ToString(Convert.ToInt64(did) + 1);
                else
                    did = Convert.ToString(yymm * 10000 + 1);
            }
            else
            {
                did = Convert.ToString(yymm * 10000 + 1);
            }
            return did;
        }

        // POST: BMED/Keep/Index
        [HttpPost]
        public IActionResult Index(QryKeepListData qdata)
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

            if (qtyEngCode != null)
            {
                searchAllDoc = true;
            }

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


            List<KeepListVModel> kv = new List<KeepListVModel>();
            /* Get login user. */
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();

            var kps = _context.BMEDKeeps.ToList();
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
            /* Search date by DateType.(ApplyDate) */
            if (string.IsNullOrEmpty(qtyDate1) == false || string.IsNullOrEmpty(qtyDate2) == false)
            {
                if (qtyDateType == "送單日")
                {
                    kps = kps.Where(v => v.SentDate >= applyDateFrom && v.SentDate <= applyDateTo).ToList();
                }
            }

            /* If no search result. */
            if (kps.Count() == 0)
            {
                return View("List", kv);
            }

            switch (ftype)
            {
                /* 與登入者相關且流程不在該登入者身上的文件 */
                case "流程中":
                    kps.Join(_context.BMEDKeepFlows.Where(f2 => f2.UserId == ur.Id && f2.Status == "1")
                       .Select(f => f.DocId).Distinct(),
                               r => r.DocId, f2 => f2, (r, f2) => r)
                       .Join(_context.BMEDKeepFlows.Where(f => f.Status == "?" && f.UserId != ur.Id),
                        r => r.DocId, f => f.DocId,
                        (r, f) => new
                        {
                            keep = r,
                            flow = f
                        }).Join(_context.BMEDAssets, r => r.keep.AssetNo, a => a.AssetNo,
                        (r, a) => new
                        {
                            keep = r.keep,
                            asset = a,
                            flow = r.flow
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
                        .ForEach(j => kv.Add(new KeepListVModel
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
                            Src = j.keep.Src,
                            SentDate = j.keep.SentDate,
                            EndDate = j.keepdtl.EndDate,
                            IsCharged = j.keepdtl.IsCharged,
                            keepdata = j.keep
                        }));
                        break;
                case "已結案":
                    List<KeepFlowModel> kf = _context.BMEDKeepFlows.Where(f => f.Status == "2").ToList();

                    if (userManager.IsInRole(User, "Admin") || userManager.IsInRole(User, "MedAdmin") || 
                        userManager.IsInRole(User, "Manager") || userManager.IsInRole(User, "MedEngineer"))
                    {
                        if (userManager.IsInRole(User, "Manager"))
                        {
                            kf = kf.Join(_context.BMEDKeeps.Where(r => r.AccDpt == ur.DptId),
                            f => f.DocId, r => r.DocId, (f, r) => f).ToList();
                        }
                        /* If no other search values, search the docs belong the login engineer. */
                        if (userManager.IsInRole(User, "MedEngineer") && searchAllDoc == false)
                        {
                            kf = kf.Join(_context.BMEDKeepFlows.Where(f2 => f2.UserId == ur.Id),
                            f => f.DocId, f2 => f2.DocId, (f, f2) => f).ToList();
                        }
                    }
                    else /* If normal user, search the docs belong himself. */
                    {
                        kf = kf.Join(_context.BMEDKeepFlows.Where(f2 => f2.UserId == ur.Id),
                        f => f.DocId, f2 => f2.DocId, (f, f2) => f).ToList();
                    }
                    //
                    kf.Select(f => new
                      {
                          f.DocId,
                          f.UserId,
                          f.Cls,
                          f.Status
                      }).Distinct()
                      .Join(kps.DefaultIfEmpty(), f => f.DocId, k => k.DocId,
                      (f, k) => new
                      {
                          keep = k,
                          flow = f
                      }).Join(_context.BMEDAssets, r => r.keep.AssetNo, a => a.AssetNo,
                      (r, a) => new
                      {
                          keep = r.keep,
                          asset = a,
                          flow = r.flow
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
                      .ForEach(j => kv.Add(new KeepListVModel
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
                          Src = j.keep.Src,
                          SentDate = j.keep.SentDate,
                          EndDate = j.keepdtl.EndDate,
                          CloseDate = j.keepdtl.CloseDate.Value.Date,
                          IsCharged = j.keepdtl.IsCharged,
                          keepdata = j.keep
                      }));
                      break;
                case "待簽核":
                    /* Get all dealing repair docs. */
                    var keepFlows = _context.BMEDKeepFlows.Join(kps.DefaultIfEmpty(), f => f.DocId, k => k.DocId,
                    (f, k) => new
                    {
                        keep = k,
                        flow = f
                    }).ToList();

                    if (userManager.IsInRole(User, "Admin") || userManager.IsInRole(User, "MedAdmin") || 
                        userManager.IsInRole(User, "MedEngineer"))
                    {
                        /* If has other search values, search all RepairDocs which flowCls is in engineer. */
                        /* Else return the docs belong the login engineer.  */
                        if (userManager.IsInRole(User, "MedEngineer") && searchAllDoc == true)
                        {
                            keepFlows = keepFlows.Where(f => f.flow.Status == "?" && f.flow.Cls.Contains("工程師")).ToList();
                            if (!string.IsNullOrEmpty(qtyEngCode))  //工程師搜尋
                            {
                                keepFlows = keepFlows.Where(f => f.keep.EngId == Convert.ToInt32(qtyEngCode)).ToList();
                            }
                        }
                        else
                        {
                            /* 個人或同部門結案案件 */
                            //keepFlows = keepFlows.Where(f => (f.flow.Status == "?" && f.flow.UserId == ur.Id) ||
                            //                                 (f.flow.Status == "?" && f.flow.Cls == "驗收人" &&
                            //                                  _context.AppUsers.Find(f.flow.UserId).DptId == ur.DptId)).ToList();

                            /* 個人案件 */
                            keepFlows = keepFlows.Where(f => (f.flow.Status == "?" && f.flow.UserId == ur.Id)).ToList();
                        }
                    }
                    else
                    {
                        /* 個人或同部門結案案件 */
                        //keepFlows = keepFlows.Where(f => (f.flow.Status == "?" && f.flow.UserId == ur.Id) ||
                        //                                 (f.flow.Status == "?" && f.flow.Cls == "驗收人" &&
                        //                                  _context.AppUsers.Find(f.flow.UserId).DptId == ur.DptId)).ToList();

                        /* 個人案件 */
                        keepFlows = keepFlows.Where(f => (f.flow.Status == "?" && f.flow.UserId == ur.Id)).ToList();
                    }

                    keepFlows.Join(_context.BMEDAssets, r => r.keep.AssetNo, a => a.AssetNo,
                    (r, a) => new
                    {
                        keep = r.keep,
                        asset = a,
                        flow = r.flow
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
                    .ForEach(j => kv.Add(new KeepListVModel
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
                        Src = j.keep.Src,
                        SentDate = j.keep.SentDate,
                        EndDate = j.keepdtl.EndDate,
                        IsCharged = j.keepdtl.IsCharged,
                        keepdata = j.keep
                    }));
                    break;
            };

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

            return View("List", kv);
        }

        // GET: BMED/Keep/QueryAssets
        public JsonResult QueryAssets(string QueryStr, string QueryAccDpt, string QueryDelivDpt)
        {
            List<AssetModel> assets = new List<AssetModel>();
            // No query string.
            if (string.IsNullOrEmpty(QueryStr) && string.IsNullOrEmpty(QueryAccDpt) && string.IsNullOrEmpty(QueryDelivDpt))
            {
                assets = _context.BMEDAssets.Where(a => a.AssetNo.Contains(QueryStr) ||
                                                        a.Cname.Contains(QueryStr)).ToList();
            }
            else
            {
                assets = _context.BMEDAssets.ToList();
                if (!string.IsNullOrEmpty(QueryStr))     /* Search assets by assetNo or Cname. */
                {
                    assets = assets.Where(a => a.AssetNo.Contains(QueryStr) ||
                                               a.Cname.Contains(QueryStr)).ToList();
                }
                if (!string.IsNullOrEmpty(QueryAccDpt))    /* Search assets by AccDpt. */
                {
                    assets = assets.Where(a => a.AccDpt == QueryAccDpt).ToList();
                }
                if (!string.IsNullOrEmpty(QueryDelivDpt))   /* Search assets by DelivDpt. */
                {
                    assets = assets.Where(a => a.DelivDpt == QueryDelivDpt).ToList();
                }
            }

            List<SelectListItem> list = new List<SelectListItem>();
            if (assets.Count() != 0)
            {
                assets.ForEach(asset => {
                       list.Add(new SelectListItem
                       {
                           Text = asset.Cname + "(" + asset.AssetNo + ")",
                           Value = asset.AssetNo.ToString()
                       });
                });
            }
            return Json(list);
        }

        // GET: BMED/Keep/Edit
        public IActionResult Edit(string id, int page)
        {
            AppUserModel ur = _context.AppUsers.Where(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            ViewData["Page"] = page;
            if (!string.IsNullOrEmpty(id))
            {
                KeepModel keep = _context.BMEDKeeps.Find(id);
                if (keep == null)
                {
                    return StatusCode(404);
                }
                if (userManager.IsInRole(User, "Admin") || userManager.IsInRole(User, "MedAdmin") || 
                    userManager.IsInRole(User, "MedManager") || userManager.IsInRole(User, "MedEngineer"))
                {
                    return View(keep);
                }
                KeepFlowModel rf = _context.BMEDKeepFlows.Where(f => f.DocId == id && f.Status == "?").FirstOrDefault();
                if (rf != null)
                {
                    if (rf.UserId != ur.Id)
                    {
                        return RedirectToAction("Index", "Home", new { Area = "" });
                    }
                }
                return View(keep);
            }
            return StatusCode(404);
        }

        // POST: BMED/Keep/Update/5
        [HttpPost]
        public IActionResult Update(KeepModel keepModel)
        {
            KeepModel keep = _context.BMEDKeeps.Find(keepModel.DocId);
            if (keep == null)
            {
                return BadRequest("查無案件!");
            }

            if (string.IsNullOrEmpty(keepModel.AccDpt))
            {
                return BadRequest("成本中心不可空白!");
            }

            keepModel.AccDpt = keepModel.AccDpt.Trim();
            var dpt = _context.Departments.Find(keepModel.AccDpt);
            if (dpt == null)
            {
                return BadRequest("此編號查無部門!");
            }
            keep.AccDpt = keepModel.AccDpt;
            _context.Entry(keep).State = EntityState.Modified;
            _context.SaveChanges();
            return PartialView("Update", keep);
        }

        // GET: BMED/Keep/Views
        public IActionResult Views(string id)
        {
            KeepModel keep = _context.BMEDKeeps.Find(id);
            if (keep == null)
            {
                return StatusCode(404);
            }
            return View(keep);
        }

        // GET: BMED/Keep/GetKeepCounts
        public JsonResult GetKeepCounts()
        {
            /* Get user details. */
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            var keepCount = _context.BMEDKeepFlows.Where(f => f.Status == "?")
                                                  .Where(f => f.UserId == ur.Id).Count();
            return Json(keepCount);
        }

        // GET: BMED/Keep/PrintKeepDoc/5
        public IActionResult PrintKeepDoc(string DocId, int printType)
        {
            /* Get all print details according to the DocId. */
            KeepModel keep = _context.BMEDKeeps.Find(DocId);
            KeepDtlModel dtl = _context.BMEDKeepDtls.Find(DocId);
            KeepEmpModel emp = _context.BMEDKeepEmps.Where(ep => ep.DocId == DocId).FirstOrDefault();

            /* Get the last flow. */
            string[] s = new string[] { "?", "2" };
            KeepFlowModel flow = _context.BMEDKeepFlows.Where(f => f.DocId == DocId)
                                                       .Where(f => s.Contains(f.Status)).FirstOrDefault();
            KeepPrintVModel vm = new KeepPrintVModel();
            if (keep == null)
            {
                return StatusCode(404);
            }
            else
            {
                vm.Docid = DocId;
                vm.UserId = keep.UserId;
                vm.UserName = keep.UserName;
                vm.UserAccount = _context.AppUsers.Find(keep.UserId).UserName;
                vm.AccDpt = keep.AccDpt;
                vm.SentDate = keep.SentDate;
                vm.AssetNo = keep.AssetNo;
                vm.AssetNam = keep.AssetName;
                vm.Company = _context.Departments.Find(keep.DptId).Name_C;
                vm.Amt = 1;
                vm.Cycle = keep.Cycle;
                vm.Contact = keep.Ext;
                vm.PlaceLoc = keep.PlaceLoc;

                if (dtl != null)
                {
                    vm.Result = dtl.Result == null ? "" : _context.BMEDKeepResults.Find(dtl.Result).Title;
                    vm.Memo = dtl.Memo;
                    vm.EndDate = dtl.EndDate;
                }
                //
                vm.AccDptNam = _context.Departments.Find(keep.AccDpt).Name_C;
                vm.Hour = dtl.Hours == null ? 0 : dtl.Hours.Value;
                vm.InOut = dtl.InOut == "0" ? "自行" :
                        dtl.InOut == "1" ? "委外" :
                        dtl.InOut == "2" ? "租賃" :
                        dtl.InOut == "3" ? "保固" : "";
                //vm.EngName = emp == null ? "" : _context.AppUsers.Find(emp.UserId).FullName;
                var lastFlowEng = _context.BMEDKeepFlows.Where(rf => rf.DocId == DocId)
                                                        .Where(rf => rf.Cls.Contains("工程師"))
                                                        .OrderByDescending(rf => rf.StepId).FirstOrDefault();
                AppUserModel EngTemp = _context.AppUsers.Find(lastFlowEng.UserId);
                if (EngTemp != null)
                {
                    vm.EngName = EngTemp.FullName + " (" + EngTemp.UserName + ")";
                }
                else
                {
                    vm.EngName = "";
                }

                var engMgr = _context.BMEDKeepFlows.Where(r => r.DocId == DocId)
                                                   .Where(r => r.Cls.Contains("醫工主管")).ToList();
                if (engMgr.Count() != 0)
                {
                    engMgr = engMgr.GroupBy(e => e.UserId).Select(group => group.FirstOrDefault()).ToList();
                    foreach (var item in engMgr)
                    {
                        vm.EngMgr += item == null ? "" : _context.AppUsers.Find(item.UserId).FullName + "  ";
                    }
                }

                var engDirector = _context.BMEDKeepFlows.Where(r => r.DocId == DocId)
                                                        .Where(r => r.Cls.Contains("醫工主任")).LastOrDefault();
                vm.EngDirector = engDirector == null ? "" : _context.AppUsers.Find(engDirector.UserId).FullName;

                if (flow != null)
                {
                    if (flow.Status == "2")
                    {
                        vm.CloseDate = flow.Rtt;
                        AppUserModel u = _context.AppUsers.Find(flow.UserId);
                        if (u != null)
                        {
                            vm.DelivEmp = u.UserName;
                            vm.DelivEmpName = u.FullName;
                        }
                    }
                }
            }
            //if (printType != 0)
            //{
            //    return View("PrintRepairDoc2", vm);
            //}
            return View(vm);
        }

        // GET: BMED/Keep/Delete/5
        public IActionResult Delete(string id)
        {
            // Find document.
            KeepModel keep = _context.BMEDKeeps.Find(id);
            keep.DptName = _context.Departments.Find(keep.DptId).Name_C;
            keep.AccDptName = _context.Departments.Find(keep.AccDpt).Name_C;
            keep.UserAccount = _context.AppUsers.Find(keep.UserId).UserName;

            if (keep == null)
            {
                return StatusCode(404);
            }
            return View(keep);
        }

        // POST: BMED/Keep/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            KeepFlowModel keepflow = _context.BMEDKeepFlows.Where(f => f.DocId == id && f.Status == "?")
                                                           .FirstOrDefault();
            keepflow.Status = "3";
            keepflow.Rtp = ur.Id;
            keepflow.Rtt = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
            base.Dispose(disposing);
        }
    }
}