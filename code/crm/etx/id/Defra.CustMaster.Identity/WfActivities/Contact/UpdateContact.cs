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
    public class UpdateContact : WorkFlowActivityBase
    {
        #region "Parameter Definition"
        [RequiredArgument]
        [Input("PayLoad")]
        public InArgument<String> PayLoad { get; set; }
        [Output("OutPutJson")]
        public OutArgument<string> ReturnMessageDetails { get; set; }
        #endregion
        #region Local Properties
        SCII.Helper objCommon;
        //EntityReference _Contact;
        int _errorCode = 400; //Bad Request
        string _errorMessage = string.Empty;
        string _errorMessageDetail = string.Empty;
        Guid _contactId = Guid.Empty;
        string _uniqueReference = string.Empty;

        #endregion
        protected override void Execute(CodeActivityContext context)
        {

            // Construct the Local plug-in context.
            LocalWorkflowContext localcontext = new LocalWorkflowContext(context);
            localcontext.Trace("started execution");

            #region "Load CRM Service from context"
            objCommon = new SCII.Helper(context);

            localcontext.Trace("CreateContact activity:Load CRM Service from context --- OK");
            #endregion

            #region "Create Execution"

            try
            {
                string jsonPayload = this.PayLoad.Get(context);
                SCII.ContactUpdate contactPayload = JsonConvert.DeserializeObject<SCII.ContactUpdate>(jsonPayload);
                Boolean duplicateRecordExist = false;
                Entity contact;
                var ValidationContext = new ValidationContext(contactPayload, serviceProvider: null, items: null);
                ICollection<ValidationResult> ValidationResults = null;
                ICollection<ValidationResult> ValidationResultsAddress = null;

                var isValid = objCommon.Validate(contactPayload, out ValidationResults);
                localcontext.Trace("just after validation");

                if (isValid )//&& isValidAddress)
                {
                    if (_errorMessage == string.Empty)
                    {
                        //search contact record based on key named B2COBJECTID 
                        OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);
                        var ContactWithUPN = from c in orgSvcContext.CreateQuery(SCS.Contact.ENTITY)
                                             where ((Guid)c[SCS.Contact.Id]).Equals((contactPayload.contactid))
                                             select new { ContactId = c.Id, UniqueReference = c[SCS.Contact.UNIQUEREFERENCE] };

                        var contactRecordWithUPN = ContactWithUPN.FirstOrDefault() == null ? null : ContactWithUPN.FirstOrDefault();
                        if (contactRecordWithUPN != null)
                        {
                            _contactId = contactRecordWithUPN.ContactId;
                            _uniqueReference = contactRecordWithUPN.UniqueReference.ToString();
                             

                            //Search contact record based on key named emailaddress to prevent duplicates
                            if (!string.IsNullOrEmpty(contactPayload.updates.email))
                            {
                                localcontext.Trace("searching for contact ignoring current record");

                                //compare with record ignoring current record
                                var ContactWithEmail = from c in orgSvcContext.CreateQuery(SCS.Contact.ENTITY)
                                                       where ((string)c[SCS.Contact.EMAILADDRESS1]) == contactPayload.updates.email.Trim()
                                                       && (string)c[SCS.Contact.UNIQUEREFERENCE] != _uniqueReference
                                                       select new { ContactId = c.Id, UniqueReference = c[SCS.Contact.UNIQUEREFERENCE] };
                                var contactRecordWithEmail = ContactWithEmail.FirstOrDefault() == null ? null : ContactWithEmail.FirstOrDefault();
                                duplicateRecordExist = contactRecordWithEmail == null ? false : true;
                                localcontext.Trace("duplicate check: " + duplicateRecordExist);

                            }
                            if (!duplicateRecordExist)
                            {
                                contact = new Entity(SCS.Contact.ENTITY, _contactId);
                                localcontext.Trace("update activity:ContactRecordGuidWithUPN is empty started, update ReqContact..");

                                if (contactPayload.updates.title.HasValue && !String.IsNullOrEmpty(Enum.GetName(typeof(SCSE.defra_Title), contactPayload.updates.title)))
                                { 
                                    contact[SCS.Contact.TITLE] = new OptionSetValue(contactPayload.updates.title.Value);
                                }
                                    localcontext.Trace("setting contact date params:started..");
                                if (!string.IsNullOrEmpty(contactPayload.updates.tacsacceptedon) && !string.IsNullOrWhiteSpace(contactPayload.updates.tacsacceptedon))
                                {
                                    localcontext.Trace("date accepted on in string" + contactPayload.updates.tacsacceptedon);
                                    DateTime resultDate;
                                    if (DateTime.TryParse(contactPayload.updates.tacsacceptedon, out resultDate))
                                    {
                                        localcontext.Trace("date accepted on in dateformat" + resultDate);
                                        contact[SCS.Contact.TACSACCEPTEDON] = (resultDate);
                                    }
                                }

                                if (contactPayload.updates.firstname != null)
                                    contact[SCS.Contact.FIRSTNAME] = contactPayload.updates.firstname;
                                if (contactPayload.updates.lastname != null)
                                    contact[SCS.Contact.LASTNAME] = contactPayload.updates.lastname;
                                if (contactPayload.updates.middlename != null)
                                    contact[SCS.Contact.MIDDLENAME] = contactPayload.updates.middlename;
                                if (contactPayload.updates.email != null)
                                    contact[SCS.Contact.EMAILADDRESS1] = contactPayload.updates.email;
                                
                                if (contactPayload.updates.tacsacceptedversion != null)
                                    contact[SCS.Contact.TACSACCEPTEDVERSION] = contactPayload.updates.tacsacceptedversion;
                                if (contactPayload.updates.telephone != null)
                                    contact[SCS.Contact.TELEPHONE1] = contactPayload.updates.telephone;


                                //set birthdate
                                if (!string.IsNullOrEmpty(contactPayload.updates.dob) && !string.IsNullOrWhiteSpace(contactPayload.updates.dob))
                                {
                                    DateTime resultDob;
                                    if (DateTime.TryParse(contactPayload.updates.dob, out resultDob))
                                        contact[SCS.Contact.GENDERCODE] = resultDob;
                                }

                                if (contactPayload.updates.gender.HasValue && !String.IsNullOrEmpty(Enum.GetName(typeof(SCSE.Contact_GenderCode), contactPayload.updates.gender)))
                                {
                                    contact[SCS.Contact.GENDERCODE] = new OptionSetValue(contactPayload.updates.gender.Value);
                                }


                                if (contactPayload.updates.gender != null)
                                {
                                    //Check whether the gendercode is found in GenderEnum mapping
                                    if (Enum.IsDefined(typeof(SCII.ContactGenderCodes), contactPayload.updates.gender))
                                    {
                                        //Check whether gendercode is found in Dynamics GenderEnum mapping
                                        string genderCode = Enum.GetName(typeof(SCSE.Contact_GenderCode), contactPayload.updates.gender);
                                        {
                                            SCSE.Contact_GenderCode dynamicsGenderCode = (SCSE.Contact_GenderCode) Enum.Parse(typeof(SCSE.Contact_GenderCode), genderCode);
                                            contact[SCS.Contact.GENDERCODE] = new OptionSetValue((int)dynamicsGenderCode);
                                        }
                                    }
                                }
                                localcontext.Trace("contactid: " + _contactId);
                                objCommon.service.Update(contact);
                                _errorCode = 200;//Success
                                localcontext.Trace("CreateContact activity:ended. " + _contactId.ToString());
                            }
                            else
                            {
                                localcontext.Trace("CreateContact activity:ContactRecordGuidWithB2C/Email is found/duplicate.");
                                _errorCode = 412;//Duplicate UPN
                                _errorMessage = "Duplicate Record";
                            }

                        }
                        else
                        {
                            {
                                localcontext.Trace("record does not exists");
                                _errorCode = 404;//record does not exists
                                _errorMessage = "record does not exists.";
                            }
                        }
                    }
                }
                else
                {
                    localcontext.Trace("inside validation result");


                    StringBuilder ErrorMessage = new StringBuilder();
                    //this will throw an error
                    foreach (ValidationResult vr in ValidationResults)
                    {
                        ErrorMessage.Append(vr.ErrorMessage + " ");
                    }
                    //if (contactPayload.updates.address != null)
                    //    foreach (ValidationResult vr in ValidationResultsAddress)
                    //    {
                    //        ErrorMessage.Append(vr.ErrorMessage + " ");
                    //    }
                    _errorCode = 400;
                    _errorMessage = ErrorMessage.ToString();
                }
                localcontext.Trace("CreateContact activity:setting output params like error code etc.. started");
                localcontext.Trace("CreateContact activity:setting output params like error code etc.. ended");

            }
            catch (Exception ex)
            {
                _errorCode = 500;//Internal Error
                _errorMessage = "Error occured while processing request";
                _errorMessageDetail = ex.Message;
                localcontext.Trace(ex.Message);
                //throw ex;                

            }
            finally
            {
                localcontext.Trace("finally block start");
                SCIIR.ContactResponse responsePayload = new SCIIR.ContactResponse()
                {
                    code = _errorCode,
                    message = _errorMessage,
                    datetime = DateTime.UtcNow,
                    version = "1.0.0.2",
                    program = "UpdateContact",
                    status = _errorCode == 200 || _errorCode == 412 ? "success" : "failure",
                    data = new SCIIR.ContactData()
                    {
                        contactid = _contactId == Guid.Empty ? null : _contactId.ToString(),
                        uniquereference = _uniqueReference == string.Empty ? null : _uniqueReference,
                        error = new SCIIR.ResponseErrorBase() { details = _errorMessageDetail == string.Empty ? _errorMessage : _errorMessageDetail }
                    }

                };

                string resPayload = JsonConvert.SerializeObject(responsePayload);
                ReturnMessageDetails.Set(context, resPayload);
                localcontext.Trace("finally block end");
            }

            #endregion
            ExecuteCRMWorkFlowActivity(context, localcontext);

        }

    }



}
