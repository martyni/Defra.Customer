using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SCS = Defra.CustMaster.D365.Common.schema;
using SCII = Defra.CustMaster.D365.Common.Ints.Idm;
using SCSE = Defra.CustMaster.D365.Common.Schema.ExtEnums;
using SCIIR = Defra.CustMaster.D365.Common.Ints.Idm.Resp;


namespace Defra.CustMaster.Identity.WfActivities
{
    /// <summary>
    /// {'organisationid': 'D1B35E7C-D072-E811-A83B-000D3AB4F7AF','updates':{'name':'Update my create', 'type':'910400000', 'crn':'18062018','email':'Updateacme@acme.com', 'telephone':'004412345678', 'hierarchylevel':'910400000' , 'parentorganisation':{'parentorganisationcrmid':'89EF9173-016F-E811-A83A-000D3AB4F534'}}}
    /// </summary>
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
        SCII.Helper objCommon;
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

            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(SCII.Organisation));
            int? optionSetValue;
            Guid orgId = Guid.Empty;
            Entity existingAccountRecord = new Entity();
            StringBuilder ErrorMessage = new StringBuilder();
            string _uniqueReference = string.Empty;
            string _crn = string.Empty;
            bool isOrgExists = false;
            Entity AccountObject = new Entity(SCS.AccountContants.ENTITY_NAME);

            try
            {

                objCommon = new SCII.Helper(executionContext);
                objCommon.tracingService.Trace("Load CRM Service from context --- OK");

                objCommon.tracingService.Trace("attempt to seriallised");

                string jsonPayload = ReqPayload.Get(executionContext);
                SCII.UpdateOrganisation accountPayload = JsonConvert.DeserializeObject<SCII.UpdateOrganisation>(jsonPayload);
                objCommon.tracingService.Trace("seriallised object working"+accountPayload.organisationid+","+accountPayload.updates.name);

                var ValidationContext = new ValidationContext(accountPayload, serviceProvider: null, items: null);
                ICollection<ValidationResult> ValidationResults = null;
                ICollection<ValidationResult> ValidationResultUpdates = null;

                var isValid = objCommon.Validate(accountPayload, out ValidationResults);
                Boolean isValidUpdate = accountPayload.updates == null ? true :

                   objCommon.Validate(accountPayload.updates, out ValidationResultUpdates);

                if (isValid && isValidUpdate)
                {
                    //check organisation id exists
                    if (!string.IsNullOrEmpty(accountPayload.organisationid) && !string.IsNullOrWhiteSpace(accountPayload.organisationid))
                    {

                        if (Guid.TryParse(accountPayload.organisationid, out orgId))
                        {
                            existingAccountRecord = objCommon.service.Retrieve("account", orgId, new Microsoft.Xrm.Sdk.Query.ColumnSet(SCS.AccountContants.UNIQUEREFERENCE, SCS.AccountContants.COMPANY_HOUSE_ID, SCS.AccountContants.PARENTACCOUNTID));
                            if (existingAccountRecord != null && existingAccountRecord.Id != null)
                            {
                                AccountObject.Id = existingAccountRecord.Id;
                                _uniqueReference = (string)existingAccountRecord[SCS.AccountContants.UNIQUEREFERENCE];
                                _crn = (string)existingAccountRecord[SCS.AccountContants.COMPANY_HOUSE_ID];
                                isOrgExists = true;
                            }

                        }
                    }
                    // if org exists then go on to update the organisation
                    if (isOrgExists&&accountPayload.updates!=null)
                    {


                        objCommon.tracingService.Trace("length{0}", accountPayload.updates.name.Length);
                        if (accountPayload.updates.hierarchylevel != 0)
                        {
                            objCommon.tracingService.Trace("hierarchylevel level: {0}", accountPayload.updates.hierarchylevel);

                            if (!String.IsNullOrEmpty(Enum.GetName(typeof(SCSE.defra_OrganisationHierarchyLevel), accountPayload.updates.hierarchylevel)))
                            {

                                objCommon.tracingService.Trace("before assinging value");

                                AccountObject[SCS.AccountContants.HIERARCHYLEVEL] = new OptionSetValue(accountPayload.updates.hierarchylevel);
                                objCommon.tracingService.Trace("after assinging value");
                            }
                            else
                            {

                                ErrorMessage = ErrorMessage.Append(String.Format("Option set value {0} for orgnisation hirarchy level not found.",
                                accountPayload.updates.hierarchylevel));
                            }
                        }

                        if (!String.IsNullOrEmpty(Enum.GetName(typeof(SCSE.defra_OrganisationType), accountPayload.updates.type)))
                        {
                            optionSetValue = accountPayload.updates.type;
                            objCommon.tracingService.Trace("before assigning type  " + accountPayload.updates.type);
                            objCommon.tracingService.Trace(optionSetValue.ToString());
                            objCommon.tracingService.Trace("after  setting up option set value");
                            OptionSetValueCollection BusinessTypes = new OptionSetValueCollection();
                            BusinessTypes.Add(new OptionSetValue(optionSetValue.Value));
                            AccountObject[SCS.AccountContants.TYPE] = BusinessTypes;
                        }
                        else
                        {
                            ErrorMessage = ErrorMessage.Append(String.Format("Option set value {0} for orgnisation type does not exists.",
                            accountPayload.updates.type));
                        }

                        //check if crn exists
                        if (accountPayload.updates.crn != null && _crn != accountPayload.updates.crn)
                        {
                            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);
                            var checkCRNExistis = from c in orgSvcContext.CreateQuery("account")
                                                  where (string)c[SCS.AccountContants.COMPANY_HOUSE_ID] == accountPayload.updates.crn
                                                  select new { organisationid = c.Id };


                            if (checkCRNExistis.FirstOrDefault() == null)
                            {
                                AccountObject[SCS.AccountContants.COMPANY_HOUSE_ID] = accountPayload.updates.crn;
                            }
                            else
                            {
                                _errorCode = 412;
                                ErrorMessage = ErrorMessage.Append(String.Format("Company house id already exists."));
                            }
                        }
                        if (accountPayload.updates.name != null)
                            AccountObject[SCS.AccountContants.NAME] = accountPayload.updates.name;
                        if (accountPayload.updates.telephone != null)
                            AccountObject[SCS.AccountContants.TELEPHONE1] = accountPayload.updates.telephone;
                        objCommon.tracingService.Trace("after  setting other fields");

                        bool IsValidGuid;
                        Guid ParentAccountId;
                        if (accountPayload.updates.parentorganisation != null && !String.IsNullOrEmpty(accountPayload.updates.parentorganisation.parentorganisationcrmid))
                        {
                            IsValidGuid = Guid.TryParse(accountPayload.updates.parentorganisation.parentorganisationcrmid, out ParentAccountId);
                            if (IsValidGuid)
                            {
                                if (existingAccountRecord.Contains(SCS.AccountContants.PARENTACCOUNTID))
                                {
                                    objCommon.tracingService.Trace("inside parent update:" + ParentAccountId);
                                    if (((EntityReference)existingAccountRecord[SCS.AccountContants.PARENTACCOUNTID]).Id.ToString() != accountPayload.updates.parentorganisation.parentorganisationcrmid)
                                    {

                                        AccountObject[SCS.AccountContants.PARENTACCOUNTID] = new EntityReference(SCS.AccountContants.ENTITY_NAME, ParentAccountId);
                                    }
                                }
                                else
                                {
                                    AccountObject[SCS.AccountContants.PARENTACCOUNTID] = new EntityReference(SCS.AccountContants.ENTITY_NAME, ParentAccountId);
                                }
                            }
                            else
                            {
                                ErrorMessage = ErrorMessage.Append(String.Format("parentorganisationcrmid: {0} is not valid guid",
                         accountPayload.updates.parentorganisation.parentorganisationcrmid));
                            }

                        }
                        if (accountPayload.updates.email != null)
                            AccountObject[SCS.AccountContants.EMAILADDRESS1] = accountPayload.updates.email;
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
                    if (ValidationResultUpdates != null)
                        foreach (ValidationResult vr in ValidationResultUpdates)
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
                SCIIR.AccountResponse responsePayload = new SCIIR.AccountResponse()
                {
                    code = _errorCode,
                    message = ErrorMessage.ToString(),
                    datetime = DateTime.UtcNow,
                    version = "1.0.0.2",

                    status = _errorCode == 200 ? "success" : "failure",
                    data = new SCIIR.AccountData()
                    {
                        accountid = AccountObject.Id,
                        uniquerefere = _uniqueReference,
                        error = new SCIIR.ResponseErrorBase() { details = _errorMessageDetail }
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