using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestSharp;

namespace Defra.CustomerMaster.Identity.Api.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        const string URL = "http://localhost:54670/api/";
        const string CONTACTID = "97063ff4-f479-e811-a963-000d3a2bccc5";
         string _b2cobjectId = "7b1ad2d0-7946-11e8-8d36-851e870eee8a";
         string _serviceId = "534BA555-037A-E811-A95B-000D3A2BC547";
        //{"contactid":"97063ff4-f479-e811-a963-000d3a2bccc5","defra_uniquereference":"CID-0000001015","errorCode":200,"errorMsg":null}
    [TestMethod]
        public void InitialMatchTestWithValidInput()
        {          
            
            var client = new RestClient(URL);
            // var key = ApplicationAuthenticator.GetS2SAccessTokenForProdMSAAsync();
            var request = new RestRequest("InitialMatch/"+_b2cobjectId, Method.GET)
            {
                RequestFormat = DataFormat.Json

            };
           

            var response = client.Get(request);
            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(response.Content))
            {
              

                InitialMatchResponse returnValue = JsonConvert.DeserializeObject<InitialMatchResponse>(response.Content);
                Assert.IsNotNull(returnValue.ServiceUserID);                
                Assert.AreEqual(CONTACTID, returnValue.ServiceUserID);
                Assert.AreEqual(200, returnValue.ErrorCode);

            }

        }
        [TestMethod]
        public void AuthzTestWithValidInput()
        {

            var client = new RestClient(URL);
            // var key = ApplicationAuthenticator.GetS2SAccessTokenForProdMSAAsync();
            var request = new RestRequest("Authz/ServiceID=" + _serviceId + "&B2CObjectId=" + _b2cobjectId, Method.GET)
            {
                RequestFormat = DataFormat.Json

            };


            var response = client.Get(request);
            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(response.Content))
            {


                AuthzResponse returnValue = JsonConvert.DeserializeObject<AuthzResponse>(response.Content);
                Assert.IsTrue(returnValue.Roles.Count > 0);


            }

        }
        [TestMethod]
        public void InitialMatchTestWithInValidInput()
        {
            _b2cobjectId = "7b1ad2d0-7946-11e8-8d36-851e870eee8";
            var client = new RestClient(URL);
            // var key = ApplicationAuthenticator.GetS2SAccessTokenForProdMSAAsync();
            var request = new RestRequest("InitialMatch/" + _b2cobjectId, Method.GET)
            {
                RequestFormat = DataFormat.Json

            };


            var response = client.Get(request);
            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(response.Content))
            {


                InitialMatchResponse returnValue = JsonConvert.DeserializeObject<InitialMatchResponse>(response.Content);
                Assert.AreEqual(400, returnValue.ErrorCode);
                Assert.AreEqual("B2CObjectid is invalid", returnValue.ErrorMsg);

            }

        }
        [TestMethod]
        public void InitialMatchTestWhereB2CObjectNotExists()
        {
            _b2cobjectId = "7b1ad2d0-7946-11e8-8d36-851e870eee8b";
            var client = new RestClient(URL);
            // var key = ApplicationAuthenticator.GetS2SAccessTokenForProdMSAAsync();
            var request = new RestRequest("InitialMatch/" + _b2cobjectId, Method.GET)
            {
                RequestFormat = DataFormat.Json

            };


            var response = client.Get(request);
            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(response.Content))
            {

                InitialMatchResponse returnValue = JsonConvert.DeserializeObject<InitialMatchResponse>(response.Content);
                Assert.AreEqual(204, returnValue.ErrorCode);
                Assert.AreEqual("No Content", returnValue.ErrorMsg);

            }

        }

      
        [TestMethod]
        public void AuthzTestWithValidInputWithNoRolesInTheSystem()
        {
            _b2cobjectId = "7b1ad2d0-7946-11e8-8d36-851e870eee8d";
            var client = new RestClient(URL);
            // var key = ApplicationAuthenticator.GetS2SAccessTokenForProdMSAAsync();
            var request = new RestRequest("Authz/ServiceID=" + _serviceId + "&B2CObjectId=" + _b2cobjectId, Method.GET)
            {
                RequestFormat = DataFormat.Json

            };


            var response = client.Get(request);
            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(response.Content))
            {


                AuthzResponse returnValue = JsonConvert.DeserializeObject<AuthzResponse>(response.Content);
                Assert.AreEqual(204, returnValue.ErrorCode);
                Assert.AreEqual("No Content", returnValue.ErrorMsg);

            }

        }
        [TestMethod]
        public void AuthzTestWithInValidInput()
        {
            _serviceId = "534BA555-037A-E811-A95B-000D3A2BC54";
            _b2cobjectId = "7b1ad2d0-7946-11e8-8d36-851e870eee8a";
            var client = new RestClient(URL);
            // var key = ApplicationAuthenticator.GetS2SAccessTokenForProdMSAAsync();
            var request = new RestRequest("Authz/ServiceID=" + _serviceId + "&B2CObjectId=" + _b2cobjectId, Method.GET)
            {
                RequestFormat = DataFormat.Json

            };


            var response = client.Get(request);
            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(response.Content))
            {


                AuthzResponse returnValue = JsonConvert.DeserializeObject<AuthzResponse>(response.Content);
                Assert.AreEqual(400, returnValue.ErrorCode);
                Assert.AreEqual("ServiceId is invalid", returnValue.ErrorMsg);


            }

        }
    }
}
