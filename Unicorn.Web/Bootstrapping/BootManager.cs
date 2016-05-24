using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Extras.Quartz;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Unicorn.Web.Contracts;
using Unicorn.Web.Contracts.Messaging.ServiceBus;
using Unicorn.Web.Contracts.Processes;
using Unicorn.Web.Contracts.Services;
using Unicorn.Web.Core;
using Unicorn.Web.Core.Jobs;
using Unicorn.Web.Core.Messaging.ServciceBus;
using Unicorn.Web.Core.Processes;
using Unicorn.Web.Core.Services;
using Unicorn.Web.WebHooks;

namespace Unicorn.Web.Bootstrapping
{
    public class BootManager
    {

        private static IContainer _container;
        private List<IProcessor> _processors;

        public BootManager()
        {
            _container = BuildContainer();
        }

        /// <summary>
        /// Exposes the DI Container
        /// </summary>
        public IContainer Container
        {
            get { return _container ?? BuildContainer(); }
        }
        /// <summary>
        /// When the application ends the processors needs to be stopped
        /// </summary>
        public void OnApplicationEnd()
        {
            _processors.ForEach(p => p.Stop());
            //LogHelper.Info<BootManager>("Application ended so all processors were stopped.");
        }

        private IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<SlackWebHookHandler>().As<IWebHookHandler>();

            //Register the MVC controllers
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            //Register the Web API controllers
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            //Register the SignalR hubs
            builder.RegisterHubs(Assembly.GetExecutingAssembly());

            // Get settings from config
            var serviceBusConnectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            var discoQueueName = ConfigurationManager.AppSettings["DiscoTopicName"];
            var discoTriggers = ConfigurationManager.AppSettings["DiscoTriggers"];

            builder.Register(context => new GlobalSettings(serviceBusConnectionString, discoQueueName, discoTriggers)).As<IGlobalSettings>();
            builder.RegisterType<TopicService>().As<ITopicService>();

            //ServiceBus NamespaceManager and MessagingFactory
            builder.RegisterInstance(MessagingFactory.CreateFromConnectionString(serviceBusConnectionString)).As<MessagingFactory>();
            builder.RegisterInstance(NamespaceManager.CreateFromConnectionString(serviceBusConnectionString)).As<NamespaceManager>();
            builder.RegisterType<MessageWrapper>().As<IMessageWrapper>().SingleInstance();

            // Processors
            builder.RegisterType<NotificationProcessor>().As<IProcessor>().SingleInstance();
            builder.RegisterType<DiscoProcessor>().As<IProcessor>().SingleInstance();

            builder.RegisterModule(new QuartzAutofacFactoryModule());
            builder.RegisterModule(new QuartzAutofacJobsModule(typeof(StopDiscoJob).Assembly));


            _container = builder.Build();
            return _container;
        }

        public void PostBootHandlerRegistration()
        {
            _processors = DependencyResolver.Current.GetServices<IProcessor>().ToList();
            _processors.ForEach(p => p.Start());

            //LogHelper.Info<BootManager>(string.Format("Number of started IProcessors: {0}", _processors.Count));
        }
    }
}
