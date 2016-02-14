using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Unicorn.Web.Core.Messaging.Hubs
{
    [HubName("messageHub")]
    public class MessageHub : Hub
    {
        public void SendMessage(string from, string message)
        {
            Clients.All.addMessage(from, message);
        }
    }
}
