namespace Defra.CustMaster.Identity.WfActivities
{
    using System.Activities;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    public class ContactDetailType : WorkFlowActivityBase
    {
        [RequiredArgument]
        [Input("RequestTypeValue")]
        public InArgument<int> RequestTypeValue { get; set; }

        [Output("TypeValue")]
        [AttributeTarget("defra_addressdetails", "defra_addresstype")]
        public OutArgument<OptionSetValue> TypeValue { get; set; }

        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            crmWorkflowContext.Trace("Started: Defra.CustMaster.Identity.WfActivities.ContactDetailType");

            TypeValue.Set(executionContext, new OptionSetValue(RequestTypeValue.Get(executionContext)));

            crmWorkflowContext.Trace("Finished: Defra.CustMaster.Identity.WfActivities.ContactDetailType");
        }
    }
}
