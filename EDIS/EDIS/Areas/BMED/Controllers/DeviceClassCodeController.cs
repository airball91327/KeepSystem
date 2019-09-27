using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EDIS.Areas.BMED.Data;
using EDIS.Areas.BMED.Models.KeepModels;
using EDIS.Areas.BMED.Models.RepairModels;
using EDIS.Models.Identity;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EDIS.Areas.BMED.Controllers
{
    [Area("BMED")]
    [Authorize]
    public class DeviceClassCodeController : Controller
    {
        private readonly BMEDDbContext _context;

        public DeviceClassCodeController(BMEDDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetDataByKeyname(string keyname)
        {
            List<DeviceClassCode> ul = new List<DeviceClassCode>();
            string s = "";
            if (string.IsNullOrEmpty(keyname))
                ul = _context.BMEDDeviceClassCodes.ToList();
            else
            {
                if (_context.BMEDDeviceClassCodes.Find(keyname) != null)
                    ul.Add(_context.BMEDDeviceClassCodes.Find(keyname));
                ul.AddRange(_context.BMEDDeviceClassCodes.Where(p => p.M_name.Contains(keyname)).ToList());
            }
            s = JsonConvert.SerializeObject(ul);
            return Json(s);
        }
    }
}