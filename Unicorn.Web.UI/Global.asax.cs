using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Unicorn.Web.Bootstrapping;

namespace Unicorn.Web.UI
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            var boot = new BootManager();
            var container = boot.Container;
            var resolver = new AutofacDependencyResolver(container);
            DependencyResolver.SetResolver(resolver);

            // Create the WebApi depenedency resolver
            var webApiResolver = new AutofacWebApiDependencyResolver(container);

            // Configure Web API with the dependency resolver.
            GlobalConfiguration.Configuration.DependencyResolver = webApiResolver;

            GlobalConfiguration.Configure(WebApiConfig.Register);
            boot.PostBootHandlerRegistration();

        }
    }
}