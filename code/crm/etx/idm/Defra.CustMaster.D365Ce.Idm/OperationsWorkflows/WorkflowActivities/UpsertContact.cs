using Defra.CustMaster.D365Ce.Idm.OerationsWorkflows;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Defra.Customer.Plugins.WorkflowActivities
{
    public class UpsertContact : CodeActivity
    {
        #region "Parameter Definition"
        [RequiredArgument]
        [Input("UPN")]
        public InArgument<String> UPN { get; set; }
        [Input("First name")]
        public InArgument<String> Firstname { get; set; }
        [Input("Last name")]
        public InArgument<String> Lastname { get; set; }
        [Input("Email")]
        public InArgument<String> Email { get; set; }
       
        [Output("Individual")]
        [ReferenceTarget("contact")]
        
        public OutArgument<EntityReference> Individual { get; set; }

        [Output("Is Record Created")]
        public OutArgument<Boolean> IsRecordCreated { get; set; }

        [Output("Code")]
        public OutArgument<String> Code { get; set; }

        [Output("Message")]
        public OutArgument<string> Message { get; set; }

        [Output("MessageDetail")]
        public OutArgument<string> MessageDetail { get; set; }

        #endregion
        protected override void Execute(CodeActivityContext executionContext)
        {
            #region "Load CRM Service from context"

            Common objCommon = new Common(executionContext);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            #endregion

            #region "Read Parameters"
            string _UPN = this.UPN.Get(executionContext);
            string _Firstname = this.Firstname.Get(executionContext);
            string _Lastname = this.Lastname.Get(executionContext);
            string _Email = this.Email.Get(executionContext);
            EntityReference _Contact;
            Boolean _IsRecordCreated = false;
            Int64 ErrorCode = 1; //0 -- Successful and 1 means success
            String _ErrorMessage = string.Empty;
            String _ErrorMessageDetail = string.Empty;
            Guid ContactId = Guid.Empty;
            #endregion

            #region "Upsert Execution"

            try
            {
                Entity contact = new Entity("contact");//,"defra_upn", _UPN);
                contact["firstname"] = _Firstname;
                contact["lastname"] = _Lastname;

                //search contact record based on UPN
                OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);
                var ContactWithUPN = from c in orgSvcContext.CreateQuery("contact")
                                      where  ((string) c["defra_upn"] ).Contains((_UPN.Trim()))
                                      select new { ContactId = c.Id,EmailAddress = c["emailaddress1"] };


               
                objCommon.tracingService.Trace("started.........");
                

                //search email adderss with the contact
                var ContactWithEmail = from c in orgSvcContext.CreateQuery("contact")
                                       where (string)c["emailaddress1"] == _Email &&
                                       (string)c["emailaddress1"] != String.Empty
                                       select c["contactid"];
                Guid ContactRecordGuidWithUPN = ContactWithUPN.FirstOrDefault() == null ? Guid.Empty: ContactWithUPN.FirstOrDefault().ContactId;
                if (ContactRecordGuidWithUPN == Guid.Empty)
                {
                    //no record found with UPN. create
                    if(ContactWithEmail.ToList().Count > 0 && _Email != null)
                    {
                        //Found a recprd with the provided email id
                        ErrorCode = 1;
                        _ErrorMessage = "EMAIL already exist";
                    }
                    else if (String.IsNullOrWhiteSpace(_Email))
                    {
                        ErrorCode = 1;
                        _ErrorMessage = "Email is mandatory when you want to Insert";
                    }
                    else
                    {
                        ErrorCode = 0;
                        contact["emailaddress1"] = _Email;
                        contact["defra_upn"] = _UPN;
                        ContactId = objCommon.service.Create(contact);
                        _Contact = new EntityReference("contact", ContactId);
                        this.Individual.Set(executionContext, _Contact);
                        _IsRecordCreated = true;
                    }

                }
                else 
                {
                    objCommon.tracingService.Trace("inside update 01");
                    //this update
                    if (String.IsNullOrWhiteSpace(_Lastname))
                    {
                        objCommon.tracingService.Trace("inside update 01.1");

                        ErrorCode = 1;
                        _ErrorMessage = "Last name can never be empty.";
                    }
                    else if(_Email != null && _Email.Length > 1)
                    {
                        objCommon.tracingService.Trace("inside update 01.2");

                        ErrorCode = 1;
                        _ErrorMessage = "Email address cannot be updated.";
                    }
                    else
                    {
                        objCommon.tracingService.Trace("inside update 01.3");

                        objCommon.tracingService.Trace("Guid: " + ContactRecordGuidWithUPN.ToString());
                        objCommon.tracingService.Trace("inside update 01.4");

                        contact.Id = ContactRecordGuidWithUPN;
                        objCommon.tracingService.Trace("inside update");
                        objCommon.service.Update(contact);
                        objCommon.tracingService.Trace("after update");
                        ErrorCode = 0;
                        _Contact = new EntityReference("contact", ContactRecordGuidWithUPN);
                        this.Individual.Set(executionContext, _Contact);
                        objCommon.tracingService.Trace("after setting");
                    }
                }

                
                
                this.IsRecordCreated.Set(executionContext, _IsRecordCreated);
                this.Code.Set(executionContext, ErrorCode.ToString());
                this.Message.Set(executionContext, _ErrorMessage);
                this.MessageDetail.Set(executionContext, _ErrorMessageDetail);



            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                ErrorCode = 1;
                _ErrorMessage = "Error occured while processing request";
                _ErrorMessageDetail = ex.Message;
                //throw ex;
                this.Code.Set(executionContext, ErrorCode.ToString());
                this.Message.Set(executionContext, _ErrorMessage);
                this.MessageDetail.Set(executionContext, _ErrorMessageDetail);

            }
           
            #endregion

        }
    }
}
