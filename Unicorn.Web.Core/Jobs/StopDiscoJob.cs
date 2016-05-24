using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Quartz;
using Unicorn.Web.Core.Messaging.Hubs;

namespace Unicorn.Web.Core.Jobs
{
    public class StopDiscoJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            //var context = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
            //context.Clients.All.stopDisco();
        }
    }
}
