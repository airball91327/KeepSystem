using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EDIS.Controllers.WebApi.RepCreateController;

namespace EDIS.Fliters
{
    public class WebApiValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                // ModelState錯誤訊息
                var ModelStateErrors = context.ModelState.Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(k => k.Key, k => k.Value.Errors.Select(e => e.ErrorMessage).ToArray()).FirstOrDefault();
                Root errorXMLMsg2 = new Root { Code = "400", Msg = ModelStateErrors.Value.FirstOrDefault(), SerNo = "", Mno = "" };

                context.Result = new BadRequestObjectResult(errorXMLMsg2);
            }
        }
    }
}
