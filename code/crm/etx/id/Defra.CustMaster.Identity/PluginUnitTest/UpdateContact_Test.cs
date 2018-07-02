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

namespace PluginUnitTest
{
   public class UpdateContact_Test
    {
        [TestMethod]
        public void UpdateContactCheckRequiredFields_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                                 {
                                  'b2cobjectid': 'b2c12062018-cd20180618',
                                  'title': 1,
                                  'firstname': 'idm.frist.cd20180618',
                                  'lastname': 'idm.last.cd20180618',
                                  'email': 'idm.cd20180618@customer.com',
                                  'dob': '06/07/2018',
                                  'gender': 2,
                                  'telephone': '004412345678',
                                  'tacsacceptedversion': '12345',
                                  'tacsacceptedon': '10/09/2018 06:06:06',
                                  'address': {
                                    'type': 1,
                                    'uprn': 20180618,
                                    'buildingname': 'Horizon',
                                    'buildingnumber': '3',
                                    'street': 'deanary',
                                    'locality': '',
                                    'town': '',
                                    'postcode': 'hp98tj',
                                    'country': 'gbr',
                                    'fromcompanieshouse': ''
                                  }
                                }
                ";



            String ContactIdMaxLenErrorMsg = "Contact ID is invalid/exceed the max length(50);";
            String FirstNameMaxLengthErrorMsg = "First Name cannot be greater than 50;";
            String MiddlenameMaxLengthErrorMsg = "Middle Name cannot be greater than 50;";
            String LastNameMaxLengthErrorMsg = "Last Name cannot be greater than 50";
            String EmailMaxLengthErrorMsg = "Email cannot be greater than 100;";
            String TelephoneMaxLengthErrorMsg = "Telephone cannot be greater than 50;";
            String TermsAndConditionMaxLengthErrorMsg  = "T&C Accepted Version cannot be greater than 5;";



            #region Setting Input Params
            //Inputs
            var inputs = new Dictionary<string, object>() {
                { "PayLoad", InputLoad },
                };
            Guid AgentId = Guid.NewGuid();
            Guid AgentCustomerId = Guid.NewGuid();
            ConnectionRole RoleAgent = new ConnectionRole();
            RoleAgent.Id = AgentId;
            RoleAgent.Name = "Agent";
            ConnectionRole RoleAgentcustomer = new ConnectionRole();
            RoleAgentcustomer.Id = AgentCustomerId;
            RoleAgentcustomer.Name = "Agent Customer";
            ConnectionRole PrimaryRole = new ConnectionRole();
            PrimaryRole.Id = Guid.NewGuid();
            PrimaryRole.Name = "Primary User";

            ConnectionRole AgentRole = new ConnectionRole();
            AgentRole.Id = AgentId;
            AgentRole.Name = "Agent";
            fakedContext.Initialize(new List<Entity>()
            {   new Entity() { Id = new Guid("369d71cf-c874-e811-a83b-000d3ab4f7af"), LogicalName = "contact" },
                new Entity() { Id = new Guid("b7293664-e46a-e811-a83c-000d3ab4f967"), LogicalName = "account" },
                AgentRole, RoleAgentcustomer, PrimaryRole
            });


            var connection = fakedContext.CreateQuery<Connection>();

            var result = fakedContext.ExecuteCodeActivity<UpdateContact>(inputs); 
            #endregion

            String ReturnMessage = (String)result["ReturnMessageDetails"];
            ContactResponse ContactResponseObject = JsonConvert.DeserializeObject<ContactResponse>(ReturnMessage);
            String ErrorDetails = ContactResponseObject.message;
            StringAssert.Contains(ReturnMessage, ContactIdMaxLenErrorMsg, "Contact ID validation failed.");
            StringAssert.Contains(ReturnMessage, FirstNameMaxLengthErrorMsg, "First name validation failed.");
            StringAssert.Contains(ReturnMessage, MiddlenameMaxLengthErrorMsg, "Middle name validation failed.");
            StringAssert.Contains(ReturnMessage, LastNameMaxLengthErrorMsg, "Last name validation failed.");
            StringAssert.Contains(ReturnMessage, EmailMaxLengthErrorMsg, "Email validation failed.");
            StringAssert.Contains(ReturnMessage, TelephoneMaxLengthErrorMsg, "Telephone max length validation failed.");
            StringAssert.Contains(ReturnMessage, TermsAndConditionMaxLengthErrorMsg, "T&C Validation failed.");


        }
    }
}
