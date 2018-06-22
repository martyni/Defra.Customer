using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SCS = Defra.CustMaster.D365.Common.schema;
using SCII = Defra.CustMaster.D365.Common.Ints.Idm;
using SCIIR = Defra.CustMaster.D365.Common.Ints.Idm.Resp;
using Defra.CustMaster.D365.Common.Ints.Idm.resp;

namespace Defra.CustMaster.Identity.WfActivities.Contact
{
    class AddAddress : WorkFlowActivityBase
    {
        #region "Parameter Definition"

        [RequiredArgument]
        [Input("request")]
        public InArgument<String> ReqPayload { get; set; }

        [Output("response")]
        public OutArgument<String> ResPayload { get; set; }

        #endregion
       

        #region Execute
        protected override void Execute(CodeActivityContext executionContext)
        {
            #region local variables
            SCII.Helper _objCommon;
            //EntityReference _Contact;
            int _errorCode = 400; //Bad Request
            string _errorMessageDetail = string.Empty;          
            Guid _customerId = Guid.Empty;
            Entity _existingAccountRecord = new Entity();
            StringBuilder _errorMessage = new StringBuilder();          
            bool _isRecordIdExists = false;
            AddressData createdAddress = new AddressData() { addressid = Guid.Empty, contactdetailsid = Guid.Empty };
            #endregion
            LocalWorkflowContext localcontext = new LocalWorkflowContext(executionContext);

            try
            {
                localcontext.Trace("started execution");
                localcontext.Trace("attempt to seriallised");

                string jsonPayload = ReqPayload.Get(executionContext);
                SCII.AddressRequest addressPayload = JsonConvert.DeserializeObject<SCII.AddressRequest>(jsonPayload);


                if (addressPayload.address == null)
                {

                }
                else
                {
                    _objCommon = new SCII.Helper(executionContext);
                    ValidationContext ValidationContext = new ValidationContext(addressPayload, serviceProvider: null, items: null);
                    ICollection<ValidationResult> ValidationResults = null;
                    ICollection<ValidationResult> ValidationResultsAddress = null;

                    bool isValid = _objCommon.Validate(addressPayload, out ValidationResults);
                    bool isValidAddress = _objCommon.Validate(addressPayload.address, out ValidationResultsAddress);

                    localcontext.Trace("TRACE TO valid:" + isValid);
                    string customerEntity = addressPayload.recordtype == SCII.RecordType.Organisation ? SCS.AccountContants.ENTITY_NAME : SCS.Contact.ENTITY;
                    if (isValid&& isValidAddress)
                    {
                        //check recordid exists
                        if (!string.IsNullOrEmpty(addressPayload.recordid) && !string.IsNullOrWhiteSpace(addressPayload.recordid))
                        {

                            if (Guid.TryParse(addressPayload.recordid, out _customerId))
                            {

                                _existingAccountRecord = _objCommon.service.Retrieve(customerEntity, _customerId, new Microsoft.Xrm.Sdk.Query.ColumnSet(SCS.Contact.NAME));
                                if (_existingAccountRecord != null && _existingAccountRecord.Id != null)
                                {
                                    _isRecordIdExists = true;
                                }

                            }
                        }

                        // if record exists then go on to add address
                        if (_isRecordIdExists)
                        {
                            localcontext.Trace("length:" + addressPayload.recordid);
                            EntityReference customer = new EntityReference(customerEntity, _customerId);
                            _objCommon = new SCII.Helper(executionContext);
                            if (addressPayload.address != null)
                                createdAddress = _objCommon.CreateAddress(addressPayload.address, customer);
                            localcontext.Trace("after adding address:");
                            _errorCode = 200;

                        }
                        //if the organisation does not exists
                        else
                        {
                            _errorCode = 404;
                            _errorMessage = _errorMessage.Append(String.Format("recordid with id {0} does not exists.",
                            addressPayload.recordid));
                        }
                    }
                    else
                    {
                        localcontext.Trace("inside validation result");
                        _errorMessage = new StringBuilder();
                        //this will throw an error
                        foreach (ValidationResult vr in ValidationResults)
                        {
                            _errorMessage.Append(vr.ErrorMessage + " ");
                        }
                        foreach (ValidationResult vr in ValidationResultsAddress)
                        {
                            _errorMessage.Append(vr.ErrorMessage + " ");
                        }
                        _errorCode = 400;

                    }
                }

            }

            catch (Exception ex)
            {
                localcontext.Trace("inside exception");
                _errorCode = 500;
                _errorMessage = _errorMessage.Append(" Error occured while processing request");
                _errorMessageDetail = ex.Message;
                localcontext.Trace(ex.Message);

            }
            finally
            {


                localcontext.Trace("finally block start");
               AddressResponse responsePayload = new AddressResponse()
                {
                    code = _errorCode,
                    message = _errorMessage.ToString(),
                    datetime = DateTime.UtcNow,
                    version = "1.0.0.2",

                    status = _errorCode == 200 ? "success" : "failure",
                    data = new AddressData()
                    {
                        contactdetailsid=createdAddress.contactdetailsid,
                        addressid=createdAddress.addressid,
                        error = new SCIIR.ResponseErrorBase() { details = _errorMessageDetail == string.Empty ? _errorMessage.ToString() : _errorMessageDetail }
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