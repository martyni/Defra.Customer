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
using Defra.CustMaster.D365.Common.Ints.Idm.Resp;
using IdmNs = Defra.CustMaster.D365.Common.Ints.Idm;

namespace Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.WorkflowActivities
{
    public class CreateOrganisation : WorkFlowActivityBase
    {

        #region "Parameter Definition"
        [RequiredArgument]
        [Input("PayLoad")]
        public InArgument<String> PayLoad { get; set; }
        [Output("OutPutJson")]
        public OutArgument<string> ReturnMessageDetails { get; set; }

        #endregion

        IdmNs.Helper objCommon;


        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {

            String PayloadDetails = PayLoad.Get(executionContext);
            int? optionSetValue;
            int ErrorCode = 400; //400 -- bad request
            String _ErrorMessage = string.Empty;
            String _ErrorMessageDetail = string.Empty;
            Guid ContactId = Guid.Empty;
            Guid CrmGuid = Guid.Empty;
            StringBuilder ErrorMessage = new StringBuilder();
            String UniqueReference = string.Empty;
            try
            {

                objCommon = new IdmNs.Helper(executionContext);
                objCommon.tracingService.Trace("Load CRM Service from context --- OK");
                Entity AccountObject = new Entity(Defra.CustMaster.D365.Common.schema.AccountContants.ENTITY_NAME);

                objCommon.tracingService.Trace("attempt to seriallised new");
                IdmNs.Organisation AccountPayload = JsonConvert.DeserializeObject<IdmNs.Organisation>(PayloadDetails);
                objCommon.tracingService.Trace("seriallised object working");
                var ValidationContext = new ValidationContext(AccountPayload, serviceProvider: null, items: null);
                ICollection<System.ComponentModel.DataAnnotations.ValidationResult> ValidationResults = null;
                ICollection<System.ComponentModel.DataAnnotations.ValidationResult> ValidationResultsAddress = null;

                var isValid = objCommon.Validate(AccountPayload, out ValidationResults);
                Boolean isValidAddress = AccountPayload.address == null ? true :
                    objCommon.Validate(AccountPayload.address, out ValidationResultsAddress);

                if (isValid & isValidAddress)
                {

                    objCommon.tracingService.Trace("length{0}", AccountPayload.name.Length);
                    if (AccountPayload.hierarchylevel != 0)
                    {
                        objCommon.tracingService.Trace("hierarchylevel level: {0}", AccountPayload.hierarchylevel);

                        if (!String.IsNullOrEmpty(Enum.GetName(typeof(defra_OrganisationHierarchyLevel), AccountPayload.hierarchylevel)))
                        {

                            objCommon.tracingService.Trace("before assinging value");

                            AccountObject[Defra.CustMaster.D365.Common.schema.AccountContants.HIERARCHYLEVEL] = new OptionSetValue(AccountPayload.hierarchylevel);
                            objCommon.tracingService.Trace("after assinging value");


                            if (!String.IsNullOrEmpty(Enum.GetName(typeof(defra_OrganisationType), AccountPayload.type)))
                            {
                                //check if crn exists

                                OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);
                                var checkCRNExistis = from c in orgSvcContext.CreateQuery("account")
                                                      where (string)c[Defra.CustMaster.D365.Common.schema.AccountContants.COMPANY_HOUSE_ID] == AccountPayload.crn
                                                      select new { organisationid = c.Id };

                                if (checkCRNExistis.FirstOrDefault() == null)
                                {
                                    objCommon.tracingService.Trace("After completing validation 12" + AccountPayload.type);
                                    optionSetValue = AccountPayload.type;
                                    objCommon.tracingService.Trace("before assigning type  " + AccountPayload.type);
                                    objCommon.tracingService.Trace(optionSetValue.ToString());
                                    objCommon.tracingService.Trace("after  setting up option set value");
                                    OptionSetValueCollection BusinessTypes = new OptionSetValueCollection();
                                    BusinessTypes.Add(new OptionSetValue(optionSetValue.Value));
                                    AccountObject[Defra.CustMaster.D365.Common.schema.AccountContants.TYPE] = BusinessTypes;
                                    AccountObject[Defra.CustMaster.D365.Common.schema.AccountContants.NAME] = AccountPayload.name == null ? string.Empty : AccountPayload.name;
                                    AccountObject[Defra.CustMaster.D365.Common.schema.AccountContants.COMPANY_HOUSE_ID] = AccountPayload.crn == string.Empty ? null : AccountPayload.crn;
                                    AccountObject[Defra.CustMaster.D365.Common.schema.AccountContants.TELEPHONE1] = AccountPayload.telephone == null ? string.Empty : AccountPayload.telephone;
                                    objCommon.tracingService.Trace("after  setting other fields");

                                    bool IsValidGuid;
                                    Guid ParentAccountId;
                                    if (AccountPayload.parentorganisation != null && String.IsNullOrEmpty(AccountPayload.parentorganisation.parentorganisationcrmid))
                                    {
                                        IsValidGuid = Guid.TryParse(AccountPayload.parentorganisation.parentorganisationcrmid, out ParentAccountId);
                                        if (IsValidGuid)
                                        {
                                            AccountObject[Defra.CustMaster.D365.Common.schema.AccountContants.PARENTACCOUNTID] = ParentAccountId;
                                        }
                                    }

                                    AccountObject[Defra.CustMaster.D365.Common.schema.AccountContants.EMAILADDRESS1] = AccountPayload.email;
                                    objCommon.tracingService.Trace("before createing guid:");
                                    CrmGuid = objCommon.service.Create(AccountObject);
                                    objCommon.tracingService.Trace("after createing guid:{0}", CrmGuid.ToString());
                                    Entity AccountRecord = objCommon.service.Retrieve("account", CrmGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet(Defra.CustMaster.D365.Common.schema.AccountContants.UNIQUEREFERENCE));
                                    UniqueReference = (string)AccountRecord[Defra.CustMaster.D365.Common.schema.AccountContants.UNIQUEREFERENCE];
                                    objCommon.CreateAddress(AccountPayload.address, new EntityReference(Defra.CustMaster.D365.Common.schema.AccountContants.ENTITY_NAME, CrmGuid));
                                    ErrorCode = 200;

                                }
                                else
                                {
                                    ErrorCode = 412;
                                    ErrorMessage = ErrorMessage.Append(String.Format("Company house id already exists."));
                                }

                            }
                        }
                        else
                        {
                            ErrorCode = 400;
                            ErrorMessage = ErrorMessage.Append(String.Format("Option set value {0} for orgnisation hirarchy level not found.",
                            Defra.CustMaster.D365.Common.schema.AccountContants.HIERARCHYLEVEL.ToString()));
                        }
                    }

                    else
                    {
                        ErrorCode = 400;
                        ErrorMessage = ErrorMessage.Append(String.Format("Option set value {0} for orgnisation type does not exists.",
                        AccountPayload.type));
                    }


                }
                else
                {
                    objCommon.tracingService.Trace("inside validation result");
                    ErrorMessage = new StringBuilder();
                    //this will throw an error
                    foreach (System.ComponentModel.DataAnnotations.ValidationResult vr in ValidationResults)
                    {
                        ErrorMessage.Append(vr.ErrorMessage + " ");
                    }
                    foreach (System.ComponentModel.DataAnnotations.ValidationResult vr in ValidationResultsAddress)
                    {
                        ErrorMessage.Append(vr.ErrorMessage + " ");
                    }
                    ErrorCode = 400;

                }

            }
            catch (Exception ex)
            {
                objCommon.tracingService.Trace("inside exception");

                ErrorCode = 500;
                _ErrorMessage = "Error occured while processing request";
                _ErrorMessageDetail = ex.Message;
                ErrorCode = 400;
                this.ReturnMessageDetails.Set(executionContext, _ErrorMessageDetail);
                objCommon.tracingService.Trace(ex.Message);

            }
            finally
            {


                objCommon.tracingService.Trace("finally block start");
                AccountResponse responsePayload = new AccountResponse()
                {
                    code = ErrorCode,
                    message = ErrorMessage.ToString(),
                    datetime = DateTime.UtcNow,
                    version = "1.0.0.2",

                    status = ErrorCode == 200 ? "success" : "failure",
                    data = new AccountData()
                    {
                        accountid = CrmGuid,
                        uniquerefere = UniqueReference,
                        error = new ResponseErrorBase() { details = _ErrorMessageDetail }
                    }

                };
                objCommon.tracingService.Trace("attempting to serialise");

                string json = JsonConvert.SerializeObject(responsePayload);

                ReturnMessageDetails.Set(executionContext, json);
                // OutputCode.Set(executionContext, _errorCode);

                objCommon.tracingService.Trace("json {0}", json);
                objCommon.tracingService.Trace("finally block end");


            }

        }
    }
}

