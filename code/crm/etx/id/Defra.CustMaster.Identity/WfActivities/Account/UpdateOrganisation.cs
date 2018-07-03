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
    ///{'organisationid': 'D1B35E7C-D072-E811-A83B-000D3AB4F7AF','updates':{'validatedwithcompanieshouse':'false','name':'Update my create', 'type':'910400000', 'crn':'18062018','email':'Updateacme@acme.com', 'telephone':'004412345678', 'hierarchylevel':'910400000' , 'parentorganisationcrmid':'89EF9173-016F-E811-A83A-000D3AB4F534'},'clearlist':{'fields':['crn','email','parentorganisationcrmid','hierarchylevel','validatedwithcompanieshouse']}}
    /// {'organisationid': 'D1B35E7C-D072-E811-A83B-000D3AB4F7AF','updates':{'validatedwithcompanieshouse':true,'name':'Update my create', 'type':910400000, 'crn':'18062018','email':'Updateacme@acme.com', 'telephone':'004412345678', 'hierarchylevel':910400000 , 'parentorganisationcrmid':'89EF9173-016F-E811-A83A-000D3AB4F534'}}
    ///{'organisationid': '35282D2C-AB73-E811-A83B-000D3AB4FFEF','updates':{'validatedwithcompanieshouse':true,'name':'Update my create', 'type':910400000, 'crn':'18062018','email':'Updateacme@acme.com', 'telephone':'004412345678', 'hierarchylevel':910400000 , 'parentorganisationcrmid':'89EF9173-016F-E811-A83A-000D3AB4F534'}}
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


        #region Execute
        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            #region local variables
            SCII.Helper objCommon;
            //EntityReference _Contact;
            int _errorCode = 400; //Bad Request

            string _errorMessageDetail = string.Empty;



            int? optionSetValue;
            Guid orgId = Guid.Empty;
            Entity existingAccountRecord = new Entity();
            StringBuilder _errorMessage = new StringBuilder();
            string _uniqueReference = string.Empty;
            string _crn = string.Empty;
            bool isOrgExists = false;
            #endregion

            LocalWorkflowContext localcontext = new LocalWorkflowContext(executionContext);
            Entity AccountObject = new Entity(SCS.AccountContants.ENTITY_NAME);

            try
            {

                objCommon = new SCII.Helper(executionContext);
                OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);

                localcontext.Trace("started execution");


                localcontext.Trace("attempt to seriallised");

                string jsonPayload = ReqPayload.Get(executionContext);
                SCII.UpdateOrganisationRequest accountPayload = JsonConvert.DeserializeObject<SCII.UpdateOrganisationRequest>(jsonPayload);
                //if (accountPayload.clearlist != null)
                //    //localcontext.Trace("TRACE TO CHECK:" + accountPayload.clearlist[0].ToString());
                //    localcontext.Trace("TRACE TO CHECK:" + accountPayload.clearlist.fields[0].ToString());

                var ValidationContext = new ValidationContext(accountPayload, serviceProvider: null, items: null);
                ICollection<ValidationResult> ValidationResults = null;
                ICollection<ValidationResult> ValidationResultUpdates = null;

                var isValid = objCommon.Validate(accountPayload, out ValidationResults);
                Boolean isValidUpdate = accountPayload.updates == null ? true :

                   objCommon.Validate(accountPayload.updates, out ValidationResultUpdates);

                localcontext.Trace("TRACE TO valid:" + isValid);
                if (isValid && isValidUpdate)
                {
                    //check organisation id exists
                    if (!string.IsNullOrEmpty(accountPayload.organisationid) && !string.IsNullOrWhiteSpace(accountPayload.organisationid))
                    {

                        if (Guid.TryParse(accountPayload.organisationid, out orgId))
                        {
                            existingAccountRecord = objCommon.service.Retrieve(SCS.AccountContants.ENTITY_NAME, orgId, new Microsoft.Xrm.Sdk.Query.ColumnSet(SCS.AccountContants.UNIQUEREFERENCE, SCS.AccountContants.COMPANY_HOUSE_ID, SCS.AccountContants.PARENTACCOUNTID));
                            if (existingAccountRecord != null && existingAccountRecord.Id != null)
                            {
                                AccountObject.Id = existingAccountRecord.Id;
                                _uniqueReference = (string)existingAccountRecord[SCS.AccountContants.UNIQUEREFERENCE];
                                if (existingAccountRecord.Contains(SCS.AccountContants.COMPANY_HOUSE_ID))
                                    _crn = (string)existingAccountRecord[SCS.AccountContants.COMPANY_HOUSE_ID];
                                isOrgExists = true;
                            }

                        }
                    }
                    // if org exists then go on to update the organisation
                    if (isOrgExists && accountPayload.updates != null)
                    {
                        localcontext.Trace("length:" + accountPayload.updates.name.Length);
                        //if (accountPayload.updates.hierarchylevel != 0)

                        #region Cannot be Cleared Update Fields
                        if (accountPayload.updates.name != null)
                            AccountObject[SCS.AccountContants.NAME] = accountPayload.updates.name;
                        if (accountPayload.updates.type != null)
                        {
                            if (!String.IsNullOrEmpty(Enum.GetName(typeof(SCSE.defra_OrganisationType), accountPayload.updates.type)))
                            {
                                optionSetValue = accountPayload.updates.type;
                                localcontext.Trace("before assigning type  " + accountPayload.updates.type);
                                localcontext.Trace(optionSetValue.ToString());
                                localcontext.Trace("after  setting up option set value");
                                OptionSetValueCollection BusinessTypes = new OptionSetValueCollection();
                                BusinessTypes.Add(new OptionSetValue(optionSetValue.Value));
                                AccountObject[SCS.AccountContants.TYPE] = BusinessTypes;
                            }
                            else
                            {
                                _errorMessage = _errorMessage.Append(String.Format("Option set value {0} for orgnisation type does not exists.",
                                accountPayload.updates.type));
                            }
                        }

                        #endregion

                        #region Clear able update fields
                        //Flag to check if the clearing of data is required for the selected OrganisationRequest fields
                        bool clearRequired = accountPayload.clearlist != null &&
                                             accountPayload.clearlist.fields != null && accountPayload.clearlist.fields.Length > 0;

                        //Hierarchy Level
                        if (clearRequired && accountPayload.clearlist.fields.Contains(SCII.OrganisationClearFields.hierarchylevel))
                        {
                            AccountObject[SCS.AccountContants.HIERARCHYLEVEL] = null;
                        }
                        else if (accountPayload.updates.hierarchylevel != null)
                        {
                            localcontext.Trace("hierarchylevel level:" + accountPayload.updates.hierarchylevel);

                            if (!String.IsNullOrEmpty(Enum.GetName(typeof(SCSE.defra_OrganisationHierarchyLevel), accountPayload.updates.hierarchylevel)))
                            {

                                localcontext.Trace("before assinging value");

                                AccountObject[SCS.AccountContants.HIERARCHYLEVEL] = new OptionSetValue((int)accountPayload.updates.hierarchylevel);
                                localcontext.Trace("after assinging value");
                            }
                            else
                            {

                                _errorMessage = _errorMessage.Append(String.Format("Option set value {0} for orgnisation hirarchy level not found.",
                                accountPayload.updates.hierarchylevel));
                            }
                        }


                        if (clearRequired && accountPayload.clearlist.fields.Contains(SCII.OrganisationClearFields.crn))
                        {
                            AccountObject[SCS.AccountContants.COMPANY_HOUSE_ID] = null;
                        }
                        else
                        {

                            //check if crn exists

                            if (accountPayload.updates.crn != null && _crn != accountPayload.updates.crn)
                            {
                                orgSvcContext = new OrganizationServiceContext(objCommon.service);
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
                                    _errorMessage = _errorMessage.Append(String.Format("Company house id already exists."));
                                }
                            }
                        }
                        if (clearRequired && accountPayload.clearlist.fields.Contains(SCII.OrganisationClearFields.telephone))
                        {
                            AccountObject[SCS.AccountContants.TELEPHONE1] = null;
                        }
                        else
                        {
                            if (accountPayload.updates.telephone != null)
                                AccountObject[SCS.AccountContants.TELEPHONE1] = accountPayload.updates.telephone;
                        }
                        localcontext.Trace("after  setting other fields");
                        if (clearRequired && accountPayload.clearlist.fields.Contains(SCII.OrganisationClearFields.parentorganisationcrmid))
                        {
                            AccountObject[SCS.AccountContants.PARENTACCOUNTID] = null;
                        }
                        else
                        {
                            bool IsValidGuid;
                            Guid ParentAccountId;
                            if (!String.IsNullOrEmpty(accountPayload.updates.parentorganisationcrmid))
                            {
                                IsValidGuid = Guid.TryParse(accountPayload.updates.parentorganisationcrmid, out ParentAccountId);
                                if (IsValidGuid)
                                {
                                    var checkParentOrgExists = from c in orgSvcContext.CreateQuery(SCS.AccountContants.ENTITY_NAME)
                                                               where (string)c[SCS.AccountContants.ACCOUNTID] == accountPayload.updates.parentorganisationcrmid
                                                               select new
                                                               {
                                                                   organisationid = c.Id
                                                               };
                                    if (checkParentOrgExists.FirstOrDefault() != null)
                                    {
                                        if (existingAccountRecord.Contains(SCS.AccountContants.PARENTACCOUNTID))
                                        {
                                            localcontext.Trace("inside parent update:" + ParentAccountId);
                                            if (((EntityReference)existingAccountRecord[SCS.AccountContants.PARENTACCOUNTID]).Id.ToString() != accountPayload.updates.parentorganisationcrmid)
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
                                        localcontext.Trace("throwing error becuase organisation does not exists.");
                                        throw new Exception("Parent account id does not exists.");
                                    }
                                }
                                else
                                {

                                    localcontext.Trace("throwing error becuase invalid Guid;");
                                    throw new Exception("parentorganisationcrmid is not valid guid;");
                                }

                            }
                        }
                        if (clearRequired && accountPayload.clearlist.fields.Contains(SCII.OrganisationClearFields.email))
                        {
                            AccountObject[SCS.AccountContants.EMAILADDRESS1] = null;
                        }
                        else
                        {
                            if (accountPayload.updates.email != null)
                                AccountObject[SCS.AccountContants.EMAILADDRESS1] = accountPayload.updates.email;
                        }
                        if (clearRequired && accountPayload.clearlist.fields.Contains(SCII.OrganisationClearFields.validatedwithcompanieshouse))
                        {
                            AccountObject[SCS.AccountContants.VALIDATED_WITH_COMPANYHOUSE] = null;
                        }
                        else
                        {
                            if (accountPayload.updates.validatedwithcompanieshouse != null)
                            {
                                localcontext.Trace("inside validated with companies house:" + accountPayload.updates.validatedwithcompanieshouse);
                                bool isValidCompaniesHouse = false;
                                if (Boolean.TryParse(accountPayload.updates.validatedwithcompanieshouse.ToString(), out isValidCompaniesHouse))
                                {
                                    AccountObject[SCS.AccountContants.VALIDATED_WITH_COMPANYHOUSE] = isValidCompaniesHouse;
                                }
                                else
                                {
                                    _errorMessage = _errorMessage.Append(String.Format("validated with companyhouse value {0} is not valid;",
                            accountPayload.updates.validatedwithcompanieshouse));
                                }

                            }
                        }
                        #endregion
                        localcontext.Trace("outside validated with companies house:" + accountPayload.updates.validatedwithcompanieshouse);
                        localcontext.Trace("before updating guid:" + AccountObject.Id.ToString());
                        objCommon.service.Update(AccountObject);
                        localcontext.Trace("after updating guid:" + AccountObject.Id.ToString());

                        _errorCode = 200;

                    }
                    //if the organisation does not exists
                    else
                    {
                        _errorCode = 404;
                        _errorMessage = _errorMessage.Append(String.Format("Oranisation with id {0} does not exists.",
                        accountPayload.organisationid));
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
                    if (ValidationResultUpdates != null)
                        foreach (ValidationResult vr in ValidationResultUpdates)
                        {
                            _errorMessage.Append(vr.ErrorMessage + " ");
                        }
                    _errorCode = 400;

                }

            }

            catch (Exception ex)
            {
                localcontext.Trace("inside exception");

                _errorCode = 500;
                _errorMessage = _errorMessage.Append("Error occured while processing request");
                _errorMessageDetail = ex.Message;
                localcontext.Trace(ex.Message);

            }
            finally
            {


                localcontext.Trace("finally block start");
                SCIIR.AccountResponse responsePayload = new SCIIR.AccountResponse()
                {
                    code = _errorCode,
                    message = _errorMessage.ToString(),
                    datetime = DateTime.UtcNow,
                    version = "1.0.0.2",

                    status = _errorCode == 200 ? "success" : "failure",
                    data = new SCIIR.AccountData()
                    {
                        accountid = AccountObject.Id,
                        uniquereference = _uniqueReference,
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