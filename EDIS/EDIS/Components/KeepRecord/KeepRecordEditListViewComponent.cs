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
    public class KeepRecordEditListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public KeepRecordEditListViewComponent(ApplicationDbContext context,
                                               IRepository<AppUserModel, int> userRepo,
                                               CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(int listNo, string id = null)
        {
            KeepModel kp = _context.Keeps.Find(id);
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
                                        Docid = id,
                                        FormatId = d.FormatId,
                                        Plants = (f = _context.KeepFormats.Find(d.FormatId)) == null ? "" :
                                        f.Plants,
                                        Sno = d.Sno,
                                        ListNo = listNo,
                                        Descript = d.Descript,
                                        IsFunctional = (r = _context.KeepRecords.Find(id, d.FormatId, d.Sno, listNo)) == null ? "" :
                                        r.IsFunctional,
                                        KeepDes = (r = _context.KeepRecords.Find(id, d.FormatId, d.Sno, listNo)) == null ? "" :
                                        r.KeepDes,
                                        IsRequired = d.IsRequired
                                    });
                                });
                    }

                //}
                return View(kf);
            }
            return Content("Page not found!");  //ViewComponent can't return HTTP response (like as StatusCode() or BasRequest())
        }
    }
}
