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

namespace Defra.CustMaster.D365Ce.Idm.OperationsWorkflows
{


    public class CreateOrganisation : WorkFlowActivityBase
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
            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Account));
            
            int? optionSetValue;
            int? HiratchyType;
            Int64 ErrorCode = 400; //400 -- bad request
            String _ErrorMessage = string.Empty;
            String _ErrorMessageDetail = string.Empty;
            Guid ContactId = Guid.Empty;
            Guid CrmGuid;
            #endregion

            #region "Load CRM Service from context"
            try
            {
                objCommon = new Common(executionContext);
                objCommon.tracingService.Trace("Load CRM Service from context --- OK");
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(Payload)))
                {
                    Account AccountPayload = (Account)deserializer.ReadObject(ms);
                    objCommon.tracingService.Trace("seriallised");


                    if (AccountPayload.type == 0)
                    {
                        objCommon.tracingService.Trace("checking business type");

                        _ErrorMessage = "Business type cannot be empty.";

                    }
                    else if (!String.IsNullOrEmpty(AccountPayload.name) && AccountPayload.name.Length > 160)
                    {
                        objCommon.tracingService.Trace("checking org name");

                        _ErrorMessage = "Organiation name is more than 160 characters.";

                    }
                    else if (!String.IsNullOrEmpty(AccountPayload.crn) && AccountPayload.crn.Length > 8)
                    {
                        objCommon.tracingService.Trace("checking house id");

                        _ErrorMessage = "Company House Id cannot be more than 8 characters.";
                    }

                    else if (!(String.IsNullOrWhiteSpace(AccountPayload.validatedwithcompanieshouse)) && (AccountPayload.validatedwithcompanieshouse == "y" || 
                        AccountPayload.validatedwithcompanieshouse == "n"))
                    {
                        objCommon.tracingService.Trace("checking validated with company house id");

                        _ErrorMessage = "Validated with company house should have y or n";

                    }
                   
                    else if (!String.IsNullOrEmpty(AccountPayload.email) && AccountPayload.email.Length > 100)
                    {
                        _ErrorMessage = "Email address cannot be more than 100 characters long.";

                    }

                    else
                    {

                        objCommon.tracingService.Trace("After completing validation 12" + AccountPayload.type);

                        optionSetValue = AccountPayload.type;
                        Entity Account = new Entity("account");
                        objCommon.tracingService.Trace("before assigning type  " + AccountPayload.type);
                        objCommon.tracingService.Trace(optionSetValue.ToString());
                        objCommon.tracingService.Trace("after  setting up option set value");


                        OptionSetValueCollection BusinessTypes = new OptionSetValueCollection();
                        BusinessTypes.Add(new OptionSetValue(optionSetValue.Value)); //
                        Account["defra_type"] = BusinessTypes;
                        Account["name"] = AccountPayload.name == null ? string.Empty : AccountPayload.name;
                        Account["defra_companyhouseid"] = AccountPayload.crn == string.Empty ? null : AccountPayload.crn;
                        Account["telephone1"] = AccountPayload.telephone == null ? string.Empty : AccountPayload.telephone;

                        if (!String.IsNullOrEmpty(AccountPayload.hierarchylevel))
                        {
                            objCommon.tracingService.Trace("hierarchylevel level: {0}", AccountPayload.hierarchylevel);
                            Account["defra_hierarchylevel"] = new OptionSetValue(int.Parse(AccountPayload.hierarchylevel));
                        }
                        objCommon.tracingService.Trace("after  setting other fields");

                        bool IsValidGuid;
                        Guid ParentAccountId;
                        if (AccountPayload.parentorganisation != null && String.IsNullOrEmpty(AccountPayload.parentorganisation.parentorganisationcrmid ))
                        {

                            IsValidGuid = Guid.TryParse(AccountPayload.parentorganisation.parentorganisationcrmid, out ParentAccountId);

                            if(IsValidGuid)
                            {
                                Account["parentaccountid"] = ParentAccountId;
                            }
                        }
                        objCommon.tracingService.Trace("after assigning");

                        if (AccountPayload.validatedwithcompanieshouse == "y")
                        {
                            Account["defra_validatedwithcompanyhouse"] = new OptionSetValue(0);
                        }
                        else if (AccountPayload.validatedwithcompanieshouse == "n")
                        {
                            Account["defra_validatedwithcompanyhouse"] = new OptionSetValue(1);
                        }

                        if (AccountPayload.email != null)
                        { Account["emailaddress1"] = AccountPayload.email; }
                        objCommon.tracingService.Trace("before createing guid:");
                        
                        CrmGuid = objCommon.service.Create(Account);
                        objCommon.tracingService.Trace("after createing guid:{0}", CrmGuid.ToString());
                        this.CrmGuid.Set(executionContext, CrmGuid.ToString());
                        Entity AccountRecord = objCommon.service.Retrieve("account", CrmGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet("defra_uniquereference"));
                        this.Code.Set(executionContext, ErrorCode.ToString());
                        this.UniqueReference.Set(executionContext, AccountRecord["defra_uniquereference"]);
                        objCommon.CreateAddress(AccountPayload.address, new EntityReference("account", CrmGuid));
                        objCommon.tracingService.Trace("after creating account");

                       
                        ErrorCode = 200; //success
                    }

                    objCommon.tracingService.Trace("outside 1");

                    this.Code.Set(executionContext, ErrorCode.ToString());
                    this.Message.Set(executionContext, _ErrorMessage);
                    this.MessageDetail.Set(executionContext, _ErrorMessageDetail);
                    objCommon.tracingService.Trace("after setting error message");


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
