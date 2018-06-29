using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;

namespace Defra.CustMaster.Identity.CoreApp.Dynamics
{
    public class TokenFactory : ITokenFactory
    {
        #region Fields

        IConfiguration _iconfig;


        private readonly string _clientId;


        private string _resource;

        public string _token = string.Empty;

        private DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;


        private readonly string _clientSecret;//= CloudConfigurationManager.GetSetting("Crm.Entities.ClientSecret");

        //private object _lockObject;

        //private ILoggingFactory _loggingFactory;

        //private ILogging _logger;

        public TokenFactory(IConfiguration iConfig)//, ILoggingFactory loggingFactory)
        {
            _iconfig = iConfig;

            _clientId = _iconfig.GetValue<string>("AppSettings:DynamicsApiClientId");
            _resource = _iconfig.GetValue<string>("AppSettings:DynamicsApiBaseAddress");
            _clientSecret = _iconfig.GetValue<string>("AppSettings:DynamicsApiClientSecret");
        }

        #endregion

        #region Interface Implementations

        public string GetToken()
        {
            if (!string.IsNullOrWhiteSpace(this._token) && this._tokenExpiry > DateTimeOffset.Now.AddMinutes(5))
            {

                return this._token;
            }

            //lock (this._lockObject)
            {
                if (!string.IsNullOrWhiteSpace(this._token) && this._tokenExpiry > DateTimeOffset.Now.AddMinutes(5))
                {
                    return this._token;
                }

                if (string.IsNullOrWhiteSpace(this._token) || (!string.IsNullOrWhiteSpace(this._token) && this._tokenExpiry < DateTimeOffset.Now.AddMinutes(5)))
                {
                    AuthenticationParameters app = AuthenticationParameters.CreateFromResourceUrlAsync(
              new Uri(_resource)).Result;
                    var authContext = new AuthenticationContext(app.Authority, false);// (this._authority, false);
                    var cred = new ClientCredential(_clientId, _clientSecret); //new UserCredential(_username, this._password);
                    var result = authContext.AcquireTokenAsync(app.Resource, cred).Result;// authContext.AcquireTokenAsync(_resource, cred).Result;//.AcquireToken(this._resource, this._clientId, cred);

                    this._tokenExpiry = result.ExpiresOn;//DateTimeOffset.Now.AddHours(1);//
                    this._token = result.AccessToken;// "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImlCakwxUmNxemhpeTRmcHhJeGRacW9oTTJZayIsImtpZCI6ImlCakwxUmNxemhpeTRmcHhJeGRacW9oTTJZayJ9.eyJhdWQiOiJodHRwczovL2RlZnJhLWN1c3RtYXN0LWRldi5jcm00LmR5bmFtaWNzLmNvbS8iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC82ZjUwNDExMy02YjY0LTQzZjItYWRlOS0yNDJlMDU3ODAwMDcvIiwiaWF0IjoxNTI3NzcxOTM4LCJuYmYiOjE1Mjc3NzE5MzgsImV4cCI6MTUyNzc3NTgzOCwiYWNyIjoiMSIsImFpbyI6IlkyZGdZTGovaERQVFNqWWl1dWQ0VWZ2bFA4ZlpaalN0L0xGb1o1Z1NxL0oxaTd2NmYvVUIiLCJhbXIiOlsicHdkIl0sImFwcGlkIjoiMTgwMWQ4M2ItNTRmZi00ZjYzLWJjZmUtOTEzM2E0ZTE2OGZjIiwiYXBwaWRhY3IiOiIwIiwiZV9leHAiOjI2MjgwMCwiZmFtaWx5X25hbWUiOiJSYW1pZGkiLCJnaXZlbl9uYW1lIjoiQXJ1bmEiLCJpcGFkZHIiOiIyLjEyNS4xNzcuMjE3IiwibmFtZSI6IkFydW5hIFJhbWlkaSIsIm9pZCI6ImJjNzFlMzE1LWNiNzgtNGI1ZC1hODFhLTU0OWFjYTFmNmQyMiIsInB1aWQiOiIxMDAzQkZGREFBNUFFRjg4Iiwic2NwIjoidXNlcl9pbXBlcnNvbmF0aW9uIiwic3ViIjoiakk5TzFnU1pCYzN1ZVNUOVR4d1g4bDRiWEcyc3YyazdiOGlxNGZscVhHZyIsInRpZCI6IjZmNTA0MTEzLTZiNjQtNDNmMi1hZGU5LTI0MmUwNTc4MDAwNyIsInVuaXF1ZV9uYW1lIjoiYXJ1bmEucmFtaWRpQGRlZnJhZGV2Lm9ubWljcm9zb2Z0LmNvbSIsInVwbiI6ImFydW5hLnJhbWlkaUBkZWZyYWRldi5vbm1pY3Jvc29mdC5jb20iLCJ1dGkiOiJ6WGNhekw3Z2FVQzlZT2R1b0pnR0FBIiwidmVyIjoiMS4wIiwid2lkcyI6WyI0NDM2NzE2My1lYmExLTQ0YzMtOThhZi1mNTc4Nzg3OWY5NmEiXX0.oKkGh8IYmwGovPtNI8Bxd7Duh6UUekyNkUrAlrDZ-GVCxruz_X9CrBaDJvqj9DuSdJ6c8XBP_9oXUEQReajt4pTAUGbAaSGuGabuh930027ucOpqWKNaF2O7-YCla7tqDPcjkB25x0eynMhj7XfvO6NsERxrtWS4QyjLB_t6pBRHBVT8voVdYOe3QprT5ILO6gkkAZ3scv3cOrRKa8S5HRdOXMyVj4HHpwSjR_zOT42rGr5hjUBo_vF3bwhS9zKe52g2wkvzLjZEnyS0Ob6Bc-wHcw-WFThxAVmrezsF5WGO911eFAkOGY4zWFgzD4atkcLynab6I6mbKuaD04RJHA";//;
                    this._tokenExpiry = DateTimeOffset.Now.AddHours(1);//
                    //this._token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImlCakwxUmNxemhpeTRmcHhJeGRacW9oTTJZayIsImtpZCI6ImlCakwxUmNxemhpeTRmcHhJeGRacW9oTTJZayJ9.eyJhdWQiOiJodHRwczovL2RlZnJhLWN1c3RtYXN0LWRldi5jcm00LmR5bmFtaWNzLmNvbS8iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC82ZjUwNDExMy02YjY0LTQzZjItYWRlOS0yNDJlMDU3ODAwMDcvIiwiaWF0IjoxNTI4MDk4NjM2LCJuYmYiOjE1MjgwOTg2MzYsImV4cCI6MTUyODEwMjUzNiwiYWNyIjoiMSIsImFpbyI6IlkyZGdZRGdYZVVsU3dHYW5ZVlJweDU5QTlXY1RHSTY3ekY1aG9KdkpVaEhBL0dpMjcyY0EiLCJhbXIiOlsicHdkIl0sImFwcGlkIjoiMTgwMWQ4M2ItNTRmZi00ZjYzLWJjZmUtOTEzM2E0ZTE2OGZjIiwiYXBwaWRhY3IiOiIwIiwiZV9leHAiOjI2MjgwMCwiZ2l2ZW5fbmFtZSI6IlNBLUVVRS1TVkMiLCJpcGFkZHIiOiI4OS4xNDUuMjAwLjIwMSIsIm5hbWUiOiJTQS1FVUUtU1ZDIiwib2lkIjoiYWI4MGJmY2YtOGNmNy00ZjcwLThkZWQtM2JlYjJkM2MxOWZjIiwicHVpZCI6IjEwMDMzRkZGQUE5Rjk2MjEiLCJzY3AiOiJ1c2VyX2ltcGVyc29uYXRpb24iLCJzdWIiOiJINzl5bnpRaXliNk9RTUxnMmgtNVd1U1hzeGExNTFYVVdWai1hZUhVVXVnIiwidGlkIjoiNmY1MDQxMTMtNmI2NC00M2YyLWFkZTktMjQyZTA1NzgwMDA3IiwidW5pcXVlX25hbWUiOiJTQS1FVUUtU1ZDQGRlZnJhZGV2Lm9ubWljcm9zb2Z0LmNvbSIsInVwbiI6IlNBLUVVRS1TVkNAZGVmcmFkZXYub25taWNyb3NvZnQuY29tIiwidXRpIjoiUUpmeWJlZmdJa0NlVGhUMnh5MGlBQSIsInZlciI6IjEuMCJ9.id5dF8ZLF_blSMQRJSnf8OjTi_ljhkRILjo-14LIEEwE9wgnwlDHV89pXK1hlEQ3Nd2ug_LzmCcurq4QqYKpiBg1mMOpR9YqvYv2dbDNEsQhTsAIDpw02pIavDcIs5AbVGHJbAyVidkKKypqwESd_ovQfHu3IESqCN1dDEhIKEn-t8BQDyr3ZDPzmLe9pHSjyyZ537aKph-Dw_XgwUaIQ2ilnbUZHWMZ6qkj7tkQVaqMKHvIB4togwm-b6YWjHLbfscdUoPFe3nhvRK8t5aMtg2f9zdFPlwsrU7xrxcLhYV8_qN36MMBJSt0l1KjdMOf92nkvguvPmqCM00HAeyTxw";//;

                    //this._logger.LogAudit($"Dynamics authentication token refreshed. Token expires at: {this._tokenExpiry} Token: {this._token}");
                }

                return this._token;
            }
        }

        public void InvalidateToken()
        {
            this._tokenExpiry = DateTime.Now;
        }

        #endregion
    }
}