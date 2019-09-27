using EDIS.Areas.BMED.Data;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.KeepModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Areas.BMED.Components.KeepRecord
{
    public class BMEDKeepRecordEditViewComponent : ViewComponent
    {
        private readonly BMEDDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public BMEDKeepRecordEditViewComponent(BMEDDbContext context,
                                               IRepository<AppUserModel, int> userRepo,
                                               CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id = null)
        {
            KeepModel kp = _context.BMEDKeeps.Find(id);
            List<KeepFormatListVModel> kf = new List<KeepFormatListVModel>();
            KeepFormatModel f;
            KeepRecordModel r;
            if (kp != null)
            {
                AssetKeepModel ak = _context.BMEDAssetKeeps.Find(kp.AssetNo);
                if (ak != null)
                {
                    if (!string.IsNullOrEmpty(ak.FormatId))
                    {
                        _context.BMEDKeepFormatDtls.Where(d => d.FormatId == ak.FormatId)
                                .ToList()
                                .ForEach(d =>
                                {
                                    kf.Add(new KeepFormatListVModel
                                    {
                                        Docid = id,
                                        FormatId = d.FormatId,
                                        Plants = (f = _context.BMEDKeepFormats.Find(d.FormatId)) == null ? "" :
                                        f.Plants,
                                        Sno = d.Sno,
                                        Descript = d.Descript,
                                        KeepDes = (r = _context.BMEDKeepRecords.Find(id, d.FormatId, d.Sno)) == null ? "" :
                                        r.KeepDes
                                    });
                                });
                    }

                }
                KeepFlowModel kf2 = _context.BMEDKeepFlows.Where(f2 => f2.DocId == id)
                                                          .Where(f2 => f2.Status == "?").FirstOrDefault();
                if (kf2.Cls.Contains("工程師"))
                    return View(kf);
                else
                    return View("Details", kf);
            }
            return Content("Page not found!");  //ViewComponent can't return HTTP response (like as StatusCode() or BasRequest())
        }
    }
}
