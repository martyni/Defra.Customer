namespace Defra.CustMaster.Identity.WfActivities
{
    using System.Activities;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    public class EnrolmentStatusType : WorkFlowActivityBase
    {
        [RequiredArgument]
        [Input("OptionValue")]
        public InArgument<int> OptionValue { get; set; }

        [Output("TypeValue")]
        [AttributeTarget("defra_lobserviceuserlink", "defra_enrolmentstatus")]
        public OutArgument<OptionSetValue> TypeValue { get; set; }

        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            crmWorkflowContext.Trace("Started: Defra.CustMaster.Identity.WfActivities.EnrolmentStatusType");

            TypeValue.Set(executionContext, new OptionSetValue(OptionValue.Get(executionContext)));

            crmWorkflowContext.Trace("Finished: Defra.CustMaster.Identity.WfActivities.EnrolmentStatusType");
        }
    }
}
