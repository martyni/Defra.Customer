using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Defra.CustMaster.Identity.WfActivities.Connection;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using Microsoft.Xrm.Sdk;

using Defra.CustMaster.Identity.WfActivities;
using static Defra.CustMaster.Identity.WfActivities.WorkFlowActivityBase;
using System.Collections.Generic;
using Newtonsoft.Json;
using FakeXrmEasy;
using Defra.CustMaster.D365.Common.Ints.Idm;
using Defra.CustMaster.D365.Common.Ints.Idm.Resp;
using CrmEarlyBound;
using Defra.CustMaster.D365.Common.Ints.Idm.resp;

namespace Defra.Test
{
    [TestClass]
   public class Address_Test
    {


        const String RecordTypeCannotBeEmpty = "record type can not be empty";

        const String RecordIdCannotBeEmpty = "recordid can not be empty";
        const String RecordIDMaxLengthCheck = "record id exceeded the max length(36);";

        [TestMethod]
        public void AddressFieldCheck_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                 {
                      recordtype: '1',
                      recordid: '',
                      'address': {
                        'type': 1,
                        'uprn': 3458999,
                        'buildingname': 'Horizon',
                        'buildingnumber': '3',
                        'street': 'Road',
                        'locality': '',
                        'town': '',
                        'postcode': '12345678',
                        'country': 'gbr',
                        'fromcompanieshouse': ''
                      }
                    }

                ";


            //Inputs
            var inputs = new Dictionary<string, object>() {
                {
                        "ReqPayload", InputLoad },
                };

            var connection = fakedContext.CreateQuery<Account>();

            var result = fakedContext.ExecuteCodeActivity<AddAddress>(inputs);

            #region ErrorMessagesToCheck

            #endregion

            String ReturnMessage = (String)result["ResPayload"];
            AddressResponse AddressResponseObject = JsonConvert.DeserializeObject<AddressResponse>(ReturnMessage);
            String ErrorDetails = AddressResponseObject.message;

           // StringAssert.Contains(ErrorDetails, RecordTypeCannotBeEmpty, "Record type cannot be empty validation failed.");
            StringAssert.Contains(ErrorDetails, RecordIdCannotBeEmpty, "Record id cannot be empty validation failed.");
            //StringAssert.Contains(ErrorDetails, RecordIDMaxLengthCheck, "Record id max length validation failed.");

        }


        [TestMethod]
        public void RecordIdMaxLengthValidation()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                 {
                      recordtype: '1',
                      recordid: 'sdsdsdsdsdsdsdddddddddddddddddddddddddddddddddddddddddddddd',
                      'address': {
                        'type': 1,
                        'uprn': 3458999,
                        'buildingname': 'Horizon',
                        'buildingnumber': '3',
                        'street': 'Road',
                        'locality': '',
                        'town': '',
                        'postcode': '12345678',
                        'country': 'gbr',
                        'fromcompanieshouse': ''
                      }
                    }

                ";


            //Inputs
            var inputs = new Dictionary<string, object>() {
                {
                        "ReqPayload", InputLoad },
                };

            var connection = fakedContext.CreateQuery<Account>();

            var result = fakedContext.ExecuteCodeActivity<AddAddress>(inputs);

            #region ErrorMessagesToCheck

            #endregion

            String ReturnMessage = (String)result["ResPayload"];
            AddressResponse AddressResponseObject = JsonConvert.DeserializeObject<AddressResponse>(ReturnMessage);
            String ErrorDetails = AddressResponseObject.message;

            // StringAssert.Contains(ErrorDetails, RecordTypeCannotBeEmpty, "Record type cannot be empty validation failed.");
            //StringAssert.Contains(ErrorDetails, RecordIdCannotBeEmpty, "Record id cannot be empty validation failed.");
            StringAssert.Contains(ErrorDetails, RecordIDMaxLengthCheck, "Record id max length validation failed.");

        }

        [TestMethod]
        public void AddressCreate_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                 {
                      recordtype: 'contact',
                      recordid: '37e64f21-c035-4e49-a6b6-958cdd3af45e',
                      'address': {
                        'type': 1,
                        'uprn': 3458999,
                        'buildingname': 'Horizon',
                        'buildingnumber': '3',
                        'street': 'Road',
                        'locality': '',
                        'town': '',
                        'postcode': '12345678',
                        'country': 'gbr',
                        'fromcompanieshouse': ''
                      }
                    }

                ";


            //Inputs
            var inputs = new Dictionary<string, object>() {
                {
                        "ReqPayload", InputLoad },
                };

            var result = fakedContext.ExecuteCodeActivity<AddAddress>(inputs);
            var address = fakedContext.CreateQuery<defra_address>();

            #region ErrorMessagesToCheck

            #endregion

            String ReturnMessage = (String)result["ResPayload"];
            AddressResponse AddressResponseObject = JsonConvert.DeserializeObject<AddressResponse>(ReturnMessage);
            String ErrorDetails = AddressResponseObject.message;
            Assert.IsNotNull(AddressResponseObject.data.addressid);
            Assert.Equals(AddressResponseObject.code, 200);


        }
    }
}
