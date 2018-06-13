using System;
using System.Collections.Generic;
using Defra.CustMaster.D365.Common.Schema.ExtEnums;
using Defra.CustMaster.D365.Common.Ints.Idm;
using Defra.CustMaster.D365.Common.Ints.Idm.Resp;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.WorkflowActivities
{
    public class CreateContactNew : CodeActivity
    {
        #region "Parameter Definition"

        [RequiredArgument]
        [Input("requestpayload")]
        public InArgument<String> Payload { get; set; }

        [Output("responsepayload")]
        public OutArgument<String> Response { get; set; }

        [Input("inputerrorcode")]
        public InArgument<int> InputCode { get; set; }

        [Output("outputerrorcode")]
        public OutArgument<int> OutputCode { get; set; }

        #endregion
        #region Local Properties
        Helper objCommon;
        //EntityReference _Contact;
        int _errorCode = 400; //Bad Request
        string _errorMessage = string.Empty;
        string _errorMessageDetail = string.Empty;
        Guid _contactId = Guid.Empty;
        string _uniqueReference = string.Empty;

        #endregion
        protected override void Execute(CodeActivityContext executionContext)
        {
            #region "Load CRM Service from context"
            objCommon = new Helper(executionContext);

            objCommon.tracingService.Trace("CreateContact activity:Load CRM Service from context --- OK");
            #endregion

            #region "Create Execution"

            try
            {
                string jsonPayload = Payload.Get(executionContext);
                int inputCode = InputCode.Get(executionContext);
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonPayload)))
                {
                    DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Contact));
                    //Contact contactPayload = JsonConvert.DeserializeObject<Contact>(jsonPayload);
                    //Contact contactPayload = jsonPayload.FromJson<Contact>();

                    Contact contactPayload = (Contact)deserializer.ReadObject(ms);
                    objCommon.tracingService.Trace("deseriaized contact" + contactPayload.b2cobjectid);

                    Entity contact = new Entity(D365.Common.schema.Contact.ENTITY);//,"defra_upn", _UPN);
                    if (inputCode == 200)
                    {
                        //search contact record based on key named B2COBJECTID to prevent duplicate contact
                        OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);
                        var ContactWithUPN = from c in orgSvcContext.CreateQuery(D365.Common.schema.Contact.ENTITY)
                                             where ((string)c[D365.Common.schema.Contact.B2COBJECTID]).Equals((contactPayload.b2cobjectid.Trim()))
                                             select new { ContactId = c.Id, UniqueReference = c[D365.Common.schema.Contact.UNIQUEREFERENCE] };
                        var contactRecordWithUPN = ContactWithUPN.FirstOrDefault() == null ? null : ContactWithUPN.FirstOrDefault();
                        if (contactRecordWithUPN != null)
                        {
                            _contactId = contactRecordWithUPN.ContactId;
                            _uniqueReference = contactRecordWithUPN.UniqueReference.ToString();
                        }
                        _errorCode = inputCode;

                    }
                    else
                    {
                        _errorMessage = FieldValidation(contactPayload);

                        if (_errorMessage == string.Empty)
                        {
                            //search contact record based on key named B2COBJECTID to prevent duplicate contact
                            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);
                            var ContactWithUPN = from c in orgSvcContext.CreateQuery(D365.Common.schema.Contact.ENTITY)
                                                 where ((string)c[D365.Common.schema.Contact.B2COBJECTID]).Equals((contactPayload.b2cobjectid.Trim()))
                                                 select new { ContactId = c.Id, UniqueReference = c[D365.Common.schema.Contact.UNIQUEREFERENCE] };

                            var contactRecordWithUPN = ContactWithUPN.FirstOrDefault() == null ? null : ContactWithUPN.FirstOrDefault();
                            if (contactRecordWithUPN != null)
                            {
                                _contactId = contactRecordWithUPN.ContactId;
                                _uniqueReference = contactRecordWithUPN.UniqueReference.ToString();
                            }

                            //Search contact record based on key named emailaddress to prevent duplicates
                            if (!string.IsNullOrEmpty(contactPayload.email))
                            {
                                var ContactWithEmail = from c in orgSvcContext.CreateQuery(D365.Common.schema.Contact.ENTITY)
                                                       where ((string)c[D365.Common.schema.Contact.EMAILADDRESS1]).Equals((contactPayload.email.Trim()))
                                                       select new { ContactId = c.Id, UniqueReference = c[D365.Common.schema.Contact.UNIQUEREFERENCE] };

                                var contactRecordWithEmail = ContactWithEmail.FirstOrDefault() == null ? null : ContactWithEmail.FirstOrDefault();
                                if (contactRecordWithEmail != null)
                                {
                                    _contactId = contactRecordWithEmail.ContactId;
                                    _uniqueReference = contactRecordWithEmail.UniqueReference.ToString();
                                }
                            }
                            if (_contactId == Guid.Empty)
                            {
                                objCommon.tracingService.Trace("CreateContact activity:ContactRecordGuidWithUPN is empty started, Creating Contact..");
                                if (contactPayload.title != null)
                                {
                                    //Check whether the gendercode is found in GenderEnum mapping
                                    if (Enum.IsDefined(typeof(ContactTitles), contactPayload.title))
                                    {
                                        //Check whether gendercode is found in Dynamics GenderEnum mapping
                                        string contactTitle = Enum.GetName(typeof(ContactTitles), contactPayload.title);
                                        if (string.IsNullOrEmpty(contactTitle))
                                        {
                                            defra_Title dynamicsTitle = (defra_Title)Enum.Parse(typeof(defra_Title), contactTitle);
                                            contact[D365.Common.schema.Contact.TITLE] = new OptionSetValue((int)dynamicsTitle);
                                        }

                                    }
                                }
                                if (contactPayload.firstname != null)
                                    contact[D365.Common.schema.Contact.FIRSTNAME] = contactPayload.firstname;
                                if (contactPayload.lastname != null)
                                    contact[D365.Common.schema.Contact.LASTNAME] = contactPayload.lastname;
                                if (contactPayload.middlename != null)
                                    contact[D365.Common.schema.Contact.MIDDLENAME] = contactPayload.middlename;
                                if (contactPayload.email != null)
                                    contact[D365.Common.schema.Contact.EMAILADDRESS1] = contactPayload.email;
                                if (contactPayload.b2cobjectid != null)
                                    contact[D365.Common.schema.Contact.B2COBJECTID] = contactPayload.b2cobjectid;
                                if (contactPayload.tacsacceptedversion != null)
                                    contact[D365.Common.schema.Contact.TACSACCEPTEDVERSION] = contactPayload.tacsacceptedversion;
                                if (contactPayload.telephone != null)
                                    contact[D365.Common.schema.Contact.TELEPHONE1] = contactPayload.telephone;

                                objCommon.tracingService.Trace("setting contact date params:started..");
                                if (!string.IsNullOrEmpty(contactPayload.tacsacceptedon) && !string.IsNullOrWhiteSpace(contactPayload.tacsacceptedon))
                                {
                                    objCommon.tracingService.Trace("date accepted on in string" + contactPayload.tacsacceptedon);
                                    DateTime resultDate;
                                    if (DateTime.TryParse(contactPayload.tacsacceptedon, out resultDate))
                                    {
                                        objCommon.tracingService.Trace("date accepted on in dateformat" + resultDate);
                                        contact[D365.Common.schema.Contact.TACSACCEPTEDON] = (resultDate);
                                    }
                                }

                                //set birthdate
                                if (!string.IsNullOrEmpty(contactPayload.dob) && !string.IsNullOrWhiteSpace(contactPayload.dob))
                                {
                                    DateTime resultDob;
                                    if (DateTime.TryParse(contactPayload.dob, out resultDob))
                                        contact[D365.Common.schema.Contact.GENDERCODE] = resultDob;
                                }

                                if (contactPayload.gender != null)
                                {
                                    //Check whether the gendercode is found in GenderEnum mapping
                                    if (Enum.IsDefined(typeof(ContactGenderCodes), contactPayload.gender))
                                    {
                                        //Check whether gendercode is found in Dynamics GenderEnum mapping
                                        string genderCode = Enum.GetName(typeof(Contact_GenderCode), contactPayload.gender);
                                        {
                                            Contact_GenderCode dynamicsGenderCode = (Contact_GenderCode)Enum.Parse(typeof(Contact_GenderCode), genderCode);
                                            contact[D365.Common.schema.Contact.GENDERCODE] = new OptionSetValue((int)dynamicsGenderCode);
                                        }
                                    }
                                }
                                objCommon.tracingService.Trace("CreateContact activity:started..");
                                _contactId = objCommon.service.Create(contact);
                                Entity contactRecord = objCommon.service.Retrieve(D365.Common.schema.Contact.ENTITY, _contactId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));//Defra.CustMaster.D365.Common.schema.Contact.UNIQUEREFERENCE));
                                objCommon.tracingService.Trace((string)contactRecord[Defra.CustMaster.D365.Common.schema.Contact.UNIQUEREFERENCE]);
                                _uniqueReference = (string)contactRecord[Defra.CustMaster.D365.Common.schema.Contact.UNIQUEREFERENCE];
                                _errorCode = 200;//Success
                                objCommon.tracingService.Trace("CreateContact activity:ended. " + _contactId.ToString());

                                //create contact address and contact details
                                //if (contactPayload.address != null)
                                //{
                                //    objCommon.CreateAddress(contactPayload.address, new EntityReference(D365.Common.schema.Contact.ENTITY, _contactId));
                                //}
                            }
                            else
                            {
                                objCommon.tracingService.Trace("CreateContact activity:ContactRecordGuidWithB2C/Email is found/duplicate.");
                                _errorCode = 412;//Duplicate UPN
                                _errorMessage = "Duplicate Record";
                            }
                        }
                        objCommon.tracingService.Trace("CreateContact activity:setting output params like error code etc.. started");

                        objCommon.tracingService.Trace("CreateContact activity:setting output params like error code etc.. ended");
                    }
                }
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
                ContactResponse responsePayload = new ContactResponse()
                {
                    code = _errorCode,
                    message = _errorMessage,
                    datetime = DateTime.UtcNow,
                    version = "1.0.0.2",
                    program = "CreateContact",
                    status = _errorCode == 200 || _errorCode == 412 ? "success" : "failure",
                    data = new ContactData()
                    {
                        contactid = _contactId == Guid.Empty ? null : _contactId.ToString(),
                        uniquereference = _uniqueReference == string.Empty ? null : _uniqueReference,
                        error = new ResponseErrorBase() { details = _errorMessageDetail == string.Empty ? _errorMessage : _errorMessageDetail }
                    }

                    //contactData = new ContactData()
                    //{
                    //    contactid = _contactId == Guid.Empty ? null : _contactId.ToString(),
                    //    uniquereference = _uniqueReference == string.Empty ? null : _uniqueReference,
                    //    error = new ResponseErrorBase() { details = _errorMessageDetail == string.Empty ? _errorMessage : _errorMessageDetail }
                    //}

                };
                MemoryStream ms = new MemoryStream();

                // Serializer the Response object to the stream.  
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ContactResponse));
                ser.WriteObject(ms, responsePayload);

                ms.Position = 0;
                StreamReader sr = new StreamReader(ms);
                string json = sr.ReadToEnd();
                sr.Close();
                ms.Close();
                Response.Set(executionContext, json);
                OutputCode.Set(executionContext, _errorCode);
                objCommon.tracingService.Trace("finally block end");
            }
            //catch (FaultException<OrganizationServiceFault> ex)
            //{
            //    ErrorCode = 500;//Internal Error
            //    _ErrorMessage = "Error occured while processing request";
            //    _ErrorMessageDetail = ex.Message;
            //    //throw ex;
            //    this.Code.Set(executionContext, ErrorCode.ToString());
            //    this.Message.Set(executionContext, _ErrorMessage);
            //    this.MessageDetail.Set(executionContext, _ErrorMessageDetail);
            //    objCommon.tracingService.Trace(ex.Message);

            //}

            #endregion

        }
        string FieldValidation(Contact ContactRequest)
        {
            string _ErrorMessage = string.Empty;
            if (string.IsNullOrEmpty(ContactRequest.b2cobjectid) || string.IsNullOrWhiteSpace(ContactRequest.b2cobjectid))
            {
                _ErrorMessage = "B2C Object Id can not be empty";
            }
            else if (string.IsNullOrEmpty(ContactRequest.firstname) || string.IsNullOrWhiteSpace(ContactRequest.firstname))
                _ErrorMessage = "First Name can not empty";
            else if (string.IsNullOrEmpty(ContactRequest.lastname) || string.IsNullOrWhiteSpace(ContactRequest.lastname))
                _ErrorMessage = "Last Name can not empty";

            else if (!string.IsNullOrEmpty(ContactRequest.b2cobjectid) && !string.IsNullOrWhiteSpace(ContactRequest.b2cobjectid) && ContactRequest.b2cobjectid.Length > 50)
            {
                _ErrorMessage = "B2C Object Id is invalid/exceed the max length(50)";
            }
            else if (!string.IsNullOrEmpty(ContactRequest.firstname) && ContactRequest.firstname.Length > 50)
            {

                _ErrorMessage = "First name exceeded the max length(50)";
            }
            else if (!string.IsNullOrEmpty(ContactRequest.lastname) && ContactRequest.lastname.Length > 50)
            {

                _ErrorMessage = "Last name exceeded the max length(50)";
            }
            else if (!string.IsNullOrEmpty(ContactRequest.middlename) && ContactRequest.middlename.Length > 50)
            {

                _ErrorMessage = "Middle name exceeded the max length(50)";
            }
            else if (!string.IsNullOrEmpty(ContactRequest.email) && ContactRequest.email.Length > 100)
            {

                _ErrorMessage = "Email exceeded the max length(100)";
            }
            else if (!string.IsNullOrEmpty(ContactRequest.tacsacceptedversion) && ContactRequest.tacsacceptedversion.Length > 5)
            {

                _ErrorMessage = "T&Cs Accepted Version exceeded the max length(5)";
            }
            else if (ContactRequest.gender != null)
            {
                bool genderFound = Enum.IsDefined(typeof(ContactGenderCodes), ContactRequest.gender);
                if (!genderFound)
                    _ErrorMessage = "Gender Code is not valid";
            }
            else if (ContactRequest.title != null)
            {
                bool genderFound = Enum.IsDefined(typeof(ContactTitles), ContactRequest.title);
                if (!genderFound)
                    _ErrorMessage = "Title is not valid";
            }
            if (ContactRequest.address!=null&& ContactRequest.address.type != null)
            {
                if (!Enum.IsDefined(typeof(defra_AddressType), ContactRequest.address.type))
                {
                    _ErrorMessage = "AddressType is not valid";
                }
            }
            return _ErrorMessage;
        }

    }
}
