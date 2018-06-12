using Defra.CustMaster.D365.Common.Schema.ExtEnums;
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

namespace Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.WorkflowActivities
{
    public class CreateOrganisationNew: WorkFlowActivityBase
    {
         
        #region "Parameter Definition"
        [RequiredArgument]
        [Input("PayLoad")]
        public InArgument<String> PayLoad { get; set; }
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


        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            #region "Read Parameters"

            //Account AccountPayload = JsonConvert.DeserializeObject<Account>(PayLoad.Get(executionContext));

            String Payload = PayLoad.Get(executionContext);
            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(D365.Common.schema.AccountSchma));

            int? optionSetValue;

            Int64 ErrorCode = 400; //400 -- bad request
            String _ErrorMessage = string.Empty;
            String _ErrorMessageDetail = string.Empty;
            Guid ContactId = Guid.Empty;
            Guid CrmGuid = Guid.Empty;
            StringBuilder ErrorMessage;
            #endregion

            #region "Load CRM Service from context"

            try
            {
                objCommon = new Common(executionContext);
                objCommon.tracingService.Trace("Load CRM Service from context --- OK");
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
                        //Entity Account = new Entity(AccountSchema.ENTITY_NAME);
                        objCommon.tracingService.Trace("before assigning type  " + AccountPayload.type);
                        objCommon.tracingService.Trace(optionSetValue.ToString());
                        objCommon.tracingService.Trace("after  setting up option set value");


                        OptionSetValueCollection BusinessTypes = new OptionSetValueCollection();
                        BusinessTypes.Add(new OptionSetValue(optionSetValue.Value)); //
                                                                                     // Account[Defra.CustMaster.D365.Common.schema.Account.TYPE] = BusinessTypes;
                                                                                     // Account[Defra.CustMaster.D365.Common.schema.Account.NAME] = AccountPayload.name == null ? string.Empty : AccountPayload.name;
                                                                                     // Account[Defra.CustMaster.D365.Common.schema.Account.COMPANY_HOUSE_ID] = AccountPayload.crn == string.Empty ? null : AccountPayload.crn;
                                                                                     // Account[Defra.CustMaster.D365.Common.schema.Account.TELEPHONE1] = AccountPayload.telephone == null ? string.Empty : AccountPayload.telephone;

                        if (!String.IsNullOrEmpty(AccountPayload.hierarchylevel))
                        {
                            objCommon.tracingService.Trace("hierarchylevel level: {0}", AccountPayload.hierarchylevel);
                            // Account[Defra.CustMaster.D365.Common.schema.Account.HIERARCHYLEVEL] = new OptionSetValue(int.Parse(AccountPayload.hierarchylevel));
                        }
                        objCommon.tracingService.Trace("after  setting other fields");

                        bool IsValidGuid;
                        Guid ParentAccountId;
                        if (AccountPayload.parentorganisation != null && String.IsNullOrEmpty(AccountPayload.parentorganisation.parentorganisationcrmid))
                        {
                            IsValidGuid = Guid.TryParse(AccountPayload.parentorganisation.parentorganisationcrmid, out ParentAccountId);
                            if (IsValidGuid)
                            {
                                // Account[Defra.CustMaster.D365.Common.schema.Account.PARENTACCOUNTID] = ParentAccountId;
                            }
                        }
                        objCommon.tracingService.Trace("after assigning");

                        if (AccountPayload.validatedwithcompanieshouse == "y")
                        {
                            //Account[Defra.CustMaster.D365.Common.schema.Account.VALIDATED_WITH_COMPANYHOUSE] = new OptionSetValue(0);
                        }
                        else if (AccountPayload.validatedwithcompanieshouse == "n")
                        {
                            //Account[Defra.CustMaster.D365.Common.schema.Account.VALIDATED_WITH_COMPANYHOUSE] = new OptionSetValue(1);
                        }

                        if (AccountPayload.email != null)
                        {

                            //Account[Defra.CustMaster.D365.Common.schema.Account.EMAILADDRESS1] = AccountPayload.email; }
                            objCommon.tracingService.Trace("before createing guid:");
                            //CrmGuid = objCommon.service.Create(Account);
                            objCommon.tracingService.Trace("after createing guid:{0}", CrmGuid.ToString());
                            this.CrmGuid.Set(executionContext, CrmGuid.ToString());
                            Entity AccountRecord = objCommon.service.Retrieve("account", CrmGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet(Defra.CustMaster.D365.Common.schema.AccountSchma.UNIQUEREFERENCE));
                            this.Code.Set(executionContext, ErrorCode.ToString());
                            this.UniqueReference.Set(executionContext, AccountRecord[Defra.CustMaster.D365.Common.schema.AccountSchma.UNIQUEREFERENCE]);
                            objCommon.CreateAddress(AccountPayload.address, new EntityReference(Defra.CustMaster.D365.Common.schema.AccountSchma.ENTITY_NAME, CrmGuid));
                            objCommon.tracingService.Trace("after creating account");
                            ErrorCode = 200; //success
                        }

                        else
                        {
                            ErrorMessage = new StringBuilder();
                            //this will throw an error
                            foreach (System.ComponentModel.DataAnnotations.ValidationResult vr in ValidationResult)
                            {
                                ErrorMessage.Append(vr.ErrorMessage + "\n");
                            }
                        }
                        this.Code.Set(executionContext, ErrorCode.ToString());
                        this.Message.Set(executionContext, _ErrorMessage);
                        this.MessageDetail.Set(executionContext, _ErrorMessageDetail);
                    }
                }
            }

            catch (Exception ex)
            {

                ErrorCode = 500;
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
    }
}

