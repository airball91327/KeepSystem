using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EDIS.Areas.BMED.Data;
using EDIS.Areas.BMED.Models.KeepModels;
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
    public class DepartmentController : Controller
    {
        private readonly BMEDDbContext _context;

        public DepartmentController(BMEDDbContext context)
        {
            _context = context;
        }

        public class DptList
        {
            public string dptid;
            public string dptname;
        }

        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetDptsByKeyname(string keyname)
        {
            List<DptList> dpts = new List<DptList>();

            string s = "";
            if (!string.IsNullOrEmpty(keyname))
            {
                _context.Departments.Where(d => d.Name_C.Contains(keyname))
                  .ToList()
                  .ForEach(dp =>
                  {
                      DptList d = new DptList();
                      d.dptid = dp.DptId;
                      d.dptname = dp.Name_C;
                      dpts.Add(d);
                  });
                //
                DepartmentModel dt = _context.Departments.Find(keyname);
                if (dt != null)
                {
                    DptList d = new DptList();
                    d.dptid = dt.DptId;
                    d.dptname = dt.Name_C;
                    dpts.Add(d);
                }
            }
            else
            {
                _context.Departments.ToList()
                  .ForEach(dp =>
                  {
                      DptList d = new DptList();
                      d.dptid = dp.DptId;
                      d.dptname = dp.Name_C;
                      dpts.Add(d);
                  });
            }
            s = JsonConvert.SerializeObject(dpts);

            return Json(s);
        }
    }
}