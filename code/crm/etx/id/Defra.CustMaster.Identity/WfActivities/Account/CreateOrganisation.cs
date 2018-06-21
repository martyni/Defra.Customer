using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Workflow;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SCII = Defra.CustMaster.D365.Common.Ints.Idm;
using SCIIR = Defra.CustMaster.D365.Common.Ints.Idm.Resp;
using SCSE = Defra.CustMaster.D365.Common.Schema.ExtEnums;
using SCS = Defra.CustMaster.D365.Common.schema;

namespace Defra.CustMaster.Identity.WfActivities
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

        SCII.Helper objCommon;


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

                objCommon = new SCII.Helper(executionContext);
                objCommon.tracingService.Trace("Load CRM Service from context --- OK");
                Entity AccountObject = new Entity(Defra.CustMaster.D365.Common.schema.AccountContants.ENTITY_NAME);

                objCommon.tracingService.Trace("attempt to seriallised new");
                SCII.OrganisationRequest AccountPayload = JsonConvert.DeserializeObject<SCII.OrganisationRequest>(PayloadDetails);
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

                        if (!String.IsNullOrEmpty(Enum.GetName(typeof(SCSE.defra_OrganisationHierarchyLevel), AccountPayload.hierarchylevel)))
                        {

                            objCommon.tracingService.Trace("before assinging value");

                            AccountObject[Defra.CustMaster.D365.Common.schema.AccountContants.HIERARCHYLEVEL] = new OptionSetValue(AccountPayload.hierarchylevel);
                            objCommon.tracingService.Trace("after assinging value");


                            if (!String.IsNullOrEmpty(Enum.GetName(typeof(SCSE.defra_OrganisationType), AccountPayload.type)))
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
                                    if (AccountPayload.validatedwithcompanieshouse != null)
                                    {
                                        bool isValidCompaniesHouse = false;
                                        if (Boolean.TryParse(AccountPayload.validatedwithcompanieshouse.ToString(), out isValidCompaniesHouse))
                                        {
                                            AccountObject[SCS.AccountContants.VALIDATED_WITH_COMPANYHOUSE] = isValidCompaniesHouse;
                                        }
                                        else
                                        {
                                            ErrorMessage = ErrorMessage.Append(String.Format("validated with companyhouse value {0} is not valid;",
                                    AccountPayload.validatedwithcompanieshouse));
                                        }

                                    }
                                    objCommon.tracingService.Trace("after  setting other fields");

                                    bool IsValidGuid;
                                    Guid ParentAccountId;
                                    if (AccountPayload.parentorganisation != null && !String.IsNullOrEmpty(AccountPayload.parentorganisation.parentorganisationcrmid))
                                    {

                                        objCommon.tracingService.Trace("before checking value");
                                        IsValidGuid = Guid.TryParse(AccountPayload.parentorganisation.parentorganisationcrmid, out ParentAccountId);
                                        if (IsValidGuid)
                                        {
                                            var checkParentOrgExists = from c in orgSvcContext.CreateQuery("account")
                                                                       where (string)c[Defra.CustMaster.D365.Common.schema.AccountContants.ACCOUNTID] == AccountPayload.parentorganisation.parentorganisationcrmid
                                                                       select new
                                                                       {
                                                                           organisationid = c.Id
                                                                       };
                                            if (checkParentOrgExists.FirstOrDefault() != null)
                                            {
                                                AccountObject[Defra.CustMaster.D365.Common.schema.AccountContants.PARENTACCOUNTID]
                                                        = new EntityReference(SCS.AccountContants.ENTITY_NAME, ParentAccountId);
                                                objCommon.tracingService.Trace("after assinging value");
                                            }
                                            else
                                            {
                                                objCommon.tracingService.Trace("throwing error becuase organisation does not exists.");
                                                throw new Exception("Parent account id does not exists.");
                                            }
                                        }
                                        else
                                        {
                                            objCommon.tracingService.Trace("invalid Guid.");
                                            throw new Exception("Invalid parent account Id.");
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
                            else
                            {
                                ErrorCode = 400;
                                ErrorMessage = ErrorMessage.Append(String.Format("Option set value {0} for orgnisation type does not exists.",
                                AccountPayload.type));
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
                SCIIR.AccountResponse responsePayload = new SCIIR.AccountResponse()
                {
                    code = ErrorCode,
                    message = ErrorMessage.ToString(),
                    datetime = DateTime.UtcNow,
                    version = "1.0.0.2",

                    status = ErrorCode == 200 ? "success" : "failure",
                    data = new SCIIR.AccountData()
                    {
                        accountid = CrmGuid,
                        uniquereference = UniqueReference,
                        error = new SCIIR.ResponseErrorBase() { details = _ErrorMessageDetail }
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

