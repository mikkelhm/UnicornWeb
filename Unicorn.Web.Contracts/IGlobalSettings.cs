using System.Collections.Generic;

namespace Unicorn.Web.Contracts
{
    public interface IGlobalSettings
    {
        string ServiceBusConnectionString { get; }
        string DiscoTopicName { get; }
        List<string> DiscoTriggers { get; }
    }
}
