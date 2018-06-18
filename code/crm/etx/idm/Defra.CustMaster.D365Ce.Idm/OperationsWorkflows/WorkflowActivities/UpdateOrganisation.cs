using System;
using System.Collections.Generic;
using Defra.CustMaster.D365.Common.Schema.ExtEnums;
using Defra.CustMaster.D365.Common.Ints.Idm.Resp;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Defra.CustMaster.D365.Common.Ints.Idm;
using CommonSchema = Defra.CustMaster.D365.Common.schema;
using Newtonsoft.Json;

namespace Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.WorkflowActivities
{
    public class UpdateOrganisation : WorkFlowActivityBase
    {
        #region "Parameter Definition"

        [RequiredArgument]
        [Input("request")]
        public InArgument<String> ReqPayload { get; set; }

        [Output("response")]
        public OutArgument<String> ResPayload { get; set; }

        #endregion
        #region global parameters
        Helper objCommon;
        //EntityReference _Contact;
        int _errorCode = 400; //Bad Request
        string _errorMessage = string.Empty;
        string _errorMessageDetail = string.Empty;
        Guid orgId = Guid.Empty;
        string _uniqueReference = string.Empty;


        #endregion

        #region Execute
        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {

            string orgPayload = ReqPayload.Get(executionContext);

            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Organisation));
            int? optionSetValue;
            Guid orgId = Guid.Empty;
            Entity existingAccountRecord = new Entity();
            StringBuilder ErrorMessage = new StringBuilder();
            string _uniqueReference = string.Empty;
            string _crn = string.Empty;
            bool isOrgExists = false;
            Entity AccountObject = new Entity(CommonSchema.AccountContants.ENTITY_NAME);

            try
            {

                objCommon = new Helper(executionContext);
                objCommon.tracingService.Trace("Load CRM Service from context --- OK");

                objCommon.tracingService.Trace("attempt to seriallised");

                string jsonPayload = ReqPayload.Get(executionContext);
                Organisation accountPayload = JsonConvert.DeserializeObject<Organisation>(jsonPayload);
                objCommon.tracingService.Trace("seriallised object working");

                var ValidationContext = new ValidationContext(accountPayload, serviceProvider: null, items: null);
                ICollection<ValidationResult> ValidationResults = null;

                var isValid = objCommon.Validate(accountPayload, out ValidationResults);


                if (isValid)
                {
                    //check organisation id exists
                    if (!string.IsNullOrEmpty(accountPayload.organisationid) && !string.IsNullOrWhiteSpace(accountPayload.organisationid))
                    {

                        if (Guid.TryParse(accountPayload.organisationid, out orgId))
                        {
                            existingAccountRecord = objCommon.service.Retrieve("account", orgId, new Microsoft.Xrm.Sdk.Query.ColumnSet(CommonSchema.AccountContants.UNIQUEREFERENCE, CommonSchema.AccountContants.COMPANY_HOUSE_ID, CommonSchema.AccountContants.PARENTACCOUNTID));
                            if (existingAccountRecord != null && existingAccountRecord.Id != null)
                            {
                                AccountObject.Id = existingAccountRecord.Id;
                                _uniqueReference = (string)existingAccountRecord[CommonSchema.AccountContants.UNIQUEREFERENCE];
                                _crn = (string)existingAccountRecord[CommonSchema.AccountContants.COMPANY_HOUSE_ID];
                                isOrgExists = true;
                            }

                        }
                    }
                    // if org exists then go on to update the organisation
                    if (isOrgExists)
                    {


                        objCommon.tracingService.Trace("length{0}", accountPayload.name.Length);
                        if (accountPayload.hierarchylevel != 0)
                        {
                            objCommon.tracingService.Trace("hierarchylevel level: {0}", accountPayload.hierarchylevel);

                            if (!String.IsNullOrEmpty(Enum.GetName(typeof(defra_OrganisationHierarchyLevel), accountPayload.hierarchylevel)))
                            {

                                objCommon.tracingService.Trace("before assinging value");

                                AccountObject[CommonSchema.AccountContants.HIERARCHYLEVEL] = new OptionSetValue(accountPayload.hierarchylevel);
                                objCommon.tracingService.Trace("after assinging value");
                            }
                            else
                            {

                                ErrorMessage = ErrorMessage.Append(String.Format("Option set value {0} for orgnisation hirarchy level not found.",
                                accountPayload.hierarchylevel));
                            }
                        }

                        if (!String.IsNullOrEmpty(Enum.GetName(typeof(defra_OrganisationType), accountPayload.type)))
                        {
                            objCommon.tracingService.Trace("After completing validation 12" + accountPayload.type);
                            optionSetValue = accountPayload.type;
                            objCommon.tracingService.Trace("before assigning type  " + accountPayload.type);
                            objCommon.tracingService.Trace(optionSetValue.ToString());
                            objCommon.tracingService.Trace("after  setting up option set value");
                            OptionSetValueCollection BusinessTypes = new OptionSetValueCollection();
                            BusinessTypes.Add(new OptionSetValue(optionSetValue.Value));
                            AccountObject[CommonSchema.AccountContants.TYPE] = BusinessTypes;
                        }
                        else
                        {
                            ErrorMessage = ErrorMessage.Append(String.Format("Option set value {0} for orgnisation type does not exists.",
                            accountPayload.type));
                        }

                        //check if crn exists
                        if (accountPayload.crn != null && _crn != accountPayload.crn)
                        {
                            objCommon.tracingService.Trace("account crn:" + _crn + "request crn:" + accountPayload.crn);
                            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);
                            var checkCRNExistis = from c in orgSvcContext.CreateQuery("account")
                                                  where (string)c[CommonSchema.AccountContants.COMPANY_HOUSE_ID] == accountPayload.crn
                                                  select new { organisationid = c.Id };


                            if (checkCRNExistis.FirstOrDefault() == null)
                            {
                                AccountObject[CommonSchema.AccountContants.COMPANY_HOUSE_ID] = accountPayload.crn;
                            }
                            else
                            {
                                _errorCode = 412;
                                ErrorMessage = ErrorMessage.Append(String.Format("Company house id already exists."));
                            }
                        }
                        if (accountPayload.name != null)
                            AccountObject[CommonSchema.AccountContants.NAME] = accountPayload.name;
                        if (accountPayload.telephone != null)
                            AccountObject[CommonSchema.AccountContants.TELEPHONE1] = accountPayload.telephone;
                        objCommon.tracingService.Trace("after  setting other fields");

                        bool isValidGuid;
                        Guid parentAccountId;
                        if (accountPayload.parentorganisation != null && !String.IsNullOrEmpty(accountPayload.parentorganisation.parentorganisationcrmid))
                        {
                            if (((EntityReference)existingAccountRecord[CommonSchema.AccountContants.PARENTACCOUNTID]).Id.ToString()!= accountPayload.parentorganisation.parentorganisationcrmid)
                            {
                                isValidGuid = Guid.TryParse(accountPayload.parentorganisation.parentorganisationcrmid, out parentAccountId);
                                if (isValidGuid)
                                {
                                    AccountObject[CommonSchema.AccountContants.PARENTACCOUNTID] = new EntityReference("account", parentAccountId);
                                }
                                else
                                {
                                    ErrorMessage = ErrorMessage.Append(String.Format("parentorganisationcrmid: {0} is not valid guid",
                             accountPayload.parentorganisation.parentorganisationcrmid));
                                }
                            }
                        }
                        if (accountPayload.email != null)
                            AccountObject[CommonSchema.AccountContants.EMAILADDRESS1] = accountPayload.email;
                        objCommon.tracingService.Trace("before updating guid:" + AccountObject.Id.ToString());
                        objCommon.service.Update(AccountObject);
                        objCommon.tracingService.Trace("after updating guid:{0}", AccountObject.Id.ToString());

                        _errorCode = 200;

                    }
                    //if the organisation does not exists
                    else
                    {
                        _errorCode = 417;
                        ErrorMessage = ErrorMessage.Append(String.Format("Oranisation with id {0} does not exists.",
                        accountPayload.organisationid));
                    }
                }
                else
                {
                    objCommon.tracingService.Trace("inside validation result");
                    ErrorMessage = new StringBuilder();
                    //this will throw an error
                    foreach (ValidationResult vr in ValidationResults)
                    {
                        ErrorMessage.Append(vr.ErrorMessage + " ");
                    }

                    _errorCode = 400;

                }

            }

            catch (Exception ex)
            {
                objCommon.tracingService.Trace("inside exception");

                _errorCode = 500;
                _errorMessage = "Error occured while processing request";
                _errorMessageDetail = ex.Message;

                objCommon.tracingService.Trace(ex.Message);

            }
            finally
            {


                objCommon.tracingService.Trace("finally block start");
                AccountResponse responsePayload = new AccountResponse()
                {
                    code = _errorCode,
                    message = ErrorMessage.ToString(),
                    datetime = DateTime.UtcNow,
                    version = "1.0.0.2",

                    status = _errorCode == 200 ? "success" : "failure",
                    data = new AccountData()
                    {
                        accountid = AccountObject.Id,
                        uniquerefere = _uniqueReference,
                        error = new ResponseErrorBase() { details = _errorMessageDetail }
                    }

                };

                string resPayload = JsonConvert.SerializeObject(responsePayload);
                ResPayload.Set(executionContext, resPayload);
                objCommon.tracingService.Trace("finally block end");


            }

        }
        #endregion
    }
}