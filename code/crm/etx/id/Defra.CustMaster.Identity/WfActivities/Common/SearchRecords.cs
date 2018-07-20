using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Defra.CustMaster.Identity.WfActivities.Common
{
   public class SearchRecords : WorkFlowActivityBase
    {
        [RequiredArgument]
        [Input("AdvancedFindXml")]
        public InArgument<String> fetchQuery { get; set; }

        [RequiredArgument]
        [Input("ReplaceString")]
        public InArgument<string> StringToReplace { get; set; }

        [Output("RecordCount")]
        public OutArgument<Int32> RecordCount { get; set; }

        

        string AdvancedFindXml;
        string Replacestring;
        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            AdvancedFindXml = fetchQuery.Get(executionContext);
            crmWorkflowContext.TracingService.Trace("Started: Defra.CustMaster.Identity.WfActivities.ExecuteCRMWorkFlowActivity");
            //Create the IWorkflowContext and the

            Replacestring = StringToReplace.Get(executionContext);
            AdvancedFindXml = AdvancedFindXml.Replace("XXX", StringToReplace.Get(executionContext));
            var results = crmWorkflowContext.OrganizationService.RetrieveMultiple(new FetchExpression(AdvancedFindXml));
            crmWorkflowContext.Trace(AdvancedFindXml);

            RecordCount.Set(executionContext, results.Entities.Count);
            crmWorkflowContext.Trace("Finished: Defra.CustMaster.Identity.WfActivities.ExecuteCRMWorkFlowActivity");
        }
    }
}
