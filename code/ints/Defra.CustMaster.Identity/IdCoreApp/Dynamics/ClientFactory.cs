using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
namespace Defra.CustomerMaster.Identity.Api.Dynamics
{  
    public class HttpClientFactory : IClientFactory
    {
        
        private readonly ITokenFactory _iTokenFactory;
        IConfiguration iconfig;

        private string _baseAddress = "";

        public HttpClientFactory(IConfiguration iConfig,ITokenFactory tokenFactory)
        {
            iconfig = iConfig;
            _iTokenFactory = tokenFactory;
            _baseAddress= iconfig.GetValue<string>("AppSettings:DynamicsApiBaseAddress");// iconfig.GetValue<string>("AppSettings:DynamicsApiBaseAddress");
        }
        public HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();         
            
            httpClient.BaseAddress = new Uri(this._baseAddress);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.GetToken());
            //httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {GetToken()}");
            httpClient.Timeout = new TimeSpan(0, 2, 0);  // 2 minutes
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            return httpClient;
        }

        private string GetToken()
        {
            var token = _iTokenFactory.GetToken();
            return token;
        }
    }
}
