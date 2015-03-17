using System;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Infestation.VirtualParadise.Configuration;

namespace Infestation.VirtualParadise.Web
{
    public class Server : IDisposable
    {
        private readonly HttpSelfHostServer _server;
        public Server(string baseAddress)
        {
            var config = new HttpSelfHostConfiguration(baseAddress);
           
            config.EnableCors();
            config.Routes.MapHttpRoute("API Default", "api/{controller}/{id}", new { id = RouteParameter.Optional });
           
            _server = new HttpSelfHostServer(config);
        }

        public void Start()
        {
            _server.OpenAsync().Wait();
        }

        public void Stop()
        {
            _server.CloseAsync();
        }

        public void Dispose()
        {
            if(_server != null)
                _server.Dispose();
        }
    }
}
