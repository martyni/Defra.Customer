using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Workflow;
using System.Runtime.Serialization.Json;
using System;
using System.Activities;
using System.Linq;
using System.ServiceModel;
using System.IO;
using System.Text;
using Defra.CustMaster.D365Ce.Idm.OerationsWorkflows.Model;

namespace Defra.CustMaster.D365Ce.Idm.OerationsWorkflows.WorkflowActivities
{
    public class CreateContact : CodeActivity
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
            string jsonPayload = Payload.Get(executionContext);
            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Contact));
            //Contact contactPayload = JsonConvert.DeserializeObject<Contact>(jsonPayload);


            //EntityReference _Contact;
            Boolean _IsRecordCreated = false;
            Int64 ErrorCode = 400; //Bad Request
            String _ErrorMessage = string.Empty;
            String _ErrorMessageDetail = string.Empty;
            Guid ContactId = Guid.Empty;
            #endregion

            #region "Create Execution"

            try
            {
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonPayload)))
                {
                    Contact contactPayload = (Contact)deserializer.ReadObject(ms);
                    objCommon.tracingService.Trace("deseriaized contact" + contactPayload.b2cobjectid);
                    Entity contact = new Entity("contact");//,"defra_upn", _UPN);
                    if (string.IsNullOrEmpty(contactPayload.b2cobjectid) || string.IsNullOrWhiteSpace(contactPayload.b2cobjectid))
                        _ErrorMessage = "B2C Object Id can not be empty";
                    if (string.IsNullOrEmpty(contactPayload.firstname) || string.IsNullOrWhiteSpace(contactPayload.firstname))
                        _ErrorMessage = "First Name can not empty";
                    if (string.IsNullOrEmpty(contactPayload.lastname) || string.IsNullOrWhiteSpace(contactPayload.lastname))
                        _ErrorMessage = "Last Name can not empty";

                    if (!string.IsNullOrEmpty(contactPayload.b2cobjectid) && !string.IsNullOrWhiteSpace(contactPayload.b2cobjectid) && contactPayload.b2cobjectid.Length > 50)
                    {
                        _ErrorMessage = "B2C Object Id is invalid/exceed the max length(50)";
                    }
                    if (!string.IsNullOrEmpty(contactPayload.firstname) && contactPayload.firstname.Length > 50)
                    {

                        _ErrorMessage = "Firstname exceeded the max length(50)";
                    }
                    if (!string.IsNullOrEmpty(contactPayload.lastname) && contactPayload.lastname.Length > 50)
                    {

                        _ErrorMessage = "Lastname exceeded the max length(50)";
                    }
                    if (!string.IsNullOrEmpty(contactPayload.lastname) && contactPayload.lastname.Length > 100)
                    {

                        _ErrorMessage = "Email exceeded the max length(100)";
                    }

                    if (_ErrorMessage == string.Empty)
                    {
                        //search contact record based on UPN
                        OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);
                        var ContactWithUPN = from c in orgSvcContext.CreateQuery("contact")
                                             where ((string)c["defra_b2cobjectid"]).Equals((contactPayload.b2cobjectid.Trim()))
                                             select new { ContactId = c.Id, UniqueReference = c["defra_uniquereference"] };

                        var contactRecordWithUPN = ContactWithUPN.FirstOrDefault() == null ? null : ContactWithUPN.FirstOrDefault();
                        Guid ContactRecordGuidWithUPN = contactRecordWithUPN == null ? Guid.Empty : contactRecordWithUPN.ContactId;

                        if (ContactRecordGuidWithUPN == Guid.Empty)
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
                                DateTime resultDate;
                                if (DateTime.TryParse(contactPayload.tacsacceptedon, out resultDate))
                                    contact["defra_tacsacceptedon"] = resultDate;
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
                            //var CreatedContacts = from c in orgSvcContext.CreateQuery("contact")
                            //                      where ((Guid)c["contactid"]).Equals((ContactId))
                            //                      select new { ContactUID = c["defra_uniquereference"] };
                            //var CreatedContact = CreatedContacts.FirstOrDefault() == null ? null : CreatedContacts.FirstOrDefault();
                            //if (CreatedContact != null)
                            //    UID.Set(executionContext, CreatedContact.ContactUID);
                            this.CrmGuid.Set(executionContext, ContactId.ToString());
                            // Entity ContactRecord = objCommon.service.Retrieve("contact", ContactId, new Microsoft.Xrm.Sdk.Query.ColumnSet("defra_uniquereference"));
                            // this.UniqueReference.Set(executionContext, ContactRecord["defra_uniquereference"]);

                            // _Contact = new EntityReference("contact", ContactId);                   
                            // this.Individual.Set(executionContext, _Contact);
                            _IsRecordCreated = true;
                            if (contactPayload.address != null)
                            {
                                //CreateAddress(contactPayload.address, ContactId);
                                objCommon.CreateAddress(contactPayload.address, new EntityReference("contact", ContactId));
                            }
                        }
                        else
                        {
                            this.CrmGuid.Set(executionContext, ContactRecordGuidWithUPN.ToString());
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
            catch (FaultException<OrganizationServiceFault> ex)
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

            #endregion

        }

        public void CreateAddress(Address addressDetails, Guid contactId)
        {
            objCommon.tracingService.Trace("CreateAddress activity:started..");
            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);
            Entity address = new Entity("defra_address");
            address["defra_uprn"] = addressDetails.uprn;
            address["defra_name"] = addressDetails.buildingname;
            address["defra_premises"] = addressDetails.buildingnumber + "," + addressDetails.buildingname;
            address["defra_street"] = addressDetails.street;
            address["defra_locality"] = addressDetails.locality;
            address["defra_towntext"] = addressDetails.town;
            address["defra_postcode"] = addressDetails.postcode;
            bool resultCompanyHouse;
            if (Boolean.TryParse(addressDetails.fromcompanieshouse, out resultCompanyHouse))
                address["defra_fromcompanieshouse"] = resultCompanyHouse;
            var CountryRecord = from c in orgSvcContext.CreateQuery("defra_country")
                                where ((string)c["defra_name"]).ToLower().Contains((addressDetails.county.Trim().ToLower()))
                                select new { CountryId = c.Id };
            Guid countryGuid = CountryRecord != null && CountryRecord.FirstOrDefault() != null ? CountryRecord.FirstOrDefault().CountryId : Guid.Empty;
            if (countryGuid != Guid.Empty)
                address["defra_country"] = new EntityReference("defra_country", countryGuid);
            objCommon.tracingService.Trace("CreateAddress activity:creating address..");
            Guid addressId = objCommon.service.Create(address);
            objCommon.tracingService.Trace("CreateAddress activity:created address..");
            if (addressId != Guid.Empty)
            {
                objCommon.tracingService.Trace("CreateAddressDetails activity:started..");
                Entity contactDetails = new Entity("defra_addressdetails");
                contactDetails["defra_address"] = new EntityReference("defra_address", addressId);
                int resultAddressType;
                if (int.TryParse(addressDetails.type, out resultAddressType))
                {
                    contactDetails["defra_addresstype"] = new OptionSetValue(resultAddressType);
                }
                contactDetails["defra_customer"] = new EntityReference("contact", contactId);
                objCommon.tracingService.Trace("CreateAddressDetails activity:creating address details..");
                Guid contactDetailId = objCommon.service.Create(contactDetails);
                objCommon.tracingService.Trace("CreateAddressDetails activity:created address details..");
            }

        }

    }
}


