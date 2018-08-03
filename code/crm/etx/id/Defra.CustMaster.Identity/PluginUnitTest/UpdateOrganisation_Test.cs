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
//using Newtonsoft.Json;
using FakeXrmEasy;
using Defra.CustMaster.D365.Common.Ints.Idm;
using Defra.CustMaster.D365.Common.Ints.Idm.Resp;
using CrmEarlyBound;

namespace PluginUnitTest
{

        [TestClass]
        public class UpdateOrganisation_Test
    {
        #region Local Constants

        const string OrganisationIDRequired =  "Organisationid is mandatory.";

        const String AccountNameLengthErrorMessage = "Organiation name is more than 160 characters.";
        const String BusinessTypeRequiredErrMsg = "Business type cannot be empty.";
        const String CompanyHouseLengthLimitErrorMsg = "Company House Id cannot be more than 8 characters.";
        const String EmailAddressMoreLengthValidation = "Email address cannot be more than 100 characters long.";

           
            #endregion

            [TestMethod]
            public void CheckRequiredFieldsAndFieldLenth_Success()
            {

           
                var fakedContext = new XrmFakedContext();
                //input object does not contain to record id which is mandatory.
                string InputLoad = @"
                  {
                      'updates': {
                        'validatedwithcompanieshouse': 'false',
                        'name': 'Update my createUpdate my createUpdate my createUpdate my createUpdate my createUpdate my createUpdate my createUpdate my createUpdate my createUpdate my createUpdate my create',
                        'type': '',
                        'crn': '1806201823232',
                        'email': 'eUpdateacmeUpdateacmeUpdateacmeUpdateacmeUpdateacmeUpdateacmeUpdateacmeUpdateacmeUpdateacmeUpdateacmeme.com',
                        'telephone': '004412345678',
                        'hierarchylevel': '910400000',
                        'parentorganisationcrmid': '89EF9173-016F-E811-A83A-000D3AB4F534'
                      },
                      'clearlist': {
                        'fields': [
                          'crn',
                          'email',
                          'parentorganisationcrmid',
                          'hierarchylevel',
                          'validatedwithcompanieshouse'
                        ]
                      }
                    }
                ";


                //Inputs
                var inputs = new Dictionary<string, object>() {
                {
                        "ReqPayload", InputLoad },
                };

                var connection = fakedContext.CreateQuery<Account>();

                var result = fakedContext.ExecuteCodeActivity<UpdateOrganisation>(inputs);

                #region ErrorMessagesToCheck

                #endregion

                String ReturnMessage = (String)result["ResPayload"];
                AccountResponse AccountResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountResponse>(ReturnMessage);
                String ErrorDetails = AccountResponseObject.message;
                StringAssert.Contains(ReturnMessage, OrganisationIDRequired, "Organisation Id field check failed.");
                StringAssert.Contains(ReturnMessage, AccountNameLengthErrorMessage, "Account name field length check failed.");
                StringAssert.Contains(ReturnMessage, CompanyHouseLengthLimitErrorMsg, "Company house id field length check failed.");
                StringAssert.Contains(ReturnMessage, EmailAddressMoreLengthValidation, "Email id field check failed");

        }


        [TestMethod]
            public void CheckUpdateMethod_Success()
            {
                var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                  {
                      'organisationid': 'D1B35E7C-D072-E811-A83B-000D3AB4F7AF',
                      'updates': {
                        'validatedwithcompanieshouse': 'false',
                        'name': 'Update my create',
                        'type': '910400000',
                        'crn': '18062018',
                        'email': 'Updateacme@acme.com',
                        'telephone': '004412345678',
                        'hierarchylevel': '910400000',
                        'parentorganisationcrmid': '89EF9173-016F-E811-A83A-000D3AB4F534'
                      },
                      'clearlist': {
                        'fields': [
                          'crn',
                          'email',
                          'parentorganisationcrmid',
                          'hierarchylevel',
                          'validatedwithcompanieshouse'
                        ]
                      }
                    }
                ";


            //Inputs
            var inputs = new Dictionary<string, object>() {
                { "ReqPayload", InputLoad },
                };

                //var connection = fakedContext.CreateQuery<Account>();

                var result = fakedContext.ExecuteCodeActivity<UpdateOrganisation>(inputs);

                #region ErrorMessagesToCheck




                #endregion

                String ReturnMessage = (String)result["ResPayload"];
                AccountResponse ContactResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountResponse>(ReturnMessage);
                String ErrorDetails = ContactResponseObject.message;
                bool ContainsErrorMessageToRole = ErrorDetails.Contains("To role is mandatory.");


                

            }

        [TestMethod]
        public void CheckUpdateWithoutOrganisationType_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                 {
                     'organisationid': 'a494d047-137e-e811-a95b-000d3a2bc547',
                     'updates': {
                       'name': 'Associated Dairies'
                     },
                     'clearlist': {
                       'fields': []
                     }
                    }
                ";

            fakedContext.Initialize(new List<Entity>()
            {   new Entity() { Id = new Guid("369d71cf-c874-e811-a83b-000d3ab4f7af"), LogicalName = "contact" },
                new Entity() { Id = new Guid("a494d047-137e-e811-a95b-000d3a2bc547"), LogicalName = "account" }
                
            });
            //Inputs
            var inputs = new Dictionary<string, object>() {
                { "ReqPayload", InputLoad },
                };

            //var connection = fakedContext.CreateQuery<Account>();

            var result = fakedContext.ExecuteCodeActivity<UpdateOrganisation>(inputs);
            
            #region ErrorMessagesToCheck
            String ReturnMessage = (String)result["ResPayload"];


            AccountResponse ContactResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountResponse>(ReturnMessage);


            #endregion

            
            String ErrorDetails = ContactResponseObject.message;
            Assert.IsNotNull(ContactResponseObject.data.accountid);




        }
    }


   
}
    
