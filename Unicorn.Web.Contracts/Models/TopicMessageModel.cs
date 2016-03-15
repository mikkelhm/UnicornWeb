using System;

namespace Unicorn.Web.Contracts.Models
{
    public class TopicMessageModel
    {
        public TopicMessageModel()
        {
            RecievedDate = DateTime.UtcNow;
        }
        public DateTime RecievedDate { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public bool Disco { get; set; }
    }
}
