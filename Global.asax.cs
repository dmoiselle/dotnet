using Castle.Windsor;
using Castle.Windsor.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Bridge.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters( GlobalFilterCollection filters ) {
            filters.Add( new HandleErrorAttribute() );
        }

        public static void RegisterRoutes( RouteCollection routes ) {
            routes.IgnoreRoute( "{resource}.axd/{*pathInfo}" );

            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters( GlobalFilters.Filters );
            RegisterRoutes( RouteTable.Routes );

            BootstrapWindsorContainer();
        }

        protected void Application_End() {
            windsorContainer.Dispose();
        }

        private static void BootstrapWindsorContainer() {
            windsorContainer = new WindsorContainer().Install( FromAssembly.This() );
            var controllerFactory = new WindsorControllerFactory( windsorContainer.Kernel );
            ControllerBuilder.Current.SetControllerFactory( controllerFactory );
        }

        private static IWindsorContainer windsorContainer;

        public static readonly Version CurrentVersion = Assembly.GetAssembly( typeof( MvcApplication ) ).GetName().Version;
    }
}