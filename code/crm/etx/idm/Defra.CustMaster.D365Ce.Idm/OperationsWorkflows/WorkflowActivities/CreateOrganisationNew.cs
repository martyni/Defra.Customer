﻿using Defra.CustMaster.D365.Common.Schema.ExtEnums;
using Defra.CustMaster.D365.Common.schema;
using Defra.CustMaster.D365Ce.Idm.OperationsWorkflows;
using Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Workflow;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Text;
using Defra.CustMaster.D365.Common;
using Microsoft.Crm.Sdk.Messages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Defra.CustMaster.D365.Common.Ints.Idm.Resp;

namespace Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.WorkflowActivities
{
    public class CreateOrganisationNew : WorkFlowActivityBase
    {

        #region "Parameter Definition"
        [RequiredArgument]
        [Input("PayLoad")]
        public InArgument<String> PayLoad { get; set; }

        [Output("OutPutJson")]
        public OutArgument<string> ReturnMessageDetails { get; set; }

        #endregion

        Common objCommon;


        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {

            String Payload = PayLoad.Get(executionContext);
            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(D365.Common.schema.AccountSchma));
            int? optionSetValue;
            int ErrorCode = 400; //400 -- bad request
            String _ErrorMessage = string.Empty;
            String _ErrorMessageDetail = string.Empty;
            Guid ContactId = Guid.Empty;
            Guid CrmGuid = Guid.Empty;
            StringBuilder ErrorMessage;
            AccountResponse AccountDataResponse = new AccountResponse();
            try
            {
                objCommon = new Common(executionContext);
                objCommon.tracingService.Trace("Load CRM Service from context --- OK");
                Entity AccountObject = new Entity(Defra.CustMaster.D365.Common.schema.AccountSchma.ENTITY_NAME);

                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(Payload)))
                {
                    Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.Model.Account AccountPayload = (Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.Model.Account)deserializer.ReadObject(ms);
                    objCommon.tracingService.Trace("seriallised");
                    var ValidationContext = new ValidationContext(AccountPayload, serviceProvider: null, items: null);
                    var ValidationResult = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

                    var isValid = Validator.TryValidateObject(AccountPayload, ValidationContext, ValidationResult);


                    if (isValid)
                    {
                        objCommon.tracingService.Trace("After completing validation 12" + AccountPayload.type);
                        optionSetValue = AccountPayload.type;
                        objCommon.tracingService.Trace("before assigning type  " + AccountPayload.type);
                        objCommon.tracingService.Trace(optionSetValue.ToString());
                        objCommon.tracingService.Trace("after  setting up option set value");
                        OptionSetValueCollection BusinessTypes = new OptionSetValueCollection();
                        BusinessTypes.Add(new OptionSetValue(optionSetValue.Value));
                        AccountObject[Defra.CustMaster.D365.Common.schema.AccountSchma.TYPE] = BusinessTypes;
                        AccountObject[Defra.CustMaster.D365.Common.schema.AccountSchma.NAME] = AccountPayload.name == null ? string.Empty : AccountPayload.name;
                        AccountObject[Defra.CustMaster.D365.Common.schema.AccountSchma.COMPANY_HOUSE_ID] = AccountPayload.crn == string.Empty ? null : AccountPayload.crn;
                        AccountObject[Defra.CustMaster.D365.Common.schema.AccountSchma.TELEPHONE1] = AccountPayload.telephone == null ? string.Empty : AccountPayload.telephone;

                        if (!String.IsNullOrEmpty(AccountPayload.hierarchylevel))
                        {
                            objCommon.tracingService.Trace("hierarchylevel level: {0}", AccountPayload.hierarchylevel);
                            AccountObject[Defra.CustMaster.D365.Common.schema.AccountSchma.HIERARCHYLEVEL] = new OptionSetValue(int.Parse(AccountPayload.hierarchylevel));
                        }
                        objCommon.tracingService.Trace("after  setting other fields");

                        bool IsValidGuid;
                        Guid ParentAccountId;
                        if (AccountPayload.parentorganisation != null && String.IsNullOrEmpty(AccountPayload.parentorganisation.parentorganisationcrmid))
                        {
                            IsValidGuid = Guid.TryParse(AccountPayload.parentorganisation.parentorganisationcrmid, out ParentAccountId);
                            if (IsValidGuid)
                            {
                                AccountObject[Defra.CustMaster.D365.Common.schema.AccountSchma.PARENTACCOUNTID] = ParentAccountId;
                            }
                        }

                        AccountObject[Defra.CustMaster.D365.Common.schema.AccountSchma.EMAILADDRESS1] = AccountPayload.email;
                        objCommon.tracingService.Trace("before createing guid:");
                        CrmGuid = objCommon.service.Create(AccountObject);
                        objCommon.tracingService.Trace("after createing guid:{0}", CrmGuid.ToString());
                        AccountDataResponse.AccountData.accountid = CrmGuid;
                        Entity AccountRecord = objCommon.service.Retrieve("account", CrmGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet(Defra.CustMaster.D365.Common.schema.AccountSchma.UNIQUEREFERENCE));
                        AccountDataResponse.AccountData.uniquerefere = (string)AccountRecord[Defra.CustMaster.D365.Common.schema.AccountSchma.UNIQUEREFERENCE];
                        objCommon.CreateAddress(AccountPayload.address, new EntityReference(Defra.CustMaster.D365.Common.schema.AccountSchma.ENTITY_NAME, CrmGuid));
                        AccountDataResponse.code = 200;

                    }

                    else
                    {
                        ErrorMessage = new StringBuilder();
                        //this will throw an error
                        foreach (System.ComponentModel.DataAnnotations.ValidationResult vr in ValidationResult)
                        {
                            ErrorMessage.Append(vr.ErrorMessage + "\n");
                        }
                        ErrorCode = 400;

                    }
                }
            }
            catch (Exception ex)
            {

                ErrorCode = 500;
                _ErrorMessage = "Error occured while processing request";
                _ErrorMessageDetail = ex.Message;
                AccountDataResponse.code = 400;
                this.ReturnMessageDetails.Set(executionContext, _ErrorMessageDetail);
                objCommon.tracingService.Trace(ex.Message);

            }
            finally
            {
                AccountDataResponse.datetime = DateTime.UtcNow;
                AccountDataResponse.version = "1.0.0.2";
                AccountDataResponse.data.error.details = _ErrorMessage;
                AccountDataResponse.message = "message";
                AccountDataResponse.status = "status";
                AccountDataResponse.code = ErrorCode;
                MemoryStream ms = new MemoryStream();
                // Serializer the Response object to the stream.  
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ContactResponse));
                ser.WriteObject(ms, AccountDataResponse);
                byte[] json = ms.ToArray();
                ms.Close();
                ReturnMessageDetails.Set(executionContext, Encoding.Unicode.GetString(json, 0, json.Length));
            }

        }
    }
}

