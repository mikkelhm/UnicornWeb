using System.Threading.Tasks;
using Unicorn.Web.Contracts.Models;

namespace Unicorn.Web.Contracts.Services
{
    public interface ITopicService
    {
        Task AddMessageToTopic(TopicMessageModel message);
    }
}
