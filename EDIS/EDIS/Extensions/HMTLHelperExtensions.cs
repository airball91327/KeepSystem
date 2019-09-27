using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace EDIS.Extensions
{
    public static class HMTLHelperExtensions
    {
        public static string IsSelected(this IHtmlHelper html, string area = null, string controller = null, string action = null, string cssClass = null)
        {
            if (String.IsNullOrEmpty(cssClass))
                cssClass = "active";

            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            string currentController = (string)html.ViewContext.RouteData.Values["controller"];
            string currentArea = (string)html.ViewContext.RouteData.Values["area"];

            //if (String.IsNullOrEmpty(area))
            //    area = currentArea;

            if (String.IsNullOrEmpty(controller))
                controller = currentController;

            if (String.IsNullOrEmpty(action))
                action = currentAction;

            return controller == currentController && action == currentAction && area == currentArea ?
                cssClass : String.Empty;
        }
    }
}
