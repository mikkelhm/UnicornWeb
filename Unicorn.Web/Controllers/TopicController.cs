using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Unicorn.Web.Contracts.Models;
using Unicorn.Web.Contracts.Services;
using Unicorn.Web.Core.Messaging.Hubs;
using Unicorn.Web.Models;

namespace Unicorn.Web.Controllers
{
    public class TopicController : ApiController
    {
        private readonly ITopicService _topicService;

        public TopicController(ITopicService topicService)
        {
            _topicService = topicService;
        }

        [HttpGet]
        public string AddMessage()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
            context.Clients.All.addMessage("api", Guid.NewGuid());
            return "OK";
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add(MessageModel model)
        {
            await _topicService.AddMessageToTopic(new TopicMessageModel() { Message = model.Message, Disco = model.Disco, Sender = "api" });
            return Request.CreateResponse(HttpStatusCode.OK, new { model.Message });
        }

    }
}
