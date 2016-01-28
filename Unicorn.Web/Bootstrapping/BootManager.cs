using System.Configuration;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Unicorn.Web.Contracts;
using Unicorn.Web.Contracts.Services;
using Unicorn.Web.Core;
using Unicorn.Web.Core.Services;
using Unicorn.Web.WebHooks;

namespace Unicorn.Web.Bootstrapping
{
    public class BootManager
    {

        private static IContainer _container;
        public BootManager()
        {
        }

        /// <summary>
        /// Exposes the DI Container
        /// </summary>
        public IContainer Container
        {
            get { return _container ?? BuildContainer(); }
        }


        private IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<SlackWebHookHandler>().As<IWebHookHandler>();

            //Register the MVC controllers
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            //Register the Web API controllers
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // Get settings from config
            var serviceBusConnectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            var discoQueueName = ConfigurationManager.AppSettings["DiscoTopicName"];
            var discoTriggers = ConfigurationManager.AppSettings["DiscoTriggers"];

            builder.Register(context => new GlobalSettings(serviceBusConnectionString, discoQueueName, discoTriggers)).As<IGlobalSettings>();
            builder.RegisterType<TopicService>().As<ITopicService>();

            _container = builder.Build();
            return _container;
        }
    }
}
