using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS.Data;
using EDIS.Models.RepairModels;
using Microsoft.AspNetCore.Authorization;

namespace EDIS.Controllers
{
    [Authorize]
    public class ScrapAssetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScrapAssetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ScrapAsset
        public async Task<IActionResult> Index()
        {
            return View(await _context.ScrapAssets.ToListAsync());
        }

        // GET: ScrapAsset/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ScrapAsset/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DocId,AssetNo")] ScrapAssetModel scrapAssetModel)
        {
            ModelState.Remove("DeviceNo");
            if (ModelState.IsValid)
            {
                var asset = _context.Assets.Where(a => a.AssetNo == scrapAssetModel.AssetNo).FirstOrDefault();
                if(asset == null)
                {
                    return new JsonResult(scrapAssetModel)
                    {
                        Value = new { isExist = true, error = "查無此財產編號!" }
                    };
                }
                var ExistScrapAsset = _context.ScrapAssets.Find(scrapAssetModel.DocId, asset.DeviceNo, scrapAssetModel.AssetNo);
                if (ExistScrapAsset != null)
                {
                    return new JsonResult(scrapAssetModel)
                    {
                        Value = new { isExist = true, error = "資料已存在!" }
                    };
                }
                else
                {
                    scrapAssetModel.DeviceNo = asset.DeviceNo;
                    _context.Add(scrapAssetModel);
                    await _context.SaveChangesAsync();
                }

                // Return ViewComponent for ajax request.
                return ViewComponent("ScrapAssetList", new { id = scrapAssetModel.DocId });
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

        // POST: ScrapAsset/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(string docId, string assetNo)
        {
            try
            {
                var deviceNo = _context.Assets.Where(a => a.AssetNo == assetNo).FirstOrDefault().DeviceNo;
                var scrapAssetModel = await _context.ScrapAssets.FindAsync(docId, deviceNo, assetNo);
                _context.ScrapAssets.Remove(scrapAssetModel);
                await _context.SaveChangesAsync();
                return new JsonResult(scrapAssetModel)
                {
                    Value = new { success = true, error = "" }
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }        
        }

        // GET: ScrapAsset/GetScrapList/5
        public ActionResult GetScrapList(string docId, int? printType)
        {
            return ViewComponent("ScrapAssetList", new { id = docId, type = printType });
        }

        private bool ScrapAssetModelExists(string id)
        {
            return _context.ScrapAssets.Any(e => e.DocId == id);
        }
    }
}
