using Bridge.Services.Recruiting;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;

namespace Bridge.Web.Installers
{
    public class ServicesInstaller : IWindsorInstaller
    {
        public void Install( IWindsorContainer container, IConfigurationStore store ) {

            container.Register( Classes.FromAssemblyNamed( "Bridge.Services" )
                .Pick()
                .WithService.DefaultInterfaces() );
        }
    }
}