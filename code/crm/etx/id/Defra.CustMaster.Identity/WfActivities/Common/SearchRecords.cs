namespace Defra.CustMaster.Identity.WfActivities
{
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Xrm.Sdk.Workflow;
    using System;
    using System.Activities;

    public class SearchRecords : WorkFlowActivityBase
    {
        [RequiredArgument]
        [Input("FetchXmlQuery")]
        public InArgument<string> InFetchXmlQuery { get; set; }


        [Input("Values")]
        public InArgument<string> InValues { get; set; }

        [Output("RecordCount")]
        public OutArgument<int> OutRecordCount { get; set; }

        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            crmWorkflowContext.Trace("Started: Defra.CustMaster.Identity.WfActivities.SearchRecords");
            string advancedFindXml;
            string replaceString;
            const string BREAK_CHAR = "{SEP}"; // Separator string for the values input
            string[] replaceValues;

            advancedFindXml = InFetchXmlQuery.Get(executionContext);
            if (string.IsNullOrEmpty(advancedFindXml))
            {
                throw new InvalidPluginExecutionException("Please provide valid Email Address in the Email.");
            }

            replaceString = InValues.Get(executionContext);

            crmWorkflowContext.Trace("SearchRecords: Value Replacement started...");

            if (!string.IsNullOrEmpty(replaceString))
            {
                //replaceValues = replaceString.Split(BREAK_CHAR.ToCharArray());
                replaceValues = replaceString.Split(new string[] { BREAK_CHAR }, StringSplitOptions.RemoveEmptyEntries);
                if (replaceValues.Length <= 0)
                {
                    throw new InvalidPluginExecutionException("Please provide a valid replace string in the format {value0}{SEP}{value1}{SEP}{value2}.");
                }

                int iLoop = 0;
                foreach (string replaceValue in replaceValues)
                {
                    crmWorkflowContext.Trace(string.Format("SearchRecords: Value{0} = {1}", iLoop, replaceValue));
                    advancedFindXml = advancedFindXml.Replace("{" + iLoop++ + "}", replaceValue);
                }
            }


            crmWorkflowContext.Trace("SearchRecords: Replaced query = " + advancedFindXml);

            crmWorkflowContext.Trace("SearchRecords: Value Replacement finished!");

            crmWorkflowContext.Trace("SearchRecords: Calling Retrieve Multiple...");

            EntityCollection results = crmWorkflowContext.OrganizationService.RetrieveMultiple(new FetchExpression(advancedFindXml));
            crmWorkflowContext.Trace("count from entity collection count" + results.Entities.Count);
            crmWorkflowContext.Trace("count from entity collection total record count" + results.TotalRecordCount);
            int recourdCount = 0;
            if (results != null)
            {
                recourdCount = results.TotalRecordCount;
                //recourdCount = results.Entities.Count;
            }

            crmWorkflowContext.TracingService.Trace(string.Format("SearchRecords: Found {0} records.", recourdCount));
            OutRecordCount.Set(executionContext, recourdCount);

            crmWorkflowContext.Trace("Finished: Defra.CustMaster.Identity.WfActivities.ExecuteCRMWorkFlowActivity");
        }
    }
}
