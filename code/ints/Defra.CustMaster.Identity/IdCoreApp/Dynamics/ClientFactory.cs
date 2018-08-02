namespace Defra.CustMaster.Identity.CoreApp.Dynamics
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Microsoft.Extensions.Configuration;
    
    public class HttpClientFactory : IClientFactory
    {        
        private readonly ITokenFactory iTokenFactory;
        private IConfiguration iconfig;

        private string baseAddress = "";

        public HttpClientFactory(IConfiguration iConfig,ITokenFactory tokenFactory)
        {
            iconfig = iConfig;
            iTokenFactory = tokenFactory;
            baseAddress= iconfig.GetValue<string>("AppSettings:DynamicsApiBaseAddress");// iconfig.GetValue<string>("AppSettings:DynamicsApiBaseAddress");
        }

        public HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();    
            httpClient.BaseAddress = new Uri(this.baseAddress);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.GetToken());
            //httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {GetToken()}");
            httpClient.Timeout = new TimeSpan(0, 2, 0);  // 2 minutes
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Add("Prefer", "odata.include-annotations=OData.Community.Display.V1.FormattedValue");
            return httpClient;
        }

        private string GetToken()
        {
            var token = iTokenFactory.GetToken();
            return token;
        }
    }
}
