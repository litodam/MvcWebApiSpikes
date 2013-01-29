namespace MvcApplication8.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.SelfHost;

    public abstract class WebApiClassBase : IDisposable
    {
        private readonly string baseAddress;
        private readonly Type controllerType;

        private HttpSelfHostConfiguration configuration;
        private HttpSelfHostServer server;

        protected WebApiClassBase(Type controllerType) : this("localhost", 8080, controllerType)
        {
        }

        protected WebApiClassBase(string host, int port, Type controllerType)
        {
            this.controllerType = controllerType;
            if (string.IsNullOrEmpty(host))
            {
                host = "localhost";
            }

            this.baseAddress = string.Format("http://{0}:{1}", host, port);
        }

        public virtual HttpSelfHostConfiguration Configuration
        {
            get
            {
                if (this.configuration == null)
                {
                    this.configuration = new HttpSelfHostConfiguration(this.baseAddress);
                    this.configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
                    this.configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(this.controllerType));
                    this.configuration.Routes.MapHttpRoute(name: "DefaultApi", routeTemplate: "api/{controller}/{id}", defaults: new { id = RouteParameter.Optional });
                }

                return this.configuration;
            }
        }

        public virtual HttpSelfHostServer Server
        {
            get { return this.server ?? (this.server = new HttpSelfHostServer(this.Configuration)); }
        }

        public string BaseAddress
        {
            get { return this.baseAddress; }
        }

        public void Start()
        {
            this.Server.OpenAsync().Wait();
        }

        public void Close()
        {
            this.Server.CloseAsync().Wait();
        }

        public void Dispose()
        {
            if (this.configuration != null)
            {
                this.configuration.Dispose();
                this.configuration = null;
            }

            if (this.server != null)
            {
                this.server.Dispose();
                this.server = null;
            }
        }

        protected HttpResponseMessage CreateRequest(string url, HttpMethod method, string acceptedMediaType = null)
        {
            var request = new HttpRequestMessage();

            request.RequestUri = new Uri(this.baseAddress + url);

            if (acceptedMediaType != null)
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptedMediaType));
            }

            request.Method = method;

            var client = new HttpClient(this.Server);
            using (HttpResponseMessage response = client.SendAsync(request).Result)
            {
                return response;
            }
        }

        public class TestAssemblyResolver : IAssembliesResolver
        {
            private readonly Type controllerType;

            public TestAssemblyResolver(Type controllerType)
            {
                this.controllerType = controllerType;
            }

            public ICollection<Assembly> GetAssemblies()
            {
                List<Assembly> baseAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

                if (!baseAssemblies.Contains(this.controllerType.Assembly))
                {
                    baseAssemblies.Add(this.controllerType.Assembly);
                }

                return baseAssemblies;
            }
        }
    }
}