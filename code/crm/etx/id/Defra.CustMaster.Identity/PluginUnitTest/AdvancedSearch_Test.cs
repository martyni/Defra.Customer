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
//;
using FakeXrmEasy;
using Defra.CustMaster.D365.Common.Ints.Idm;
using Defra.CustMaster.D365.Common.Ints.Idm.Resp;
using CrmEarlyBound;
using Defra.CustMaster.Identity.WfActivities.Common;

namespace Defra.PluginUnitTest
{

    [TestClass]
    public class AdvancedSearch_Test
    {

        //var fakedContext = new XrmFakedContext();
        ////input object does not contain to record id which is mandatory.
        //string InputLoad = @"
        //          {
                      
                     
                     
        //        ";


        ////Inputs
        //var inputs = new Dictionary<string, object>() {
        //        { "advancedfi", InputLoad },
        //        };

        //var connection = fakedContext.CreateQuery<Account>();

        //var result = fakedContext.ExecuteCodeActivity<SearchRecords>(inputs);

        //#region ErrorMessagesToCheck




        //#endregion

        //String ReturnMessage = (String)result["response"];
        //AccountResponse ContactResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountResponse>(ReturnMessage);
        //String ErrorDetails = ContactResponseObject.message;
        //bool ContainsErrorMessageToRole = ErrorDetails.Contains("To role is mandatory validation failed.");
        ////StringAssert.Contains(ErrorDetails, AccountNameLengthErrorMessage, "Account name length validation failed.");
        //StringAssert.Contains(ErrorDetails, AccountNameRequiredErrorMessage, "Account name required validation failed.");
        
    }
}
