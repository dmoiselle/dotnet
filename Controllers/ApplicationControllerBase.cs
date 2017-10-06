using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace Bridge.Web.Controllers
{
    public class ApplicationControllerBase : Controller
    {
        public ILogger Logger { get; set; }

        protected override void OnException( ExceptionContext filterContext ) {
            if ( filterContext.HttpContext.IsDebuggingEnabled == false ) {
                filterContext.ExceptionHandled = true;
            }

            Logger.Error( filterContext.Exception.Message, filterContext.Exception );

            if ( filterContext.Exception is HttpAntiForgeryException ) {
                filterContext.Result = this.RedirectToAction( "LogOn", "Account" );
            }
            else {
                filterContext.Result = this.View( "~/Views/Shared/Error.cshtml" );
            }
            base.OnException( filterContext );
        }
    }
}