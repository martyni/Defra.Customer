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

namespace Defra.Test
{
   


    [TestClass]
    public  class CreateOrgnaisationRquiredFields_Test
    {
        #region Local Constants
        const  String AccountNameLengthErrorMessage = "Organisation name is more than 160 characters.";
        const  String AccountNameRequiredErrorMessage = "Organisation name is mandatory.";
        const  String BusinessTypeRequiredErrMsg = "Business type cannot be empty.";
        const  String CompanyHouseLengthLimitErrorMsg = "Company House Id cannot be more than 8 characters.";
        const  String EmailAddressMoreLengthValidation = "Email address cannot be more than 100 characters long.";

        //address Error Messages

        const   String AddressTypeCannotBeEmtyErrMsg = "type can not be empty";
        const   String UprnCannotBeEmtyErrMsg = "UPRN can not be empty";
        const   String UprnLengthERrorMessage = "UPRN cannot be greater than 20;";
        const    String BuildingNameCannotEmptyErrMsg = "building name can not be empty";
        const    String BuildingNameLengthErrorMsg = "Building Name cannot be greater than 450;";
        const    String BuildingNumberLengthErroMsg = "Building Number cannot be greater than 20;";
        const    String StreetCannotMoreThan100ErrMsg = "Street cannot be greater than 100;";
        const    String LocalityLengthErrorMsg = "Locality cannot be greater than 100;";
        const    String TownLengthErrorMsg = "Town cannot be greater than 70;";
        const    String PostCodeRequiredErrorMessage = "postcode can not be empty";
        const    String PostCodeLengthErrorMsg = "Postcode cannot be greater than 8;";
        const    String CountryRequiredErrMsg = "country can not be empty";
        const    String ISOCodeLengthErrMsg = "Country ISO ALPHA-3 Code cannot be greater than 3;";
        #endregion

        [TestMethod]
        public void CheckRequiredFields_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                  {
                      
                     
                      'crn': '14292356',
                      'email': 'acme@acme.com',
                      'address': {
                  
                       
                        'buildingname': 'Horizon House',
                        'buildingnumber': '123',
                        'street': 'Deanery Road',
                        'locality': '',
                        'town': '',
                        'fromcompanieshouse': 'true'
                      },
                      'telephone': '004412345678',
                      'hierarchylevel': '910400000',
                      'parentorganisation': {
                        'parentorganisationcrmid': '14770817-e16a-e811-a83c-000d3ab4f967'
                      }
                    }
                ";


            //Inputs
            var inputs = new Dictionary<string, object>() {
                { "request", InputLoad },
                };

            var connection = fakedContext.CreateQuery<Account>();

            var result = fakedContext.ExecuteCodeActivity<CreateOrganisation>(inputs);

            #region ErrorMessagesToCheck

            


            #endregion

            String ReturnMessage = (String)result["response"];
            AccountResponse ContactResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountResponse>(ReturnMessage);
            String ErrorDetails = ContactResponseObject.message;
            bool ContainsErrorMessageToRole = ErrorDetails.Contains("To role is mandatory validation failed.");
            //StringAssert.Contains(ErrorDetails, AccountNameLengthErrorMessage, "Account name length validation failed.");
            StringAssert.Contains(ErrorDetails, AccountNameRequiredErrorMessage, "Account name required validation failed.");
            StringAssert.Contains(ErrorDetails, BusinessTypeRequiredErrMsg, "Business type required validation failed.");
            //StringAssert.Contains(ErrorDetails, CompanyHouseLengthLimitErrorMsg, "Company house length validation failed.");
            //StringAssert.Contains(ErrorDetails, EmailAddressMoreLengthValidation, "Email address length validation failed.");
            StringAssert.Contains(ErrorDetails, AddressTypeCannotBeEmtyErrMsg, "address type required validation not working.");
            StringAssert.Contains(ErrorDetails, UprnCannotBeEmtyErrMsg, "UPRN required validation not working");
            //StringAssert.Contains(ErrorDetails, UprnLengthERrorMessage, "UPRN length  validation not working.");
            //StringAssert.Contains(ErrorDetails, BuildingNameCannotEmptyErrMsg, "Building name  validation not working.");
            //StringAssert.Contains(ErrorDetails, BuildingNameLengthErrorMsg, "Building name  validation not working.");
            //StringAssert.Contains(ErrorDetails, BuildingNumberLengthErroMsg, "Building number  validation not working.");
            //StringAssert.Contains(ErrorDetails, StreetCannotMoreThan100ErrMsg, "Street character length cannot be more than 100 .");
            //StringAssert.Contains(ErrorDetails, LocalityLengthErrorMsg, "Locality length validation not working.");
            //StringAssert.Contains(ErrorDetails, TownLengthErrorMsg, "Town length validation not working.");
            StringAssert.Contains(ErrorDetails, PostCodeRequiredErrorMessage, "Postcode is required.");
            //StringAssert.Contains(ErrorDetails, PostCodeLengthErrorMsg, "Post code length validation failed.");
            StringAssert.Contains(ErrorDetails, CountryRequiredErrMsg, "Country required validation failed.");
            //StringAssert.Contains(ErrorDetails, ISOCodeLengthErrMsg, "ISO code field length validation failed.");

        }
        [TestMethod]
        public void CheckHirachyLevelErrorMessage_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                  {
                      'name': 'Acme Limited',
                      'type': 910400001, 
                      'crn': '1230234', 
                      'email': 'acme@acme.com',  
                      'telephone': '004412345678', 
                      'validatedwithcompanieshouse': true,
                      'address': { 
                        'type': 1,
                        'uprn': '200010019924', 
                        'buildingname': 'Horizon House', 
                        'buildingnumber': '123', 
                        'street': 'Deanery Road',
                        'locality': 'new',
                        'town': 'test', 
                        'postcode': 'HA9 7AH', 
                        'country': 'UK', 
                        'fromcompanieshouse': 'true' 
                       },
                       
                       'parentorganisation': {
                         'parentorganisationcrmid': '194bc6f6-1685-e811-a845-000d3ab4fddf'
                       }
                    }
                ";


            //Inputs
            var inputs = new Dictionary<string, object>() {
                { "request", InputLoad },
                };

            var connection = fakedContext.CreateQuery<Account>();

            var result = fakedContext.ExecuteCodeActivity<CreateOrganisation>(inputs);
            String ReturnMessage = (String)result["response"];
            AccountResponse ContactResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountResponse>(ReturnMessage);
            String ErrorDetails = ContactResponseObject.message;
            StringAssert.Contains(ErrorDetails,  String.Format("Option set value {0} for orgnisation hirarchy level not found.", 910400));

        }

        [TestMethod]
        public void CheckOrgnistaionTypeErrorMessage_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                  {
                'name': 'Sainsburys',
                'type': 91040,
                'crn': '23274286',
                'email': 'c0f4d100-86c1-11e8-8d0b-17eda72fe0d2@idm-test.example.com',
                'telephone': '+447812555555',
                'validatedwithcompanieshouse': true,
                'address': {
                  'type': 1,
                  'uprn': '200010019924',
                  'buildingname': 'Horizon House',
                  'street': 'Lombard Street',
                  'town': 'Bristol',
                  'postcode': 'BH0 0HB',
                  'country': 'GBR',
                  'fromcompanieshouse': true
                }
                }
                ";


            //Inputs
            var inputs = new Dictionary<string, object>() {
                { "request", InputLoad },
                };

            var connection = fakedContext.CreateQuery<Account>();

            var result = fakedContext.ExecuteCodeActivity<CreateOrganisation>(inputs);
            String ReturnMessage = (String)result["response"];
            AccountResponse ContactResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountResponse>(ReturnMessage);
            String ErrorDetails = ContactResponseObject.message;
            StringAssert.Contains(ErrorDetails, 
                String.Format("Option set value {0} for orgnisation type does not exists.", 91040));
        }

        [TestMethod]
        public void CheckFieldLength_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                  {
                      'name': 'newonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestname',
                      'type': '910400000',
                      'crn': '14292356111',
                      'email': 'newonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestname@sdsd.com',
                      'address': {
                        'type': '3',
                        'uprn': '208412459142084124591420841245914',
                        'buildingname': '',
                        'buildingnumber': '123456789012345678901',
                        'street': 'newonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestname',
                        'locality': 'newonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestname',
                        'town': 'newonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestnamenewonetestname',
                        'postcode': 'HA9 7AHXXXXXXX',
                        'country': 'ABCD',
                        'fromcompanieshouse': 'true'
                      },
                      'telephone': '004412345678',
                      'hierarchylevel': '910400000',
                      'parentorganisation': {
                        'parentorganisationcrmid': '14770817-e16a-e811-a83c-000d3ab4f967'
                      }
                    }
                ";


            //Inputs
            var inputs = new Dictionary<string, object>() {
                { "request", InputLoad },
                };

            var connection = fakedContext.CreateQuery<Account>();

            var result = fakedContext.ExecuteCodeActivity<CreateOrganisation>(inputs);

            #region ErrorMessagesToCheck




            #endregion

            String ReturnMessage = (String)result["response"];
            AccountResponse ContactResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountResponse>(ReturnMessage);
            String ErrorDetails = ContactResponseObject.message;
            bool ContainsErrorMessageToRole = ErrorDetails.Contains("To role is mandatory.");


            StringAssert.Contains(ErrorDetails, AccountNameLengthErrorMessage, "Account name length validation failed.");
            StringAssert.Contains(ErrorDetails, CompanyHouseLengthLimitErrorMsg, "Company house length validation failed.");
            StringAssert.Contains(ErrorDetails, EmailAddressMoreLengthValidation, "Email address length validation failed.");
            StringAssert.Contains(ErrorDetails, UprnLengthERrorMessage, "UPRN length  validation not working.");
            StringAssert.Contains(ErrorDetails, BuildingNameCannotEmptyErrMsg, "Building name  validation not working.");
            StringAssert.Contains(ErrorDetails, BuildingNumberLengthErroMsg, "Building number  validation not working.");
            StringAssert.Contains(ErrorDetails, StreetCannotMoreThan100ErrMsg, "Street character length cannot be more than 100 .");
            StringAssert.Contains(ErrorDetails, LocalityLengthErrorMsg, "Locality length validation not working.");
            StringAssert.Contains(ErrorDetails, TownLengthErrorMsg, "Town length validation not working.");
            StringAssert.Contains(ErrorDetails, PostCodeLengthErrorMsg, "Post code length validation failed.");
            StringAssert.Contains(ErrorDetails, ISOCodeLengthErrMsg, "ISO code field length validation failed.");

        }

        [TestMethod]
        public void DuplicateCheck_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                  {
                       'organisationid': 'b7293664-e46a-e811-a83c-000d3ab4f967',
                      'name': 'orgname',
                      'type': '910400000',
                      'crn': '142923',
                      'email': 'email@email.com',
                      'address': {
                        'type': '3',
                        'uprn': '12345',
                        'buildingname': 'test',
                        'buildingnumber': '1234',
                        'street': 'street',
                        'locality': 'local',
                        'town': 'town',
                        'postcode': 'HA9 7AH',
                        'country': 'ABC',
                        'fromcompanieshouse': 'true'
                      },
                      'telephone': '004412345678',
                      'hierarchylevel': '910400000',
                     
                    }
                ";


            //Inputs
            var inputs = new Dictionary<string, object>() {
                { "request", InputLoad },
                };

            var account = fakedContext.CreateQuery<Account>();

            Account AccountObject = new Account {AccountId = new Guid("b7293664-e46a-e811-a83c-000d3ab4f967")
                                             ,defra_companyhouseid = "142923"
            };

            fakedContext.Initialize(new List<Entity>()
            {   AccountObject
            });

            var result = fakedContext.ExecuteCodeActivity<CreateOrganisation>(inputs);

            #region ErrorMessagesToCheck




            #endregion

            String ReturnMessage = (String)result["response"];
            AccountResponse ContactResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountResponse>(ReturnMessage);
            StringAssert.Contains(ContactResponseObject.code.ToString(), "412");



        }


        [TestMethod]
        public void AddressTypeCheck_Success()
        {
            var fakedContext = new XrmFakedContext();
            //input object does not contain to record id which is mandatory.
            string InputLoad = @"
                  {
                       'organisationid': 'b7293664-e46a-e811-a83c-000d3ab4f967',
                      'name': 'orgname',
                      'type': '910400000',
                      'crn': '142923',
                      'email': 'email@email.com',
                      'address': {
                        'type': '5',
                        'uprn': '12345',
                        'buildingname': 'test',
                        'buildingnumber': '1234',
                        'street': 'street',
                        'locality': 'local',
                        'town': 'town',
                        'postcode': 'HA9 7AH',
                        'country': 'ABC',
                        'fromcompanieshouse': 'true'
                      },
                      'telephone': '004412345678',
                      'hierarchylevel': '910400000',
                     
                    }
                ";


            //Inputs
            var inputs = new Dictionary<string, object>() {
                { "request", InputLoad },
                };

            var account = fakedContext.CreateQuery<Account>();

            Account AccountObject = new Account
            {
                AccountId = new Guid("b7293664-e46a-e811-a83c-000d3ab4f967")
                                             ,
                defra_companyhouseid = "142923"
            };

            fakedContext.Initialize(new List<Entity>()
            {   AccountObject
            });

            var result = fakedContext.ExecuteCodeActivity<CreateOrganisation>(inputs);

            #region ErrorMessagesToCheck




            #endregion

            String ReturnMessage = (String)result["response"];
            AccountResponse ContactResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountResponse>(ReturnMessage);
            String ErrorDetails = ContactResponseObject.message;

            StringAssert.Contains(ErrorDetails, "Option set value for address of type 5 not found");
        }


    }




    }
