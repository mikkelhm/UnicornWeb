using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.SignalR;

namespace Unicorn.Web.Controllers
{
    public class TestController : ApiController
    {
        [HttpGet]
        public string Marco()
        {
            var urlParts = Request.RequestUri.AbsolutePath.Split('/');
            if (urlParts[urlParts.GetUpperBound(0)].StartsWith("M"))
                return "Polo";
            return "polo";
        }

    }
}
