using Bridge.Data;
using Bridge.Domain.Recruiting;
using Bridge.Utility;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Web;
using Bridge.Web.Utility;

namespace Bridge.Web.Installers
{
    public class RepositoriesInstaller : IWindsorInstaller
    {
        public void Install( IWindsorContainer container, IConfigurationStore store ) {

            var configurationManager = new BridgeConfigurationManager();

            // this will handle all PR Repositories by convention
            var prConnectionString = configurationManager.GetConfigurationValue( "PRConnectionString" );
            container.Register( Classes
                .FromAssemblyContaining( typeof( PRRepositoryBase ) )
                .Where(t => t.BaseType == typeof(PRRepositoryBase) && t.IsPublic)
                .WithService.DefaultInterfaces()
                .Configure( c=> c.DependsOn( new { connectionString = prConnectionString } ) ) );

            var bridgeConnectionString = configurationManager.GetConfigurationValue("BridgeConnectionString");
            container.Register(Classes
                .FromAssemblyContaining(typeof(BridgeRepositoryBase))
                .Where(t => t.BaseType == typeof(BridgeRepositoryBase) && t.IsPublic)
                .WithService.DefaultInterfaces()
                .Configure(c => c.DependsOn(new { connectionString = bridgeConnectionString })));

            container.Register( Component
                .For<IScoringRulesRepository>()
                .ImplementedBy<ScoringRulesRepository>()
                .LifestyleTransient()
                .DependsOn(new { scoringConfigurationFile = HttpContext.Current.Server.MapPath(AppContext.CandidateScoringConfigFile) }));
        }
    }
}