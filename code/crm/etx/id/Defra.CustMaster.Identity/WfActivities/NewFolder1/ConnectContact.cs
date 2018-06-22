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

namespace Defra.CustMaster.Identity.WfActivities.Connection
{
    public class ConnectContact : WorkFlowActivityBase
    {
        #region "Parameter Definition"
        [RequiredArgument]
        [Input("PayLoad")]
        public InArgument<String> PayLoad { get; set; }
        [Output("OutPutJson")]
        public OutArgument<string> ReturnMessageDetails { get; set; }

        #endregion


        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            String PayloadDetails = PayLoad.Get(executionContext);
            int ErrorCode = 400; //400 -- bad request
            String _ErrorMessage = string.Empty;
            String _ErrorMessageDetail = string.Empty;
            Guid ContactId = Guid.Empty;
            Guid AccountId = Guid.Empty;
            StringBuilder ErrorMessage = new StringBuilder();
            String UniqueReference = string.Empty;

            try
            {

            }

            #region Catch Exception
            catch (Exception ex)
            {
                crmWorkflowContext.Trace("inside catch");
                ErrorCode = 500;
                _ErrorMessage = "Error occured while processing request";
                _ErrorMessageDetail = ex.Message;
                ErrorCode = 400;
                this.ReturnMessageDetails.Set(executionContext, _ErrorMessageDetail);

            }
            #endregion

            #region Finally Block
            finally
            {

            } 
            #endregion
        }
    }

}
