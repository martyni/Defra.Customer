namespace Defra.CustMaster.Identity.WfActivities
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using D365.Common.Ints.Idm.resp;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Workflow;
    using Newtonsoft.Json;
    using SCII = Defra.CustMaster.D365.Common.Ints.Idm;
    using SCIIR = Defra.CustMaster.D365.Common.Ints.Idm.Resp;
    using SCS = Defra.CustMaster.D365.Common.schema;

    public class AddAddress : WorkFlowActivityBase
    {
        #region "Parameter Definition"

        [RequiredArgument]
        [Input("request")]
        public InArgument<string> ReqPayload { get; set; }

        [Output("response")]
        public OutArgument<string> ResPayload { get; set; }

        #endregion


        #region Execute
        protected override void Execute(CodeActivityContext executionContext)
        {
            #region local variables
            SCII.Helper objCommon;

            // EntityReference _Contact;
            int errorCode = 400; // Bad Request
            string errorMessageDetail = string.Empty;
            Guid customerId = Guid.Empty;
            Entity existingAccountRecord = new Entity();
            StringBuilder errorMessage = new StringBuilder();
            bool isRecordIdExists = false;
            AddressData createdAddress = new AddressData() { addressid = Guid.Empty, contactdetailsid = Guid.Empty };
            #endregion
            LocalWorkflowContext localcontext = new LocalWorkflowContext(executionContext);
            objCommon = new SCII.Helper(executionContext);
            try
            {
                localcontext.Trace("started execution");
                localcontext.Trace("attempt to seriallised");

                string jsonPayload = ReqPayload.Get(executionContext);
                SCII.AddressRequest addressPayload = JsonConvert.DeserializeObject<SCII.AddressRequest>(jsonPayload);
                if (addressPayload.address == null)
                {
                    errorMessage = errorMessage.Append("Address can not be empty");
                }
                else
                {
                    objCommon = new SCII.Helper(executionContext);
                    ValidationContext validationContext = new ValidationContext(addressPayload, serviceProvider: null, items: null);
                    ICollection<ValidationResult> validationResults = null;
                    ICollection<ValidationResult> validationResultsAddress = null;

                    bool isValid = objCommon.Validate(addressPayload, out validationResults);
                    bool isValidAddress = objCommon.Validate(addressPayload.address, out validationResultsAddress);

                    localcontext.Trace("TRACE TO valid:" + isValid);
                    string customerEntity = addressPayload.recordtype == SCII.RecordType.contact ? SCS.Contact.ENTITY : SCS.AccountContants.ENTITY_NAME;
                    string customerEntityId = addressPayload.recordtype == SCII.RecordType.contact ? SCS.Contact.CONTACTID : SCS.AccountContants.ACCOUNTID;

                    // check for building name, it should be mandatory only if the building number is empty
                    if (string.IsNullOrEmpty(addressPayload.address.buildingname))
                    {
                        if (string.IsNullOrEmpty(addressPayload.address.buildingnumber))
                        {
                            errorMessage.Append("Provide either building name or building number, Building name is mandatory if the building number is empty;");
                        }
                    }

                    // check for postcode lengths, it should be 8 for UK and 25 for NON-UK
                    if (isValidAddress && isValid)
                    {
                        if (addressPayload.address.country.Trim().ToUpper() == "GBR")
                        {
                            if (addressPayload.address.postcode.Length > 8)
                            {
                                errorMessage.Append("postcode length can not be greater than 8 for UK countries;");
                            }
                        }
                        else
                        {
                            if (addressPayload.address.postcode.Length > 25)
                            {
                                errorMessage.Append("postcode length can not be greater than 25 for NON-UK countries;");
                            }
                        }
                    }

                    if (isValid && isValidAddress&& errorMessage.Length == 0)
                    {
                        // check recordid exists
                        if (!string.IsNullOrEmpty(addressPayload.recordid) && !string.IsNullOrWhiteSpace(addressPayload.recordid))
                        {
                            if (Guid.TryParse(addressPayload.recordid, out customerId))
                            {
                                localcontext.Trace("record id:" + customerEntity + ":" + customerId);
                                OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);
                                var checkRecordExists = from c in orgSvcContext.CreateQuery(customerEntity)
                                                        where (Guid)c[customerEntityId] == customerId
                                                        select new { recordId = c.Id };
                                if (checkRecordExists != null && checkRecordExists.FirstOrDefault() != null)
                                {
                                    customerId = checkRecordExists.FirstOrDefault().recordId;
                                    isRecordIdExists = true;
                                }
                            }
                        }

                        // if record exists then go on to add address
                        if (isRecordIdExists)
                        {
                            localcontext.Trace("length:" + addressPayload.recordid);
                            EntityReference customer = new EntityReference(customerEntity, customerId);
                            if (addressPayload.address != null)
                            {
                                createdAddress = objCommon.CreateAddress(addressPayload.address, customer);
                            }

                            localcontext.Trace("after adding address:");
                            errorCode = 200;
                        }

                        // if the organisation does not exists
                        else
                        {
                            errorCode = 404;
                            errorMessage = errorMessage.Append(string.Format("recordid with id {0} does not exists.", addressPayload.recordid));
                        }
                    }
                    else
                    {
                        localcontext.Trace("inside validation result");

                         // _errorMessage = new StringBuilder();

                        // this will throw an error
                        foreach (ValidationResult vr in validationResults)
                        {
                            errorMessage.Append(vr.ErrorMessage + " ");
                        }
                        foreach (ValidationResult vr in validationResultsAddress)
                        {
                            errorMessage.Append(vr.ErrorMessage + " ");
                         }

                        errorCode = 400;
                    }
                }

            }
            catch (Exception ex)
            {
                localcontext.Trace("inside exception");
                errorCode = 500;
                errorMessage = errorMessage.Append(" Error occured while processing request");
                errorMessageDetail = ex.Message;
                if(ex.Message.Contains("Contact details of same type already exist for this customer"))
                {
                    errorCode = 412;
                }
                localcontext.Trace(ex.Message);
            }
            finally
            {
                localcontext.Trace("finally block start");
                AddressResponse responsePayload = new AddressResponse()
                {
                    code = errorCode,
                    message = errorMessage.ToString(),
                    datetime = DateTime.UtcNow,
                    version = "1.0.0.2",

                    status = errorCode == 200 ? "success" : "failure",
                    data = new AddressData()
                    {
                        contactdetailsid = createdAddress.contactdetailsid,
                        addressid = createdAddress.addressid,
                        error = new SCIIR.ResponseErrorBase() { details = errorMessageDetail == string.Empty ? errorMessage.ToString() : errorMessageDetail }
                    }
                };

                string resPayload = JsonConvert.SerializeObject(responsePayload);
                ResPayload.Set(executionContext, resPayload);
                localcontext.Trace("finally block end");
            }
        }
        #endregion
    }
}