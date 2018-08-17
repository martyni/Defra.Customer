namespace Defra.CustMaster.Identity.WfActivities
{
    using System.Activities;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    public class GetHirarchyLevel : WorkFlowActivityBase
    {
        [RequiredArgument]
        [Input("RequestTypeValue")]
        public InArgument<int> HirarchyLevel { get; set; }

        [Output("HierarchyLevelType")]
        [AttributeTarget("account", "defra_hierarchylevel")]
        public OutArgument<OptionSetValue> HiarachyLevelOptionSetValue { get; set; }

        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            crmWorkflowContext.Trace("Started: Defra.CustMaster.Identity.WfActivities.SetHirarchyLevel");

            HiarachyLevelOptionSetValue.Set(executionContext, new OptionSetValue(HirarchyLevel.Get(executionContext)));

            crmWorkflowContext.Trace("Finished: Defra.CustMaster.Identity.WfActivities.SetHirarchyLevel");
        }
    }
}
