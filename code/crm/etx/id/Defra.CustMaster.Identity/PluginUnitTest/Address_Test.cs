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

namespace Defra.Test
{
   public class Address_Test
    {

        [TestMethod]
        public void CoonectContactCheckRequiredFields_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                  {
                      'fromrecordid': '369d71cf-c874-e811-a83b-000d3ab4f7af',
                      'fromrecordtype': 'contact',
                      'torecordid': 'b7293664-e46a-e811-a83c-000d3ab4f967',
                      'torecordtype': 'organisation',

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




            //fakedContext.Initialize(new List<Entity>()
            //{   new Entity() { Id = new Guid("369d71cf-c874-e811-a83b-000d3ab4f7af"), LogicalName = "account" },
            //        });
            //var connection = fakedContext.CreateQueryFromEntityName("connection");
            var connection = fakedContext.CreateQuery<Connection>();

            var result = fakedContext.ExecuteCodeActivity<ConnectContact>(inputs);

            String ReturnMessage = (String)result["ReturnMessageDetails"];
            ContactResponse ContactResponseObject = JsonConvert.DeserializeObject<ContactResponse>(ReturnMessage);
            String ErrorDetails = ContactResponseObject.message;
            bool ContainsErrorMessageToRole = ErrorDetails.Contains("To role is mandatory.");
            Assert.AreEqual(true, ContainsErrorMessageToRole, "To role is required");
        }
    }
}
