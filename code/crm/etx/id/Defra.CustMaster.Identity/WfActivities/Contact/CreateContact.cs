using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SCS = Defra.CustMaster.D365.Common.schema;
using SCSE = Defra.CustMaster.D365.Common.Schema.ExtEnums;
using SCII = Defra.CustMaster.D365.Common.Ints.Idm;
using SCIIR = Defra.CustMaster.D365.Common.Ints.Idm.Resp;

namespace Defra.CustMaster.Identity.WfActivities
{
    /// <summary>
    /// {'b2cobjectid':'b2c12062018-cd20180618','title':1,'firstname':'idm.frist.cd20180618','lastname':'idm.last.cd20180618','email':'idm.cd20180618@customer.com','dob':'06/07/2018','gender':2,'telephone':'004412345678','tacsacceptedversion':'12345','tacsacceptedon':'10/09/2018 06:06:06','address':{'type':1,'uprn':20180618,'buildingname':'Horizon','buildingnumber':'3','street':'deanary','locality':'','town':'','postcode':'hp98tj','country':'gbr','fromcompanieshouse':''}}
    /// </summary>
    public class CreateContact : WorkFlowActivityBase
    {
        #region "Parameter Definition"

        [RequiredArgument]
        [Input("request")]
        public InArgument<String> Payload { get; set; }

        [Output("response")]
        public OutArgument<String> Response { get; set; }

        #endregion
       
        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            #region Local Properties
            SCII.Helper objCommon;
            //EntityReference _Contact;
            int _errorCode = 400; //Bad Request
            string _errorMessage = string.Empty;
            string _errorMessageDetail = string.Empty;
            Guid _contactId = Guid.Empty;
            string _uniqueReference = string.Empty;

            #endregion

            #region "Create Execution"
            objCommon = new SCII.Helper(executionContext);

            try
            {
               
                objCommon.tracingService.Trace("CreateContact activity:Load CRM Service from context --- OK");
                string jsonPayload = Payload.Get(executionContext);
                SCII.ContactRequest contactPayload = JsonConvert.DeserializeObject<SCII.ContactRequest>(jsonPayload);

                Entity contact = new Entity(SCS.Contact.ENTITY);//,"defra_upn", _UPN);

                _errorMessage = FieldValidation(contactPayload);
                var ValidationContext = new ValidationContext(contactPayload, serviceProvider: null, items: null);

                ICollection<ValidationResult> ValidationResults = null;
                ICollection<ValidationResult> ValidationResultsAddress = null;

                var isValid = objCommon.Validate(contactPayload, out ValidationResults);


                Boolean isValidAddress = contactPayload.address == null ? true :

                    objCommon.Validate(contactPayload.address, out ValidationResultsAddress);

                objCommon.tracingService.Trace("just after validation");

                if (isValid && isValidAddress)
                {
                    if (_errorMessage == string.Empty)
                    {
                        //search contact record based on key named B2COBJECTID to prevent duplicate contact
                        OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);
                        var ContactWithUPN = from c in orgSvcContext.CreateQuery(SCS.Contact.ENTITY)
                                             where ((string)c[SCS.Contact.B2COBJECTID]).Equals((contactPayload.b2cobjectid.Trim()))
                                             select new { ContactId = c.Id, UniqueReference = c[SCS.Contact.UNIQUEREFERENCE] };

                        var contactRecordWithUPN = ContactWithUPN.FirstOrDefault() == null ? null : ContactWithUPN.FirstOrDefault();
                        if (contactRecordWithUPN != null)
                        {
                            _contactId = contactRecordWithUPN.ContactId;
                            _uniqueReference = contactRecordWithUPN.UniqueReference.ToString();
                        }

                        //Search contact record based on key named emailaddress to prevent duplicates
                        if (!string.IsNullOrEmpty(contactPayload.email))
                        {
                            var ContactWithEmail = from c in orgSvcContext.CreateQuery(SCS.Contact.ENTITY)
                                                   where ((string)c[SCS.Contact.EMAILADDRESS1]).Equals((contactPayload.email.Trim()))
                                                   select new { ContactId = c.Id, UniqueReference = c[SCS.Contact.UNIQUEREFERENCE] };

                            var contactRecordWithEmail = ContactWithEmail.FirstOrDefault() == null ? null : ContactWithEmail.FirstOrDefault();
                            if (contactRecordWithEmail != null)
                            {
                                _contactId = contactRecordWithEmail.ContactId;
                                _uniqueReference = contactRecordWithEmail.UniqueReference.ToString();
                            }
                        }
                        if (_contactId == Guid.Empty)
                        {
                            objCommon.tracingService.Trace("CreateContact activity:ContactRecordGuidWithUPN is empty started, Creating ReqContact..");
                            if (contactPayload.title != null)
                            {
                                contact[SCS.Contact.TITLE] = new OptionSetValue((int)contactPayload.title);
                            }

                            if (contactPayload.firstname != null)
                                contact[SCS.Contact.FIRSTNAME] = contactPayload.firstname;
                            if (contactPayload.lastname != null)
                                contact[SCS.Contact.LASTNAME] = contactPayload.lastname;
                            if (contactPayload.middlename != null)
                                contact[SCS.Contact.MIDDLENAME] = contactPayload.middlename;
                            if (contactPayload.email != null)
                                contact[SCS.Contact.EMAILADDRESS1] = contactPayload.email;
                            if (contactPayload.b2cobjectid != null)
                                contact[SCS.Contact.B2COBJECTID] = contactPayload.b2cobjectid;
                            if (contactPayload.tacsacceptedversion != null)
                                contact[SCS.Contact.TACSACCEPTEDVERSION] = contactPayload.tacsacceptedversion;
                            if (contactPayload.telephone != null)
                                contact[SCS.Contact.TELEPHONE1] = contactPayload.telephone;

                            objCommon.tracingService.Trace("setting contact date params:started..");

                            //set tcsaccepteddate 
                            if (!string.IsNullOrEmpty(contactPayload.tacsacceptedon) && !string.IsNullOrWhiteSpace(contactPayload.tacsacceptedon))
                            {
                                objCommon.tracingService.Trace("date accepted on in string" + contactPayload.tacsacceptedon);
                                DateTime resultDate;
                                if (DateTime.TryParse(contactPayload.tacsacceptedon, out resultDate))
                                {
                                    objCommon.tracingService.Trace("date accepted on in dateformat" + resultDate);
                                    contact[SCS.Contact.TACSACCEPTEDON] = (resultDate);
                                }
                            }

                            //set birthdate
                            if (!string.IsNullOrEmpty(contactPayload.dob) && !string.IsNullOrWhiteSpace(contactPayload.dob))
                            {
                                DateTime resultDob;
                                if (DateTime.TryParse(contactPayload.dob, out resultDob))
                                    contact[SCS.Contact.BIRTHDATE] = resultDob;
                            }

                            if (contactPayload.gender != null)
                            {
                                contact[SCS.Contact.GENDERCODE] = new OptionSetValue((int)contactPayload.gender);
                            }

                            objCommon.tracingService.Trace("CreateContact activity:started..");
                            _contactId = objCommon.service.Create(contact);
                            Entity contactRecord = objCommon.service.Retrieve(SCS.Contact.ENTITY, _contactId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));//Defra.CustMaster.D365.Common.schema.ReqContact.UNIQUEREFERENCE));
                            objCommon.tracingService.Trace((string)contactRecord[SCS.Contact.UNIQUEREFERENCE]);
                            _uniqueReference = (string)contactRecord[SCS.Contact.UNIQUEREFERENCE];
                            _errorCode = 200;//Success
                            objCommon.tracingService.Trace("CreateContact activity:ended. " + _contactId.ToString());

                            //create contact address and contact details
                            if (contactPayload.address != null)
                            {
                                objCommon.CreateAddress(contactPayload.address, new EntityReference(D365.Common.schema.Contact.ENTITY, _contactId));
                            }
                        }
                        else
                        {
                            objCommon.tracingService.Trace("CreateContact activity:ContactRecordGuidWithB2C/Email is found/duplicate.");
                            _errorCode = 412;//Duplicate UPN
                            _errorMessage = "Duplicate Record";
                        }
                    }
                }
                else
                {
                    objCommon.tracingService.Trace("inside validation result");


                    StringBuilder ErrorMessage = new StringBuilder();
                    //this will throw an error
                    foreach (ValidationResult vr in ValidationResults)
                    {
                        ErrorMessage.Append(vr.ErrorMessage + " ");
                    }
                    if (contactPayload.address != null)
                        foreach (ValidationResult vr in ValidationResultsAddress)
                        {
                            ErrorMessage.Append(vr.ErrorMessage + " ");
                        }
                    _errorCode = 400;
                    _errorMessage = ErrorMessage.ToString();
                }
                objCommon.tracingService.Trace("CreateContact activity:setting output params like error code etc.. started");
                objCommon.tracingService.Trace("CreateContact activity:setting output params like error code etc.. ended");

            }
            catch (Exception ex)
            {
                _errorCode = 500;//Internal Error
                _errorMessage = "Error occured while processing request";
                _errorMessageDetail = ex.Message;
                objCommon.tracingService.Trace(ex.Message);
                //throw ex;                

            }
            finally
            {
                objCommon.tracingService.Trace("finally block start");
                SCIIR.ContactResponse responsePayload = new SCIIR.ContactResponse()
                {
                    code = _errorCode,
                    message = _errorMessage,
                    datetime = DateTime.UtcNow,
                    version = "1.0.0.2",
                    program = "CreateContact",
                    status = _errorCode == 200 || _errorCode == 412 ? "success" : "failure",
                    data = new SCIIR.ContactData()
                    {
                        contactid = _contactId == Guid.Empty ? null : _contactId.ToString(),
                        uniquereference = _uniqueReference == string.Empty ? null : _uniqueReference,
                        error = new SCIIR.ResponseErrorBase() { details = _errorMessageDetail == string.Empty ? _errorMessage : _errorMessageDetail }
                    }

                };

                string resPayload = JsonConvert.SerializeObject(responsePayload);
                Response.Set(executionContext, resPayload);
                objCommon.tracingService.Trace("finally block end");
            }
            #endregion
        }

        /// <summary>
        /// FieldValidation
        /// </summary>
        /// <param name="ContactRequest"></param>
        /// <returns></returns>
        string FieldValidation(SCII.ContactRequest ContactRequest)
        {
            string _ErrorMessage = string.Empty;

            //if (string.IsNullOrEmpty(ContactRequest.b2cobjectid) || string.IsNullOrWhiteSpace(ContactRequest.b2cobjectid))
            //{
            //    _ErrorMessage = "B2C Object Id can not be empty";
            //}
            //else if (string.IsNullOrEmpty(ContactRequest.firstname) || string.IsNullOrWhiteSpace(ContactRequest.firstname))
            //    _ErrorMessage = "First Name can not empty";
            //else if (string.IsNullOrEmpty(ContactRequest.lastname) || string.IsNullOrWhiteSpace(ContactRequest.lastname))
            //    _ErrorMessage = "Last Name can not empty";

            //else if (!string.IsNullOrEmpty(ContactRequest.b2cobjectid) && !string.IsNullOrWhiteSpace(ContactRequest.b2cobjectid) && ContactRequest.b2cobjectid.Length > 50)
            //{
            //    _ErrorMessage = "B2C Object Id is invalid/exceed the max length(50)";
            //}
            //else if (!string.IsNullOrEmpty(ContactRequest.firstname) && ContactRequest.firstname.Length > 50)
            //{

            //    _ErrorMessage = "First name exceeded the max length(50)";
            //}
            //else if (!string.IsNullOrEmpty(ContactRequest.lastname) && ContactRequest.lastname.Length > 50)
            //{

            //    _ErrorMessage = "Last name exceeded the max length(50)";
            //}
            //else if (!string.IsNullOrEmpty(ContactRequest.middlename) && ContactRequest.middlename.Length > 50)
            //{

            //    _ErrorMessage = "Middle name exceeded the max length(50)";
            //}
            //else if (!string.IsNullOrEmpty(ContactRequest.email) && ContactRequest.email.Length > 100)
            //{

            //    _ErrorMessage = "Email exceeded the max length(100)";
            //}
            //else if (!string.IsNullOrEmpty(ContactRequest.tacsacceptedversion) && ContactRequest.tacsacceptedversion.Length > 5)
            //{

            //    _ErrorMessage = "T&Cs Accepted Version exceeded the max length(5)";
            //}
            //else 
            if (ContactRequest.gender != null)
            {
                bool genderFound = Enum.IsDefined(typeof(SCSE.Contact_GenderCode), ContactRequest.gender);
                if (!genderFound)
                    _ErrorMessage = String.Format("Option set value {0} for gender  not found;", ContactRequest.gender);
            }
            if (ContactRequest.title != null)
            {
                bool titleFound = Enum.IsDefined(typeof(SCSE.defra_Title), ContactRequest.title);
                if (!titleFound)
                    _ErrorMessage = String.Format("Option set value {0} for title not found;", ContactRequest.title);
            }
            if (ContactRequest.address != null && ContactRequest.address.type != null)
            {
                if (!Enum.IsDefined(typeof(SCSE.defra_AddressType), ContactRequest.address.type))
                {
                    _ErrorMessage = String.Format("Option set value for address of type {0} not found;", ContactRequest.address.type);
                }
            }
            return _ErrorMessage;
        }

    }
}
