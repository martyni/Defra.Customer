using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestSharp;
using System.Threading;

namespace Defra.CustMaster.Identity.CoreApp.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        const string URL = "http://localhost:54670/api/";//"http://defra-cm-dev-id-app.azurewebsites.net/api/";// "http://localhost:54670/api/";
        const string CONTACTID = "23339b9c-d793-e811-a960-000d3a246bb7";
        string _b2cobjectId = "b4a997bf-310d-4a75-ab4b-240823efa7f4";//"8d0fe141-93d7-11e8-9d9f-69f8b87d910d";
        string _serviceId = "5A90DD44-DD9B-E811-A94F-000D3A3A8543";//"40EE9D1F-CD85-E811-A83D-000D3AB4F967";
                                                                   //{"contactid":"97063ff4-f479-e811-a963-000d3a2bccc5","defra_uniquereference":"CID-0000001015","errorCode":200,"errorMsg":null}
        [TestMethod]
        public void InitialMatchTestWithValidInput()
        {

            var client = new RestClient(URL);
            // var key = ApplicationAuthenticator.GetS2SAccessTokenForProdMSAAsync();
            var request = new RestRequest("InitialMatch?" + _b2cobjectId, Method.GET)
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
            var request = new RestRequest("Authz?ServiceID=" + _serviceId + "&B2CObjectId=" + _b2cobjectId, Method.GET)
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
            var request = new RestRequest("InitialMatch?" + _b2cobjectId, Method.GET)
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
            var request = new RestRequest("InitialMatch?" + _b2cobjectId, Method.GET)
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
            var request = new RestRequest("Authz?ServiceID=" + _serviceId + "&B2CObjectId=" + _b2cobjectId, Method.GET)
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
            var request = new RestRequest("Authz?ServiceID=" + _serviceId + "&B2CObjectId=" + _b2cobjectId, Method.GET)
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
