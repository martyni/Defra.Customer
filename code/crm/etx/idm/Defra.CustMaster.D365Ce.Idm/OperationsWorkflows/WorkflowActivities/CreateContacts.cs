using Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.WorkflowActivities
{
  class CreateContacts : CodeActivity
    {
        #region "Parameter Definition"

        [RequiredArgument]
        [Input("payload")]
        public InArgument<String> Payload { get; set; }

        [Output("Is Record Created")]
        public OutArgument<Boolean> IsRecordCreated { get; set; }

        [Output("CRMGuid")]
        public OutArgument<String> CrmGuid { get; set; }
        [Output("UniqueReference")]
        public OutArgument<String> UniqueReference { get; set; }

        [Output("Code")]
        public OutArgument<String> Code { get; set; }

        [Output("Message")]
        public OutArgument<string> Message { get; set; }

        [Output("MessageDetail")]
        public OutArgument<string> MessageDetail { get; set; }

        #endregion
        Common objCommon;

        protected override void Execute(CodeActivityContext executionContext)
        {
            #region "Load CRM Service from context"
            objCommon = new Common(executionContext);

            objCommon.tracingService.Trace("CreateContact activity:Load CRM Service from context --- OK");
            #endregion

            #region "Read Parameters"



            //EntityReference _Contact;
            Boolean _IsRecordCreated = false;
            Int64 ErrorCode = 400; //Bad Request
            String _ErrorMessage = string.Empty;
            String _ErrorMessageDetail = string.Empty;
            Guid ContactId = Guid.Empty;
            Guid ContactRecordGuidWithEmail = Guid.Empty;
            #endregion

            #region "Create Execution"

            try
            {
                string jsonPayload = Payload.Get(executionContext);
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonPayload)))
                {
                    DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Contact));
                    //Contact contactPayload = JsonConvert.DeserializeObject<Contact>(jsonPayload);
                    //Contact contactPayload = jsonPayload.FromJson<Contact>();

                    Contact contactPayload = (Contact)deserializer.ReadObject(ms);
                    objCommon.tracingService.Trace("deseriaized contact" + contactPayload.b2cobjectid);
                    Entity contact = new Entity("contact");//,"defra_upn", _UPN);
                    _ErrorMessage = FieldValidation(contactPayload);

                    if (_ErrorMessage == string.Empty)
                    {
                        //search contact record based on UPN
                        OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);
                        var ContactWithUPN = from c in orgSvcContext.CreateQuery("contact")
                                             where ((string)c["defra_b2cobjectid"]).Equals((contactPayload.b2cobjectid.Trim()))
                                             select new { ContactId = c.Id, UniqueReference = c["defra_uniquereference"] };

                        var contactRecordWithUPN = ContactWithUPN.FirstOrDefault() == null ? null : ContactWithUPN.FirstOrDefault();

                        if (!string.IsNullOrEmpty(contactPayload.email))
                        {
                            var ContactWithEmail = from c in orgSvcContext.CreateQuery("contact")
                                                   where ((string)c["emailaddress1"]).Equals((contactPayload.email.Trim()))
                                                   select new { ContactId = c.Id, UniqueReference = c["defra_uniquereference"] };

                            var contactRecordWithEmail = ContactWithEmail.FirstOrDefault() == null ? null : ContactWithEmail.FirstOrDefault();
                            ContactRecordGuidWithEmail = contactRecordWithEmail == null ? Guid.Empty : contactRecordWithEmail.ContactId;
                        }

                        Guid ContactRecordGuidWithUPN = contactRecordWithUPN == null ? Guid.Empty : contactRecordWithUPN.ContactId;

                        if (ContactRecordGuidWithUPN == Guid.Empty && ContactRecordGuidWithEmail == Guid.Empty)
                        {
                            objCommon.tracingService.Trace("CreateContact activity:ContactRecordGuidWithUPN is empty started, Creating Contact..");

                            ErrorCode = 200;//Success
                            if (contactPayload.title != null)
                                contact["defra_title"] = new OptionSetValue((int)contactPayload.title);
                            if (contactPayload.firstname != null)
                                contact["firstname"] = contactPayload.firstname;
                            if (contactPayload.lastname != null)
                                contact["lastname"] = contactPayload.lastname;
                            if (contactPayload.middlename != null)
                                contact["middlename"] = contactPayload.middlename;
                            if (contactPayload.email != null)
                                contact["emailaddress1"] = contactPayload.email;
                            if (contactPayload.b2cobjectid != null)
                                contact["defra_b2cobjectid"] = contactPayload.b2cobjectid;
                            if (contactPayload.tacsacceptedversion != null)
                                contact["defra_tacsacceptedversion"] = contactPayload.tacsacceptedversion;
                            if (contactPayload.telephone != null)
                                contact["telephone1"] = contactPayload.telephone;

                            objCommon.tracingService.Trace("setting contact date params:started..");
                            if (!string.IsNullOrEmpty(contactPayload.tacsacceptedon) && !string.IsNullOrWhiteSpace(contactPayload.tacsacceptedon))
                            {
                                objCommon.tracingService.Trace("date accepted on in string" + contactPayload.tacsacceptedon);
                                DateTime resultDate;
                                if (DateTime.TryParse(contactPayload.tacsacceptedon, out resultDate))
                                {
                                    objCommon.tracingService.Trace("date accepted on in dateformat" + resultDate);
                                    contact["defra_tacsacceptedon"] = (resultDate);
                                }
                            }

                            //set birthdate
                            if (!string.IsNullOrEmpty(contactPayload.dob) && !string.IsNullOrWhiteSpace(contactPayload.dob))
                            {
                                DateTime resultDob;
                                if (DateTime.TryParse(contactPayload.dob, out resultDob))
                                    contact["birthdate"] = resultDob;
                            }

                            if (contactPayload.gender != null)
                            {

                                contact["gendercode"] = new OptionSetValue((int)contactPayload.gender);
                            }
                            objCommon.tracingService.Trace("CreateContact activity:started..");
                            ContactId = objCommon.service.Create(contact);
                            objCommon.tracingService.Trace("CreateContact activity:ended. " + ContactId.ToString());

                            this.CrmGuid.Set(executionContext, ContactId.ToString());

                            _IsRecordCreated = true;
                            if (contactPayload.address != null)
                            {

                                objCommon.CreateAddress(contactPayload.address, new EntityReference("contact", ContactId));
                            }
                        }
                        else
                        {
                            if (ContactRecordGuidWithUPN != Guid.Empty)
                                this.CrmGuid.Set(executionContext, ContactRecordGuidWithUPN.ToString());
                            else if (ContactRecordGuidWithEmail != Guid.Empty)
                                this.CrmGuid.Set(executionContext, ContactRecordGuidWithEmail.ToString());
                            objCommon.tracingService.Trace("CreateContact activity:ContactRecordGuidWithUPN is found/duplicate started..");
                            ErrorCode = 412;//Duplicate UPN
                            _ErrorMessage = "Duplicate Record";
                        }
                    }
                    objCommon.tracingService.Trace("CreateContact activity:setting output params like error code etc.. started");
                    this.IsRecordCreated.Set(executionContext, _IsRecordCreated);
                    this.Code.Set(executionContext, ErrorCode.ToString());
                    this.Message.Set(executionContext, _ErrorMessage);
                    this.MessageDetail.Set(executionContext, _ErrorMessageDetail);
                    objCommon.tracingService.Trace("CreateContact activity:setting output params like error code etc.. ended");
                }
            }
            catch (Exception ex)
            {
                ErrorCode = 500;//Internal Error
                _ErrorMessage = "Error occured while processing request";
                _ErrorMessageDetail = ex.Message;
                //throw ex;
                this.Code.Set(executionContext, ErrorCode.ToString());
                this.Message.Set(executionContext, _ErrorMessage);
                this.MessageDetail.Set(executionContext, _ErrorMessageDetail);
                objCommon.tracingService.Trace(ex.Message);
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
                _ErrorMessage = "B2C Object Id can not be empty";
            if (string.IsNullOrEmpty(ContactRequest.firstname) || string.IsNullOrWhiteSpace(ContactRequest.firstname))
                _ErrorMessage = "First Name can not empty";
            if (string.IsNullOrEmpty(ContactRequest.lastname) || string.IsNullOrWhiteSpace(ContactRequest.lastname))
                _ErrorMessage = "Last Name can not empty";

            if (!string.IsNullOrEmpty(ContactRequest.b2cobjectid) && !string.IsNullOrWhiteSpace(ContactRequest.b2cobjectid) && ContactRequest.b2cobjectid.Length > 50)
            {
                _ErrorMessage = "B2C Object Id is invalid/exceed the max length(50)";
            }
            if (!string.IsNullOrEmpty(ContactRequest.firstname) && ContactRequest.firstname.Length > 50)
            {

                _ErrorMessage = "First name exceeded the max length(50)";
            }
            if (!string.IsNullOrEmpty(ContactRequest.lastname) && ContactRequest.lastname.Length > 50)
            {

                _ErrorMessage = "Last name exceeded the max length(50)";
            }
            if (!string.IsNullOrEmpty(ContactRequest.middlename) && ContactRequest.middlename.Length > 50)
            {

                _ErrorMessage = "Middle name exceeded the max length(50)";
            }
            if (!string.IsNullOrEmpty(ContactRequest.email) && ContactRequest.email.Length > 100)
            {

                _ErrorMessage = "Email exceeded the max length(100)";
            }
            if (!string.IsNullOrEmpty(ContactRequest.tacsacceptedversion) && ContactRequest.tacsacceptedversion.Length > 5)
            {

                _ErrorMessage = "T&Cs Accepted Version exceeded the max length(5)";
            }
            if (ContactRequest.gender != null)
            {
                bool genderFound=Enum.GetValues(typeof(ContactGenderCodes)).Equals(ContactRequest.gender);
                if(!genderFound)
                    _ErrorMessage = "Gender Code is not valid";
            }
            if (ContactRequest.title != null)
            {
                bool genderFound = Enum.GetValues(typeof(ContactTitles)).Equals(ContactRequest.title);
                if (!genderFound)
                    _ErrorMessage = "Title is not valid";
            }
            return _ErrorMessage;
        }

    }
}