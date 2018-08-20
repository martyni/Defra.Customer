namespace Defra.CustMaster.Identity.WfActivities
{
    using System.Activities;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    public class GetSourceType : WorkFlowActivityBase
    {
        [RequiredArgument]
        [Input("SoruceTypeValue")]
        public InArgument<int> SourceTypeValue { get; set; }

        [Output("Sorurce")]
        [AttributeTarget("account", "defra_creationsource")]
        public OutArgument<OptionSetValue> Sorurce { get; set; }

        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            crmWorkflowContext.Trace("Started: Defra.CustMaster.Identity.WfActivities.ContactDetailType");

            Sorurce.Set(executionContext, new OptionSetValue(SourceTypeValue.Get(executionContext)));

            crmWorkflowContext.Trace("Finished: Defra.CustMaster.Identity.WfActivities.ContactDetailType");
        }
    }
}
