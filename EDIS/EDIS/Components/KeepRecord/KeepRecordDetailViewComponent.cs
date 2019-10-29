using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.KeepModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.KeepRecord
{
    public class KeepRecordDetailViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public KeepRecordDetailViewComponent(ApplicationDbContext context,
                                             IRepository<AppUserModel, int> userRepo,
                                             CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id = null)
        {
            KeepModel kp = _context.Keeps.Find(id);
            List<KeepFormatListVModel> kf = new List<KeepFormatListVModel>();
            KeepFormatModel f;
            KeepRecordModel r;
            if (kp != null)
            {
                //AssetKeepModel ak = _context.AssetKeeps.Find(kp.AssetNo);
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
                                        Docid = id,
                                        FormatId = d.FormatId,
                                        Plants = (f = _context.KeepFormats.Find(d.FormatId)) == null ? "" :
                                        f.Plants,
                                        Sno = d.Sno,
                                        Descript = d.Descript,
                                        IsFunctional = (r = _context.KeepRecords.Find(id, d.FormatId, d.Sno, 1)) == null ? "" :
                                        r.IsFunctional,
                                        KeepDes = (r = _context.KeepRecords.Find(id, d.FormatId, d.Sno, 1)) == null ? "" :
                                        r.KeepDes
                                    });
                                });
                    }
                //}

                // 處理多張保養紀錄
                List<KeepFormatListVModel> kf2 = new List<KeepFormatListVModel>();
                var keepRecords = _context.KeepRecords.Where(kr => kr.DocId == id);
                if (keepRecords != null)
                {
                    var countListNo = keepRecords.Select(kr => kr.ListNo).Distinct().Count();
                    ViewData["CountList"] = countListNo;
                    if (countListNo >= 2)
                    {
                        keepRecords.Join(_context.KeepFormatDtls,
                            kr => new { FormatId = kr.FormatId, Sno = kr.Sno },
                            fd => new { FormatId = fd.FormatId, Sno = fd.Sno },
                            (kr, fd) => new
                            {
                                keeprecords = kr,
                                formatdtls = fd
                            }).ToList()
                              .ForEach(d =>
                              {
                                  kf2.Add(new KeepFormatListVModel
                                  {
                                      Docid = id,
                                      FormatId = d.formatdtls.FormatId,
                                      Plants = (f = _context.KeepFormats.Find(d.formatdtls.FormatId)) == null ? "" :
                                      f.Plants,
                                      Sno = d.keeprecords.Sno,
                                      ListNo = d.keeprecords.ListNo,
                                      Descript = d.keeprecords.Descript,
                                      IsFunctional = d.keeprecords.IsFunctional == null ? "" :
                                      d.keeprecords.IsFunctional,
                                      KeepDes = d.keeprecords.KeepDes == null ? "" :
                                      d.keeprecords.KeepDes
                                  });
                              });
                        kf2 = kf2.OrderBy(k => k.Sno).ThenBy(k => k.ListNo).ToList();
                        return View("Detail2", kf2);
                    }
                }
                return View(kf);
            }
            return Content("Page not found!");  //ViewComponent can't return HTTP response (like as StatusCode() or BasRequest())
        }

    }
}
