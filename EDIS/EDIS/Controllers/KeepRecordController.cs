using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Data;
using EDIS.Models.KeepModels;
using EDIS.Models.RepairModels;
using EDIS.Models;
using EDIS.Models.Identity;
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
    public class KeepRecordController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KeepRecordController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: KeepRecord/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormCollection vmodel)
        {
            if (ModelState.IsValid)
            {
                KeepRecordModel r;
                KeepRecordModel r2;
                int i = vmodel["item.Sno"].Count();
                for (int j = 0; j < i; j++)
                {
                    r = new KeepRecordModel();
                    r.DocId = vmodel["item.DocId"][j];
                    r.FormatId = vmodel["item.FormatId"][j];
                    r.Sno = Convert.ToInt32(vmodel["item.Sno"][j]);
                    r.ListNo = Convert.ToInt32(vmodel["item.ListNo"][j]);
                    r.Descript = vmodel["item.Descript"][j];
                    r.IsFunctional = vmodel["item.IsFunctional[" + r.Sno + "]"][0];
                    r.KeepDes = vmodel["item.KeepDes"][j];
                    r2 = _context.KeepRecords.Find(r.DocId, r.FormatId, r.Sno, r.ListNo);
                    if (r2 != null)
                    {
                        r2.IsFunctional = r.IsFunctional;
                        r2.KeepDes = r.KeepDes;
                        _context.Entry(r2).State = EntityState.Modified;
                    }
                    else
                    {
                        _context.KeepRecords.Add(r);
                    }
                }
                try
                {
                    _context.SaveChanges();
                    return new JsonResult(vmodel)
                    {
                        Value = new { success = true, error = "" }
                    };
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
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

        // GET: KeepRecord/GetRecordList
        public async Task<IActionResult> GetRecordList(int listNo, string docId = null)
        {
            KeepModel kp = _context.Keeps.Find(docId);
            List<KeepFormatListVModel> kf = new List<KeepFormatListVModel>();
            KeepFormatModel f;
            KeepRecordModel r;
            if (kp != null)
            {
                //AssetKeepModel ak = _context.AssetKeeps.Find(kp.DeviceNo);
                //if (ak != null)
                //{
                    if (!string.IsNullOrEmpty(kp.FormatId))
                    {
                        _context.KeepFormatDtls.Where(d => d.FormatId == kp.FormatId)
                                .ToList()
                                .ForEach(d =>
                                {
                                    kf.Add(new KeepFormatListVModel
                                    {
                                        Docid = docId,
                                        FormatId = d.FormatId,
                                        Plants = (f = _context.KeepFormats.Find(d.FormatId)) == null ? "" :
                                        f.Plants,
                                        Sno = d.Sno,
                                        ListNo = listNo,
                                        Descript = d.Descript,
                                        KeepDes = (r = _context.KeepRecords.Find(docId, d.FormatId, d.Sno, listNo)) == null ? "" :
                                        r.KeepDes,
                                        IsRequired = d.IsRequired
                                    });
                                });

                        // 同步新增預設資料進DB.
                        KeepRecordModel keepRecord;
                        foreach (var item in kf)
                        {
                            keepRecord = new KeepRecordModel();
                            keepRecord.DocId = item.Docid;
                            keepRecord.FormatId = item.FormatId;
                            keepRecord.Sno = item.Sno;
                            keepRecord.ListNo = item.ListNo;
                            keepRecord.Descript = item.Descript;
                            keepRecord.KeepDes = item.KeepDes;
                            _context.KeepRecords.Add(keepRecord);
                        }
                        if (listNo == 2)
                        {
                            // 新增第二筆紀錄時，如第一筆資料尚未新增，一併新增。
                            var findFirstData = _context.KeepRecords.Where(kr => kr.DocId == docId && kr.ListNo == 1);
                            if (findFirstData.Count() <= 0)
                            {
                                foreach (var item in kf)
                                {
                                    keepRecord = new KeepRecordModel();
                                    keepRecord.DocId = item.Docid;
                                    keepRecord.FormatId = item.FormatId;
                                    keepRecord.Sno = item.Sno;
                                    keepRecord.ListNo = 1;
                                    keepRecord.Descript = item.Descript;
                                    keepRecord.KeepDes = item.KeepDes;
                                    _context.KeepRecords.Add(keepRecord);
                                }
                            }
                        }
                        _context.SaveChanges();
                    }
                //}
                return View(kf);
            }
            else
            {
                return BadRequest();
            }
        }

        // POST: KeepRecord/DeleteRecords
        [HttpPost]
        public async Task<IActionResult> DeleteRecords(int listNo, string docId)
        {
            var keepRecords = _context.KeepRecords.Where(kr => kr.DocId == docId && kr.ListNo == listNo);
            _context.KeepRecords.RemoveRange(keepRecords);
            try
            {
                _context.SaveChanges();
                return new JsonResult(keepRecords)
                {
                    Value = new { success = true, error = "" }
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
