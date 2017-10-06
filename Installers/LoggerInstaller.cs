using System;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Bridge.Web.Installers
{
    public class LoggerInstaller : IWindsorInstaller
    {
        public void Install( IWindsorContainer container, IConfigurationStore store ) {
            container.AddFacility<LoggingFacility>( f => f.UseNLog() );
        }
    }
}