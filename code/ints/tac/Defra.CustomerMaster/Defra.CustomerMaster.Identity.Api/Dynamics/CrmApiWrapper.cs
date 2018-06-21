using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using Newtonsoft.Json;
using System.Configuration;
using Newtonsoft.Json.Linq;
using Defra.CustomerMaster.Identity.Api.Model;

namespace Defra.CustomerMaster.Identity.Api.Dynamics
{
    public class CrmApiWrapper
    {
        private HttpClient httpClient;
        private const string Authority = "https://login.windows.net/common";
        private readonly string _clientId;//= "d38f2cff-29d1-43f6-a3e8-4b91baf4b065";
        private readonly string _password;//= "Yizh1TCPXsOMvXbC8Duu";
        private readonly string _resource;// = "https://defra-custmast-dev.api.crm4.dynamics.com";
        private readonly string _username;

        /// <summary>
        /// Constructor
        /// </summary>
        public CrmApiWrapper()
        {

            if (!ConfigurationManager.AppSettings.AllKeys.ToList().Contains("DynamicsApiTokenUrl"))
            {
                throw new ApplicationException("There was no configuration item with the name DynamicsApiTokenUrl in the config");
            }
            if (!ConfigurationManager.AppSettings.AllKeys.ToList().Contains("DynamicsApiClientId"))
            {
                throw new ApplicationException("There was no configuration item with the name DynamicsApiClientId in the config");
            }
            if (!ConfigurationManager.AppSettings.AllKeys.ToList().Contains("DynamicsApiPassword"))
            {
                throw new ApplicationException("There was no configuration item with the name DynamicsApiPassword in the config");
            }
            if (!ConfigurationManager.AppSettings.AllKeys.ToList().Contains("DynamicsApiUsername"))
            {
                throw new ApplicationException("There was no configuration item with the name DynamicsApiUsername in the config");
            }
            _resource = ConfigurationManager.AppSettings["DynamicsApiTokenUrl"];
            _clientId = ConfigurationManager.AppSettings["DynamicsApiClientId"];
            _password = ConfigurationManager.AppSettings["DynamicsApiPassword"];
            _username = ConfigurationManager.AppSettings["DynamicsApiUsername"];
        }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="UPN"></param>
      /// <returns></returns>

        public Contact InitialMatch(string UPN)
        {
            JObject exeAction = new JObject();
            exeAction["UPN"] = UPN;

            string paramsContent;
            if (exeAction.GetType().Name.Equals("JObject"))
            { paramsContent = exeAction.ToString(); }
            else
            {
                paramsContent = JsonConvert.SerializeObject(exeAction, new JsonSerializerSettings()
                { DefaultValueHandling = DefaultValueHandling.Ignore });
            }
            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _resource+ "api/data/v8.2/contacts?$select=contactid&$filter=defra_upn eq '"+UPN+"'");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _resource + "api/data/v8.2/defra_InitialMatch");
            request.Content = new StringContent(paramsContent);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            HttpResponseMessage responseContent = ConnectToCRM(request);

            var content = responseContent.Content.ReadAsStringAsync().Result;

            Contact contactResponse = JsonConvert.DeserializeObject<Contact>(content);
            contactResponse.HttpStatusCode = contactResponse.Code == 0 ? responseContent.StatusCode : System.Net.HttpStatusCode.BadRequest;

            return contactResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
       public Contact UserInfo(Contact contact)
        {        
            JObject exeAction = new JObject();
            exeAction["firstname"] = contact.firstname;
            exeAction["lastname"] = contact.lastname;
            exeAction["emailid"] = contact.emailid;
            exeAction["UPN"] = contact.UPN;

            string paramsContent;
            if (exeAction.GetType().Name.Equals("JObject"))
            { paramsContent = exeAction.ToString(); }
            else
            {
                paramsContent = JsonConvert.SerializeObject(exeAction, new JsonSerializerSettings()
                { DefaultValueHandling = DefaultValueHandling.Ignore });
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _resource + "api/data/v8.2/defra_UpsertContact");
            request.Content = new StringContent(paramsContent);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            HttpResponseMessage responseContent=ConnectToCRM(request);

            var content = responseContent.Content.ReadAsStringAsync().Result;

            Contact contactResponse = JsonConvert.DeserializeObject<Contact>(content);
            contactResponse.HttpStatusCode = contactResponse.Code == 0 ? responseContent.StatusCode : System.Net.HttpStatusCode.BadRequest;

            return contactResponse;


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ServiceID"></param>
        /// <param name="UPN"></param>
        /// <returns></returns>
        public ServiceUserLinks Authz(string ServiceID, string UPN)
        {        

            string fetchXmlRequest = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'><entity name='defra_lobserviceuserlink'>" +
              "<attribute name='defra_lobserviceuserlinkid'/><attribute name='defra_name'/>" +
              "<attribute name='createdon'/><attribute name='defra_serviceuser'/><attribute name='defra_servicerole'/>" +
              "<order attribute='defra_name' descending='false'/><filter type='and'><condition attribute='statecode' operator='eq' value='0'/></filter>" +
              "<link-entity name='contact' from='contactid' to='defra_serviceuser' link-type='inner' alias='serviceLinkContact'>" +
              "<attribute name='fullname'/><filter type='and'><condition attribute='defra_upn' operator='eq' value='" + UPN + "'/></filter>" +
              "</link-entity><link-entity name='defra_lobserivcerole' from='defra_lobserivceroleid' to='defra_servicerole' link-type='inner' alias='serviceLinkRole'>" +
              "<attribute name='defra_rolename'/><attribute name='defra_name'/><attribute name='defra_lobserivceroleid'/><filter type='and'>" +
              "<condition attribute='defra_lobservice' operator='eq' uitype='defra_lobservice' value='{" + ServiceID + "}'/>" +
              "</filter></link-entity><link-entity name='account' from='accountid' to='defra_organisation' visible='false' link-type='outer' alias='serviceLinkOrganisation'>"+
              "<attribute name='name'/><attribute name='accountid'/></link-entity></entity></fetch>";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _resource + "api/data/v8.2/defra_lobserviceuserlinks?fetchXml="+fetchXmlRequest);          
            HttpResponseMessage responseContent = ConnectToCRM(request);
            var content = responseContent.Content.ReadAsStringAsync().Result;
            ServiceUserLinks contentResponse = JsonConvert.DeserializeObject<ServiceUserLinks>(content);
            return contentResponse;
        }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="request"></param>
            /// <returns></returns>
            private HttpResponseMessage ConnectToCRM(HttpRequestMessage request)
        {
            System.Diagnostics.Trace.TraceInformation("Entered ConnectToCRM Method");
            Configuration config = new Configuration();
            config.Username = _username;


            SecureString Password = new SecureString();
            foreach (char c in _password) Password.AppendChar(c);

            config.Password = Password;
            config.ClientId = _clientId;
            config.RedirectUrl = Authority;
            config.ServiceUrl = _resource;
            Authentication auth = new Authentication(config);//, Authority);  

            httpClient = new HttpClient(auth.ClientHandler, true);
           
            httpClient.Timeout = new TimeSpan(0, 2, 0);
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            //httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {auth.AcquireToken().AccessToken}");

            var contactResponse = httpClient.SendAsync(request).Result;
            
            if (!contactResponse.IsSuccessStatusCode)
            {
                throw new WebFaultException(contactResponse.ReasonPhrase, (int)contactResponse.StatusCode);
            }
            return contactResponse;
        }
    }

}
