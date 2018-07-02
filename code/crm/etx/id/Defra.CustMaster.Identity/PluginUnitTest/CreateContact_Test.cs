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
using System.Linq;

namespace Defra.Test
{

    [TestClass]
    public class CreateContact_Test
    {
        const String ObjectIdLengthCheck = "B2C Object Id is invalid/exceed the max length(36);";
        const String FirstNameFieldLengthCheck = "First Name cannot be greater than 50;";
        const String RequiredFirstNameCheck = "Firstname is required;";
        const String MiddleNameLengthCheck = "Middle Name cannot be greater than 50;";
        const String RequiredLastNameCheck= "Lastname is required";
        const String LastNameFieldLengthCheck = "Last Name cannot be greater than 50";
        const String EmailIdFieldLengthCheck = "Email cannot be greater than 100;";
        const String TelephoneFieldLengthCheck = "Telephone cannot be greater than 50;";
        const String TAndCondiationFieldCheck = "T&C Accepted Version cannot be greater than 5;";

        [TestMethod]
        public void CreateContact_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                  {
                      'b2cobjectid': 'b2c12062018-cd20180618',
                      'firstname': 'idm.frist.cd20180618idm.',
                      'middlename': 'idm.frist.cd20180618idm.',
                        'lastname': 'idm.frist.cd20180618idm.frist.c',
                      'title': 1,
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
            var inputs = new Dictionary<string, object>() {
                { "Payload", InputLoad },
                };

            var result = fakedContext.ExecuteCodeActivity<CreateContact>(inputs);
            var contact = (from t in fakedContext.CreateQuery<Contact>()
                         select t).ToList();

            String ReturnMessage = (String)result["Response"];
            ContactResponse ContactResponseObject = JsonConvert.DeserializeObject<ContactResponse>(ReturnMessage);

            // checking 500 code as the workflow will not genrate uniqure refenrece
            //so id was checked along with response code.
            Assert.AreEqual(ContactResponseObject.code, 500, "Response code check" );
            Assert.IsNotNull(ContactResponseObject.data.contactid);


        }
        [TestMethod]
        public void CreateContactCheckRequiredFields_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                  {
                      'b2cobjectid': 'b2c12062018-cd20180618',
                      'title': 1,
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
            var inputs = new Dictionary<string, object>() {
                { "Payload", InputLoad },
                };

            var result = fakedContext.ExecuteCodeActivity<CreateContact>(inputs);

            String ReturnMessage = (String)result["Response"];
            ContactResponse ContactResponseObject = JsonConvert.DeserializeObject<ContactResponse>(ReturnMessage);
            String ErrorDetails = ContactResponseObject.message;
            StringAssert.Contains(ReturnMessage, RequiredFirstNameCheck, "First name required field check failed");
            StringAssert.Contains(ReturnMessage, RequiredLastNameCheck, "Last name required field check failed");

        }

        [TestMethod]
        public void CreateContactCheckFieldLength_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                  {
                      'b2cobjectid': 'b2c12062018-cd20180618b2c12062018-cd20180618b2c12062018-cd20180618b2c12062018-cd20180618',
                      'title': 1,
                      'firstname': 'idm.frist.cd20180618idm.frist.cd20180618idm.frist.c',
                      'middlename': 'idm.frist.cd20180618idm.frist.cd20180618idm.frist.c',
                      'lastname': 'idm.frist.cd20180618idm.frist.cd20180618idm.frist.c',
                      'email': 'idm.frist.cd20180618idm.frist.cd20180618idm.frist.cd20180618idm.frist.cd20180618idm.frist.cd20180618idm.frist.cd20180618idm.frist.cd20180618idm.frist.cd20180618@sdd.com',
                      'dob': '06/07/2018',
                      'gender': 2,
                      'telephone': '004412345678004412345678004412345678004412345678121',
                      'tacsacceptedversion': '123456',
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
            var inputs = new Dictionary<string, object>() {
                { "Payload", InputLoad },
                };

            var result = fakedContext.ExecuteCodeActivity<CreateContact>(inputs);

            String ReturnMessage = (String)result["Response"];
            ContactResponse ContactResponseObject = JsonConvert.DeserializeObject<ContactResponse>(ReturnMessage);
            String ErrorDetails = ContactResponseObject.message;
            StringAssert.Contains(ErrorDetails, ObjectIdLengthCheck, "Oject ID field length field check failed.");
            StringAssert.Contains(ErrorDetails, FirstNameFieldLengthCheck, "First name field lenght check failed.");
            StringAssert.Contains(ErrorDetails, MiddleNameLengthCheck, "Middle name field lenght check failed.");
            StringAssert.Contains(ErrorDetails, LastNameFieldLengthCheck, "Last name field lenght check failed.");
            StringAssert.Contains(ErrorDetails, EmailIdFieldLengthCheck, "Email Id field lenght check failed.");
            StringAssert.Contains(ErrorDetails, TelephoneFieldLengthCheck, "Telephone field lenght check failed.");
            StringAssert.Contains(ErrorDetails, TAndCondiationFieldCheck, "T&C field lenght check failed.");

        }

        
    }
}

