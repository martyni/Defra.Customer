using Defra.CustomerMaster.Identity.Api.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Defra.CustomerMaster.Identity.Api.Dynamics
{
    public class CrmApiWrapper : ICrmApiWrapper
    {
        IClientFactory _iHttpClient;
        HttpClient _httpClient;

        public CrmApiWrapper(IClientFactory iHttpClientFactory)
        {
            _iHttpClient = iHttpClientFactory;
            _httpClient = _iHttpClient.GetHttpClient();

        }

        /// <summary>
        /// https://defra-custmstr-sanbox.api.crm4.dynamics.com/api/data/v8.2/contacts?$select=contactid,defra_uniquereference&$filter=defra_b2cobjectid eq '7b1ad2d0-7946-11e8-8d36-851e870eee8a'
        /// </summary>
        /// <param name="b2cObjectId"></param>
        /// <returns></returns>
        public ServiceObject InitialMatch(string b2cObjectId)
        {

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "contacts?$select=contactid,defra_uniquereference&$filter=defra_b2cobjectid eq '" + b2cObjectId + "'");

            //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var content = _httpClient.SendAsync(request).Result;

            if (!content.IsSuccessStatusCode)
            {
                throw new WebFaultException(content.ReasonPhrase, (int)content.StatusCode);
            }
            InitialMatchResponse contactResponse = JsonConvert.DeserializeObject<InitialMatchResponse>(content.Content.ReadAsStringAsync().Result);

            ServiceObject returnObj = new ServiceObject() { ErrorCode = 200 };
            if (contactResponse != null && contactResponse.value != null && contactResponse.value.Count > 0)
            {
                if (contactResponse.value.Count == 1)
                {
                    returnObj.ServiceUserID = contactResponse.value[0].ServiceUserID;
                    returnObj.UniqueReferenceId = contactResponse.value[0].UniqueReferenceId;
                }
                else
                {
                    returnObj.ErrorCode = 412;
                    returnObj.ErrorMsg = "Multiple records found";
                }
            }
            else
            {
                returnObj.ErrorCode = 204;
                returnObj.ErrorMsg = "No Content";
            }

            return returnObj;
        }




        /// <summary>
        /// https://defra-custmstr-sanbox.api.crm4.dynamics.com/api/data/v8.2/defra_lobserviceuserlinks?fetchXml=%3Cfetch%20version=%271.0%27%20output-format=%27xml-platform%27%20mapping=%27logical%27%20distinct=%27false%27%3E%3Centity%20name=%27defra_lobserviceuserlink%27%3E%3Cattribute%20name=%27defra_lobserviceuserlinkid%27/%3E%3Cattribute%20name=%27defra_name%27/%3E%3Cattribute%20name=%27createdon%27/%3E%3Cattribute%20name=%27defra_serviceuser%27/%3E%3Cattribute%20name=%27defra_servicerole%27/%3E%3Corder%20attribute=%27defra_name%27%20descending=%27false%27/%3E%3Cfilter%20type=%27and%27%3E%3Ccondition%20attribute=%27statecode%27%20operator=%27eq%27%20value=%270%27/%3E%3C/filter%3E%3Clink-entity%20name=%27contact%27%20from=%27contactid%27%20to=%27defra_serviceuser%27%20link-type=%27inner%27%20alias=%27serviceLinkContact%27%3E%3Cattribute%20name=%27fullname%27/%3E%3Cfilter%20type=%27and%27%3E%3Ccondition%20attribute=%27defra_b2cobjectid%27%20operator=%27eq%27%20value=%277b1ad2d0-7946-11e8-8d36-851e870eee8a%27/%3E%3C/filter%3E%3C/link-entity%3E%3Clink-entity%20name=%27defra_lobserivcerole%27%20from=%27defra_lobserivceroleid%27%20to=%27defra_servicerole%27%20link-type=%27inner%27%20alias=%27serviceLinkRole%27%3E%3Cattribute%20name=%27defra_rolename%27/%3E%3Cattribute%20name=%27defra_name%27%20/%3Eattribute%20name=%27defra_lobserivceroleid%27/%3E%3Cfilter%20type=%27and%27%3E%3Ccondition%20attribute=%27defra_lobservice%27%20operator=%27eq%27%20uitype=%27defra_lobservice%27%20value=%27{534BA555-037A-E811-A95B-000D3A2BC547}%27/%3E%3C/filter%3E%3C/link-entity%3E%3Clink-entity%20name=%27account%27%20from=%27accountid%27%20to=%27defra_organisation%27%20visible=%27false%27%20link-type=%27outer%27%20alias=%27serviceLinkOrganisation%27%3E%3Cattribute%20name=%27name%27/%3E%3Cattribute%20name=%27accountid%27/%3E%3C/link-entity%3E%3C/entity%3E%3C/fetch%3E
        /// 
        /// https://defra-custmstr-sanbox.api.crm4.dynamics.com/api/data/v8.2/defra_lobserviceuserlinks?fetchXml=<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'><entity name='defra_lobserviceuserlink'><attribute name='defra_lobserviceuserlinkid'/><attribute name='defra_name'/><attribute name='createdon'/><attribute name='defra_serviceuser'/><attribute name='defra_servicerole'/><order attribute='defra_name' descending='false'/><filter type='and'><condition attribute='statecode' operator='eq' value='0'/></filter><link-entity name='contact' from='contactid' to='defra_serviceuser' link-type='inner' alias='serviceLinkContact'><attribute name='fullname'/><filter type='and'><condition attribute='defra_b2cobjectid' operator='eq' value='7b1ad2d0-7946-11e8-8d36-851e870eee8a'/></filter></link-entity><link-entity name='defra_lobserivcerole' from='defra_lobserivceroleid' to='defra_servicerole' link-type='inner' alias='serviceLinkRole'><attribute name='defra_rolename'/><attribute name='defra_name' />attribute name='defra_lobserivceroleid'/><filter type='and'><condition attribute='defra_lobservice' operator='eq' uitype='defra_lobservice' value='{534BA555-037A-E811-A95B-000D3A2BC547}'/></filter></link-entity><link-entity name='account' from='accountid' to='defra_organisation' visible='false' link-type='outer' alias='serviceLinkOrganisation'><attribute name='name'/><attribute name='accountid'/></link-entity></entity></fetch>
        /// </summary>
        /// <param name="ServiceID">{534BA555-037A-E811-A95B-000D3A2BC547}</param>
        /// <param name="b2cObjectId">7b1ad2d0-7946-11e8-8d36-851e870eee8a</param>
        /// <returns></returns>
        public ServiceUserLinks Authz(string serviceID, string b2cObjectId)
        {

            string fetchXmlRequest = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'><entity name='defra_lobserviceuserlink'>" +
              "<attribute name='defra_lobserviceuserlinkid'/><attribute name='defra_name'/>" +
              "<attribute name='createdon'/><attribute name='defra_serviceuser'/><attribute name='defra_servicerole'/>" +
              "<order attribute='defra_name' descending='false'/><filter type='and'><condition attribute='statecode' operator='eq' value='0'/></filter>" +
              "<link-entity name='contact' from='contactid' to='defra_serviceuser' link-type='inner' alias='serviceLinkContact'>" +
              "<attribute name='fullname'/><filter type='and'><condition attribute='defra_b2cobjectid' operator='eq' value='" + b2cObjectId + "'/></filter>" +
              "</link-entity><link-entity name='defra_lobserivcerole' from='defra_lobserivceroleid' to='defra_servicerole' link-type='inner' alias='serviceLinkRole'>" +
              "<attribute name='defra_rolename'/><attribute name='defra_name'/><attribute name='defra_lobserivceroleid'/><filter type='and'>" +
              "<condition attribute='defra_lobservice' operator='eq' uitype='defra_lobservice' value='{" + serviceID + "}'/>" +
              "</filter></link-entity><link-entity name='account' from='accountid' to='defra_organisation' visible='false' link-type='outer' alias='serviceLinkOrganisation'>" +
              "<attribute name='name'/><attribute name='accountid'/></link-entity></entity></fetch>";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "defra_lobserviceuserlinks?fetchXml=" + fetchXmlRequest);

            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _resource + "api/data/v8.2/defra_InitialMatch");
            //request.Content = new StringContent(paramsContent);
            //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var content = _httpClient.SendAsync(request).Result;

            if (!content.IsSuccessStatusCode)
            {
                throw new WebFaultException(content.ReasonPhrase, (int)content.StatusCode);
            }


            ServiceUserLinks contentResponse = JsonConvert.DeserializeObject<ServiceUserLinks>(content.Content.ReadAsStringAsync().Result);
            return contentResponse;

        }




        public ContactModel CreateContact(ContactModel contact)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "defra_CreateContact");
            //return new ServiceObject { ServiceUserID = UPN };

            JObject exeAction = new JObject();
            if (contact.b2cobjectid != null)
                exeAction["defra_b2cobjectid"] = contact.b2cobjectid;
            if (contact.firstname != null)
                exeAction["firstname"] = contact.firstname;
            if (contact.lastname != null)
                exeAction["lastname"] = contact.lastname;
            if (contact.emailaddress1 != null)
                exeAction["emailaddress1"] = contact.emailaddress1;

            string paramsContent;
            if (exeAction.GetType().Name.Equals("JObject"))
            { paramsContent = exeAction.ToString(); }
            else
            {
                paramsContent = JsonConvert.SerializeObject(exeAction, new JsonSerializerSettings()
                { DefaultValueHandling = DefaultValueHandling.Ignore });
            }
            request.Content = new StringContent(paramsContent);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var content = _httpClient.SendAsync(request).Result;

            if (!content.IsSuccessStatusCode)
            {
                throw new WebFaultException(content.ReasonPhrase, (int)content.StatusCode);
            }
            ContactModel contactResponse = JsonConvert.DeserializeObject<ContactModel>(content.Content.ReadAsStringAsync().Result);
            //ServiceObject returnObj = new ServiceObject() { ServiceUserID = contactResponse.contactid, ErrorCode = (int)contactResponse.HttpStatusCode, ErrorMsg = contactResponse.Message };
            return contactResponse;
        }

        //public ServiceObject UserInfo(ContactModel contact)
        //{

        //}
    }
}