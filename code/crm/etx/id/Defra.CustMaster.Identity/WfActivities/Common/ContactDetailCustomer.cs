using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using SCS = Defra.CustMaster.D365.Common.schema;
using SCII = Defra.CustMaster.D365.Common.Ints.Idm;
using Microsoft.Xrm.Sdk.Client;

namespace Defra.CustMaster.Identity.WfActivities
{
    public class ContactDetailCustomer : WorkFlowActivityBase
    {
        [Input("ContactDetailsId")]
        public InArgument<string> ContactDetailsId { get; set; }

        [Output("CustomerId")]
        
        public OutArgument<string> CustomerId { get; set; }        

        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            Guid contactDetailsGuid=Guid.Empty;
            Guid customerId = Guid.Empty;
          
           string contactDetailsId = ContactDetailsId.Get(executionContext);
            contactDetailsGuid = Guid.Parse(contactDetailsId);
            SCII.Helper objCommon = new SCII.Helper(executionContext);
            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);



            crmWorkflowContext.Trace("Contact Details search:started..");
            //var customerContact = from c in orgSvcContext.CreateQuery(SCS.ContactDetails.ENTITY)
            //                       where ((string)c[SCS.ContactDetails.ContactDetailsId]).Equals(contactDetailsId) && (int)c[SCS.ContactDetails.STATECODE] == 0
            //                       select new { Customer = c[SCS.ContactDetails.CUSTOMER]};

            Entity customer = objCommon.service.Retrieve(SCS.ContactDetails.ENTITY, contactDetailsGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
           
            if (customer != null)
            {
                customerId=((EntityReference)customer[SCS.ContactDetails.CUSTOMER]).Id;
                crmWorkflowContext.Trace("Contact Details Customer:" + customerId);

                if (customerId != Guid.Empty)
                {
                   
                    CustomerId.Set(executionContext, customerId.ToString());
                }
            }
            crmWorkflowContext.Trace("end of Contact Details ContactDetails:" + CustomerId);
        }
    }
}
   