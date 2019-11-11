using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using EDIS.Data;
using EDIS.Fliters;
using EDIS.Models;
using EDIS.Models.Identity;
using EDIS.Models.LocationModels;
using EDIS.Models.RepairModels;
using EDIS.Repositories;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Controllers.WebApi
{
    [Route("WebApi/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class RepCreateController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<RepairModel, string> _repRepo;
        private readonly IRepository<RepairDtlModel, string> _repdtlRepo;
        private readonly IRepository<RepairFlowModel, string[]> _repflowRepo;
        private readonly IRepository<DepartmentModel, string> _dptRepo;
        private readonly IRepository<DocIdStore, string[]> _dsRepo;
        private readonly IRepository<BuildingModel, int> _buildRepo;
        private readonly CustomRoleManager roleManager;

        public RepCreateController(ApplicationDbContext context,
                                   IRepository<RepairModel, string> repairRepo,
                                   IRepository<RepairDtlModel, string> repairdtlRepo,
                                   IRepository<RepairFlowModel, string[]> repairflowRepo,
                                   IRepository<DepartmentModel, string> dptRepo,
                                   IRepository<DocIdStore, string[]> dsRepo,
                                   IRepository<BuildingModel, int> buildRepo,
                                   CustomRoleManager customRoleManager)
        {
            _context = context;
            _repRepo = repairRepo;
            _repdtlRepo = repairdtlRepo;
            _repflowRepo = repairflowRepo;
            _dptRepo = dptRepo;
            _dsRepo = dsRepo;
            _buildRepo = buildRepo;
            roleManager = customRoleManager;
        }

        public class Root
        {
            [Required]
            public string UsrID { get; set; }
            /// <summary>
            /// Hash MD5 加密
            /// </summary>
            [Required]
            public string Passwd { get; set; }  //DES 加密
            /// <summary>
            /// 結果代碼  0: 成功
            /// </summary>
            public string Code { get; set; }    //結果代碼  0: 成功
            public string Msg { get; set; }     //結果訊息
            [Required]
            public string SerNo { get; set; }   //事件處理編號
            public string Mno { get; set; }     //維修單號
            [Required]
            public string Building { get; set; }//大樓
            [Required]
            public string Floor { get; set; }   //樓層
            [Required]
            public string Point { get; set; }   //故障點編號
            [Required]
            public string Name { get; set; }    //故障點名稱
            [Required]
            public string Area { get; set; }    //區域
            [Required]
            public string Des { get; set; }     //處理描述
            [Required]
            public string Manager { get; set; } //維修人員
        }

        //// GET: WebApi/RepCreate
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET: WebApi/RepCreate/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: WebApi/RepCreate
        /// <summary>
        /// 工務請修系統提供之WebApi，驗證並新增工務請修單，回傳單號。
        /// </summary>
        /// <param name="root">客戶指定傳入之XML格式參數</param>
        [WebApiValidateModel]
        [HttpPost]
        [Produces("application/xml")] //強制回傳設定格式
        //[Consumes("application/xml")] //強制只接收設定格式
        public async Task<IActionResult> Post([FromBody] Root root)
        {
            var userName = root.UsrID;
            AppUserModel ur = _context.AppUsers.Where(u => u.UserName == userName).FirstOrDefault();

            if (ur != null)   //Check is UserName exist
            {
                //string DESKey = "12345678";
                //string userPW = DESDecrypt(root.Passwd, DESKey);    //DES decrypt.
                Boolean CheckPassWord = true;

                //// WebApi to check password.
                //HttpClient client = new HttpClient();
                //client.BaseAddress = new Uri("http://dms.cch.org.tw:8080/");
                //string url = "WebApi/Accounts/CheckPasswdForCch?id=" + root.UsrID;
                //url += "&pwd=" + HttpUtility.UrlEncode(userPW, Encoding.GetEncoding("UTF-8"));
                //client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //HttpResponseMessage response = await client.GetAsync(url);
                //string rstr = "";
                //if (response.IsSuccessStatusCode)
                //{
                //    rstr = await response.Content.ReadAsStringAsync();
                //}
                //client.Dispose();
                ////
                //if (rstr.Contains("成功")) //彰基2000帳號WebApi登入
                //{
                //    CheckPassWord = true;
                //}
                //else  //外包帳號 or 值班帳號
                //{
                //    /* Check and get external user. */
                //    var ExternalUser = _context.ExternalUsers.Where(ex => ex.UserName == root.UsrID).FirstOrDefault();
                //    if (ExternalUser != null && ExternalUser.Password == userPW)
                //    {
                //        CheckPassWord = true;
                //    }
                //}

                if (CheckPassWord == true)   //Check passed.
                {
                    RepairModel repair = new RepairModel();
                    var dpt = _dptRepo.Find(d => d.DptId == "8410").FirstOrDefault();
                    var accdpt = _dptRepo.Find(d => d.DptId == "9390").FirstOrDefault();
                    var troDes = root.Building + root.Floor + root.Point + root.Area + "，" + root.Name + root.Des;
                    repair.DocId = GetID2();
                    repair.UserId = ur.Id;
                    repair.UserName = ur.FullName;
                    repair.UserAccount = ur.UserName;
                    repair.DptId = dpt.DptId;
                    repair.DptName = dpt.Name_C;
                    repair.AccDpt = accdpt.DptId;
                    repair.AccDptName = accdpt.Name_C;
                    repair.ApplyDate = DateTime.Now;
                    repair.LocType = "本單位";
                    repair.RepType = "請修";
                    repair.Ext = ur.Ext == null ? "" : ur.Ext;
                    repair.TroubleDes = "【事件處理編號:" + root.SerNo + "】" + "\n" + troDes;
                    repair.AssetNo = root.Point;
                    repair.AssetName = root.Name;
                    repair.Building = "6";
                    repair.Floor = "612";
                    repair.Area = "8410";

                    // 用XML傳入的工程師名稱尋找負責工程師ID
                    var engs = roleManager.GetUsersInRole("RepEngineer").ToList();
                    int engId = _context.AppUsers.Where(a => a.UserName == "181316").FirstOrDefault().Id;
                    foreach(string l in engs)
                    {
                        var u = _context.AppUsers.Where(a => a.UserName == l).FirstOrDefault();
                        if (u != null)
                        {
                            if (u.FullName == root.Manager)
                            {
                                engId = u.Id;
                            }
                        }
                    }
                    repair.EngId = engId;
                    repair.CheckerId = ur.Id;

                    /* 如有代理人，將工程師改為代理人*/
                    var subStaff = _context.EngSubStaff.SingleOrDefault(e => e.EngId == repair.EngId);
                    if (subStaff != null)
                    {
                        int startDate = Convert.ToInt32(subStaff.StartDate.ToString("yyyyMMdd"));
                        int endDate = Convert.ToInt32(subStaff.EndDate.ToString("yyyyMMdd"));
                        int today = Convert.ToInt32(DateTime.UtcNow.AddHours(08).ToString("yyyyMMdd"));
                        /* 如在代理期間內，將代理人指定為負責工程師 */
                        if (today >= startDate && today <= endDate)
                        {
                            repair.EngId = subStaff.SubstituteId;
                        }
                    }

                    string msg = "";
                    try
                    {
                        // Create Repair Doc.
                        repair.ApplyDate = DateTime.Now;
                        _repRepo.Create(repair);

                        // Create Repair Details.
                        RepairDtlModel dtl = new RepairDtlModel();
                        dtl.DocId = repair.DocId;
                        dtl.DealState = 1;  // 處理狀態"未處理"
                        _repdtlRepo.Create(dtl);

                        //Create first Repair Flow.
                        RepairFlowModel flow = new RepairFlowModel();
                        flow.DocId = repair.DocId;
                        flow.StepId = 1;
                        flow.UserId = ur.Id;
                        flow.Status = "1";  // 流程狀態"已處理"
                        flow.Rtp = ur.Id;
                        flow.Rtt = DateTime.Now;
                        flow.Cls = "申請人";
                        _repflowRepo.Create(flow);

                        // Create next flow.
                        flow = new RepairFlowModel();
                        flow.DocId = repair.DocId;
                        flow.StepId = 2;
                        flow.UserId = repair.EngId;
                        flow.Status = "?";  // 狀態"未處理"
                        flow.Rtt = DateTime.Now;
                        flow.Cls = "工務/營建工程師";
                        _repflowRepo.Create(flow);

                        Root successXML = new Root { Code = "0", Msg = "Success", SerNo = root.SerNo, Mno = repair.DocId };
                        return Ok(successXML);
                    }
                    catch (Exception ex)
                    {
                        msg = ex.Message;
                    }
                    Root errorXMLMsg = new Root { Code = "400", Msg = msg, SerNo = root.SerNo, Mno = "" };
                    return BadRequest(errorXMLMsg);
                }
                else
                {
                    Root errorXMLMsg = new Root { Code = "400", Msg = "PassWord is incorrect.", SerNo = root.SerNo, Mno = "" };
                    return BadRequest(errorXMLMsg);
                }
            }
            else
            {
                Root errorXMLMsg = new Root { Code = "400", Msg = "UserID is not exist.", SerNo = root.SerNo, Mno = "" };
                return BadRequest(errorXMLMsg);
            }
        }

        //// PUT: WebApi/RepCreate/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE: WebApi/ApiWithActions/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}

        public string GetID2()
        {
            string did = "";
            try
            {
                DocIdStore ds = new DocIdStore();
                ds.DocType = "請修";
                string s = _dsRepo.Find(x => x.DocType == "請修").Select(x => x.DocId).Max();
                int yymmdd = (System.DateTime.Now.Year - 1911) * 10000 + (System.DateTime.Now.Month) * 100 + System.DateTime.Now.Day;
                if (!string.IsNullOrEmpty(s))
                {
                    did = s;
                }
                if (did != "")
                {
                    if (Convert.ToInt64(did) / 1000 == yymmdd)
                        did = Convert.ToString(Convert.ToInt64(did) + 1);
                    else
                        did = Convert.ToString(yymmdd * 1000 + 1);
                    ds.DocId = did;
                    _dsRepo.Update(ds);
                }
                else
                {
                    did = Convert.ToString(yymmdd * 1000 + 1);
                    ds.DocId = did;
                    // 二次確認資料庫內沒該資料才新增。
                    var dataIsExist = _dsRepo.Find(x => x.DocType == "請修");
                    if (dataIsExist.Count() == 0)
                    {
                        _dsRepo.Create(ds);
                    }
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return did;
        }

        public int GetAreaEngId(int BuildingId, string FloorId, string PlaceId)
        {
            var engineers = _context.EngsInDepts.Include(e => e.AppUsers).Include(e => e.Departments)
                                                .Where(e => e.BuildingId == BuildingId &&
                                                            e.FloorId == FloorId &&
                                                            e.PlaceId == PlaceId).ToList();

            /* 擷取預設負責工程師 */
            if (engineers.Count() == 0)  //該部門無預設工程師
            {
                var engId = _context.AppUsers.Where(a => a.UserName == "181316").FirstOrDefault().Id;
                return engId;
            }
            else
            {
                if (engineers.Count() > 1)
                {
                    var eng = engineers.Join(_context.EngDealingDocs, ed => ed.EngId, e => e.EngId,
                                (ed, e) => new
                                {
                                    ed.EngId,
                                    ed.UserName,
                                    ed.AppUsers.FullName,
                                    e.DealingDocs
                                }).OrderBy(o => o.DealingDocs).FirstOrDefault();
                    return eng.EngId;
                }
                else
                {
                    var eng = engineers.Select(e => new
                    {
                        e.EngId,
                        e.UserName,
                        e.AppUsers.FullName,
                    }).FirstOrDefault();
                    return eng.EngId;
                }
            }
        }

        /// <summary>
        /// 進行DES加密。
        /// </summary>
        /// <param name="pToEncrypt">要加密的字符串。</param>
        /// <param name="sKey">密鑰，且必須為8位。</param>
        /// <returns>以Base64格式返回的加密字符串。</returns>
        public string DESEncrypt(string pToEncrypt, string sKey)
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                byte[] inputByteArray = Encoding.UTF8.GetBytes(pToEncrypt);
                des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                string str = Convert.ToBase64String(ms.ToArray());
                ms.Close();
                return str;
            }
        }

        /// <summary>
        /// 進行DES解密。
        /// </summary>
        /// <param name="pToDecrypt">要解密的以Base64</param>
        /// <param name="sKey">密鑰，且必須為8位。</param>
        /// <returns>已解密的字符串。</returns>
        public string DESDecrypt(string pToDecrypt, string sKey)
        {
            byte[] inputByteArray = Convert.FromBase64String(pToDecrypt);
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                string str = Encoding.UTF8.GetString(ms.ToArray());
                ms.Close();
                return str;
            }
        }

    }
}
