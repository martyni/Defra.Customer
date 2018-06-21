using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System.Threading;
using Newtonsoft.Json;
using Defra.CustomerMaster.Identity.Api;
using Defra.CustomerMaster.Identity.Api.Model;

namespace Defra.CustomerMaster.Identity.API.UnitTest
{
    [TestClass]
    public class EnrolmentAPITest
    {
        const string URL = "http://localhost:3409/EnrolmentAPI.svc/";//"https://defra-custmast-dev.crm4.dynamics.com/";
        string _serviceID = null;

        [TestMethod]
        public void InitialMatchTest()
        {            
            var client = new RestClient(URL);
            // var key = ApplicationAuthenticator.GetS2SAccessTokenForProdMSAAsync();
            var request = new RestRequest("InitialMatch/123", Method.GET);


            request.AddHeader("Accept", "*/*");
            request.AddHeader("Content-Type", "application/json");
            var action = new Action<IRestResponse<object>, RestRequestAsyncHandle>(CallbackResponse);


            var response = client.Get(request);
            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(response.Content))
            {
                string content = response.Content.Remove(0, 1);
                content = content.Remove(content.Length - 1, 1);
                content = content.Replace("\\", "");
                ServiceObject returnValue = JsonConvert.DeserializeObject<ServiceObject>(content);
                Assert.IsNotNull(returnValue.ServiceUserID);
             
            }


        }

        [TestMethod]
        public void UserInfoTestWithContactCreate()
        {
            var action = new Action<IRestResponse<object>, RestRequestAsyncHandle>(CallbackResponse);

            var client = new RestClient(URL);
            // var key = ApplicationAuthenticator.GetS2SAccessTokenForProdMSAAsync();
            var request = new RestRequest("UserInfo/", Method.PUT)
            {
                RequestFormat = DataFormat.Json

            };
            Contact contactContent = new Contact() {lastname = "UnitTestUser", emailid = "UnitTestUser@test.com", UPN= "UnitTestUser123" };
            //string contactJson = JsonConvert.SerializeObject(contactContent);

            request.AddBody(contactContent);
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Content-Type", "application/json");

            var response = client.Put(request);
            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(response.Content))
            {
                string content = response.Content.Remove(0, 1);
                content = content.Remove(content.Length - 1, 1).Replace("\\", "");

                ServiceObject returnValue = JsonConvert.DeserializeObject<ServiceObject>(content);
                Assert.IsNotNull(returnValue.ServiceUserID);
                _serviceID = returnValue.ServiceUserID;
                Assert.AreEqual(_serviceID, returnValue.ServiceUserID);
                
            }

        }

        [TestMethod]
        public void UserInfoTestWithContactUpdate()
        {            
            var action = new Action<IRestResponse<object>, RestRequestAsyncHandle>(CallbackResponse);

            var client = new RestClient(URL);
            // var key = ApplicationAuthenticator.GetS2SAccessTokenForProdMSAAsync();
            var request = new RestRequest("UserInfo/", Method.PUT)
            {
                RequestFormat = DataFormat.Json

            };
            Contact contactContent = new Contact() {  firstname="testfirstnameupdate",emailid = "UnitTestUser@test.com", UPN = "UnitTestUser123" };
            //string contactJson = JsonConvert.SerializeObject(contactContent);

            request.AddBody(contactContent);
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Content-Type", "application/json");

            var response = client.Put(request);
            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(response.Content))
            {
                string content = response.Content.Remove(0, 1);
                content = content.Remove(content.Length - 1, 1).Replace("\\", "");

                ServiceObject returnValue = JsonConvert.DeserializeObject<ServiceObject>(content);
                Assert.IsNotNull(returnValue.ServiceUserID);
                _serviceID = returnValue.ServiceUserID;
                Assert.AreEqual(_serviceID, returnValue.ServiceUserID);

            }

        }

        private void CallbackResponse(IRestResponse<object> arg1, RestRequestAsyncHandle arg2)
        {
            throw new NotImplementedException();
        }
    }
}



