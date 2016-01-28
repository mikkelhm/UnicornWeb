using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Unicorn.Web.Contracts.Models;
using Unicorn.Web.Contracts.Services;
using Unicorn.Web.Models;

namespace Unicorn.Web.Controllers
{
    public class QueueController : ApiController
    {
        private readonly ITopicService _topicService;

        public QueueController(ITopicService topicService)
        {
            _topicService = topicService;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add(MessageModel model)
        {
            await _topicService.AddMessageToTopic(new TopicMessageModel() { Message = model.Message, Sender = "api" });
            return Request.CreateResponse(HttpStatusCode.OK, new { model.Message });
        }

    }
}
