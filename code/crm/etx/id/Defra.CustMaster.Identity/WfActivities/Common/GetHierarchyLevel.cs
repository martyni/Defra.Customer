namespace Defra.CustMaster.Identity.WfActivities
{
    using System.Activities;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    public class GetHierarchyLevel : WorkFlowActivityBase
    {
        [RequiredArgument]
        [Input("HierarchyTypeValue")]
        public InArgument<int> HierarchyTypeValue { get; set; }

        [Output("HierarchyType")]
        [AttributeTarget("account", "defra_hierarchylevel")]
        public OutArgument<OptionSetValue> HierarchyType { get; set; }

        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            crmWorkflowContext.Trace("Started: Defra.CustMaster.Identity.WfActivities.ContactDetailType");

            HierarchyType.Set(executionContext, new OptionSetValue(HierarchyTypeValue.Get(executionContext)));

            crmWorkflowContext.Trace("Finished: Defra.CustMaster.Identity.WfActivities.ContactDetailType");
        }
    }
}
