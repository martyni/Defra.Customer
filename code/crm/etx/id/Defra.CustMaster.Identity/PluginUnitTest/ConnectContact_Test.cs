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

namespace PluginUnitTest
{
    [TestClass]
    public class ConnectContact_Test
    {
        
        [TestMethod]
        public void CoonectContactCheckRequiredFields_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"{
                  'fromrecordid': '369d71cf-c874-e811-a83b-000d3ab4f7af',
                  'fromrecordtype': 'contact',
                  'relations': {
                    'torole': 'Agent',
                    'fromrole': 'Agent Customer'
                  }
                }
                ";

            //Inputs
            var inputs = new Dictionary<string, object>() {
                { "PayLoad", InputLoad },
                };
            var result = fakedContext.ExecuteCodeActivity<ConnectContact>(inputs);
            String ReturnMessage = (String)result["ReturnMessageDetails"];
            ContactResponse ContactResponseObject = JsonConvert.DeserializeObject<ContactResponse>(ReturnMessage);

            Assert.IsNotNull(ContactResponseObject, "Response object should present");
            // Aseseting code 200 returned
            Assert.AreEqual(400, ContactResponseObject.code, String.Format( @"Return must contain 400 error code. 
                it contains {0}", ContactResponseObject.code));

            StringAssert.Contains("To record id is mandatory. ", ContactResponseObject.message);
            StringAssert.Contains("To record type required. ", ContactResponseObject.message);
            
        }
    }
}
