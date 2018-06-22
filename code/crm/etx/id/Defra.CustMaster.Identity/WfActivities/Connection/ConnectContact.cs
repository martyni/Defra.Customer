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
using Microsoft.Xrm.Sdk.Query;

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


        LocalWorkflowContext localcontext = null;

        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            localcontext = crmWorkflowContext;
            String PayloadDetails = PayLoad.Get(executionContext);
            int ErrorCode = 400; //400 -- bad request
            int RoleCountToCheck = 0;
            String _ErrorMessage = string.Empty;
            String _ErrorMessageDetail = string.Empty;
            Guid ContactId = Guid.Empty;
            Guid AccountId = Guid.Empty;
            StringBuilder ErrorMessage = new StringBuilder();
            String UniqueReference = string.Empty;
            Guid? ToConnectId = Guid.Empty;
            SCII.Helper objCommon = new SCII.Helper(executionContext);
            try
            {
                SCII.ConnectContactRequest contactPayload = JsonConvert.DeserializeObject<SCII.ConnectContactRequest>(PayloadDetails);
                EntityReference FromEntityContact;
                EntityReference ToEntityAccount;

                var ValidationContext = new ValidationContext(contactPayload, serviceProvider: null, items: null);
                ICollection<ValidationResult> ValidationResults = null;

                var isValid = objCommon.Validate(contactPayload, out ValidationResults);


                if (isValid)
                {
                    //get role ids

                    #region Validate record exists
                    //check if account record exists
                    FromEntityContact = new EntityReference(SCS.Contact.ENTITY, Guid.Parse(contactPayload.contactid));
                    ToEntityAccount = new EntityReference(SCS.AccountContants.ENTITY_NAME, Guid.Parse(contactPayload.contactid));
                    Boolean ContactExists = CheckIfRecordExists(FromEntityContact);
                    Boolean AccountExists = CheckIfRecordExists(ToEntityAccount);
                    if (ContactExists && AccountExists)
                    {
                        #region Getting connection role IDs

                        List<String> RoleNames = new List<string>();
                        if (contactPayload.fromrole != null)
                        {
                            RoleNames.Add(contactPayload.fromrole);
                            RoleCountToCheck++;
                        }
                        if (contactPayload.torole != null)
                        {
                            RoleNames.Add(contactPayload.torole);
                            RoleCountToCheck++;

                        }
                        RoleNames.Add(SCS.Connection.PRIMARYUSERROLENAME);

                        List<Entity> RolesList = GetRoles(RoleNames);
                        EntityReference FromEntityRole = null;
                        EntityReference ToEntityRole = null;
                        EntityReference PrimaryUserRole = null;

                        foreach (Entity ConnectionRoles in RolesList)
                        {
                            if (contactPayload.fromrole == (string)ConnectionRoles[SCS.Connection.NAME])
                            {
                                FromEntityRole = new EntityReference(ConnectionRoles.LogicalName, ConnectionRoles.Id);
                            }

                            if (contactPayload.torole == (string)ConnectionRoles[SCS.Connection.NAME])
                            {
                                ToEntityRole = new EntityReference(ConnectionRoles.LogicalName, ConnectionRoles.Id);
                            }

                            if (SCS.Connection.PRIMARYUSERROLENAME == (string)ConnectionRoles[SCS.Connection.NAME])
                            {
                                PrimaryUserRole = new EntityReference(ConnectionRoles.LogicalName, ConnectionRoles.Id);
                            }
                        } 

                        if(!String.IsNullOrEmpty(contactPayload.fromrole ) && FromEntityRole == null)
                        {
                            //from role not found
                            ErrorCode = 404;
                            _ErrorMessage = String.Format( "From role {0} not found.", contactPayload.fromrole);
                        }
                        

                        if (!String.IsNullOrEmpty(contactPayload.torole) && ToEntityAccount == null)
                        {
                            //from role not found
                            ErrorCode = 404;
                            _ErrorMessage = String.Format("To role {0} not found.", contactPayload.torole);
                        }

                        if (!String.IsNullOrEmpty(contactPayload.torole) && ToEntityAccount == null)
                        {
                            //from role not found
                            ErrorCode = 400;
                            _ErrorMessage = String.Format("Primary role {0} not found.") ;
                        }

                        #endregion


                        if (RoleCountToCheck > 1 && _ErrorMessage != string.Empty)
                        {
                            if (contactPayload.fromrole != null && contactPayload.torole != null)
                            {

                                // check if reverse connection exists
                                if (CheckIfReverseConnectionExists(crmWorkflowContext, FromEntityRole, ToEntityRole))
                                {
                                   
                                   if ( CheckIfSameConnectionExists(FromEntityContact, ToEntityAccount, Guid.Parse(contactPayload.fromrole)
                                        , Guid.Parse(contactPayload.torole)))
                                        {
                                        //connection already exists
                                        ErrorCode = 412;
                                        _ErrorMessage = "Connection already exists";
                                    }

                                   else
                                    {
                                        //check if there is any other contact as a primary user for the same account
                                        
                                        if (!IsPrimaryUserExists(ToEntityAccount, PrimaryUserRole.Id))
                                        {
                                            //create primary connection

                                            CreateConnection(FromEntityContact, ToEntityAccount, FromEntityRole.Id);
                                        }

                                        ToConnectId =  CreateConnection(FromEntityContact, ToEntityAccount, FromEntityRole.Id, ToEntityRole.Id);
                                    }
                                }
                                else
                                {
                                    _ErrorMessage = String.Format("From role {0} and reverse role {1} combination doesn't exists.",
                                    contactPayload.fromrole, contactPayload.torole);
                                }

                            }

                            else
                            {
                                //single conneciton

                                if (CheckIfSameConnectionExists( FromEntityContact, ToEntityAccount, Guid.Parse(contactPayload.fromrole) ))
                                          
                                {
                                    //connection already exists
                                    ErrorCode = 412;
                                    _ErrorMessage = "Connection already exists";
                                }

                                else
                                {
                                    //check if there are any other contact as a primary user for the same account

                                    if (!IsPrimaryUserExists( ToEntityAccount, PrimaryUserRole.Id))
                                    {
                                        //create primary connection
                                        CreateConnection(FromEntityContact, ToEntityAccount, FromEntityRole.Id);
                                    }

                                    CreateConnection( FromEntityContact, ToEntityAccount, FromEntityRole.Id, ToEntityRole.Id);
                                }
                            }

                            


                        }
                        else
                        {
                            _ErrorMessage = "Mimimum 1 role is required.";
                        }
                        #endregion




                    }

                    {
                        if(!ContactExists & !AccountExists)
                        {
                            ErrorCode = 404;
                            _ErrorMessage = String.Format(@"Contact id {0} does not exists
                                and Account id{1} does not exists", contactPayload.contactid, contactPayload.accountid);
                        }
                        else
                        if (!ContactExists)
                        {
                            ErrorCode = 404;
                            _ErrorMessage = String.Format("Contact id {0} does not exists", contactPayload.contactid);
                        }
                        else if(!AccountExists)
                        {
                            ErrorCode = 404;
                            _ErrorMessage = String.Format("Account id {0} does not exists", contactPayload.contactid);
                        }
                    }
                }





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
                SCIIR.ConnectContactResponse responsePayload = new SCIIR.ConnectContactResponse()
                {
                    code = ErrorCode,
                    message = _ErrorMessage,
                    datetime = DateTime.UtcNow,
                    version = "1.0.0.2",
                    program = "Connect Contact",
                    status = ErrorCode == 200 || ErrorCode == 412 ? "success" : "failure",
                    data = new SCIIR.ConnectContactData()
                    {
                        fromconnectionid = ToConnectId.Value,
                       
                    }

                };

                string resPayload = JsonConvert.SerializeObject(responsePayload);
                ReturnMessageDetails.Set(executionContext, resPayload);
                localcontext.Trace("finally block end");

            } 
            #endregion

            
        }


        private Boolean CheckIfRecordExists( EntityReference Record)
        {
            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(localcontext.OrganizationService);
            var record = from c in orgSvcContext.CreateQuery(Record.LogicalName )
                         where c.Id == Record.Id && (int)c["status"]  == 0
                         select new { c.Id};

            return record.Count() > 0 ? true : false;
        }
        /// <summary>
        /// This method will return true if active connection role exists 
        /// </summary>
        /// <param name="LocalExecutionContext"></param>
        /// <param name="RoleNames"></param>
        /// <returns></returns>
        private List<Entity> GetRoles( List<String> RoleNames)
        {
            localcontext.Trace("inside get roles");

            var filter = new FilterExpression(LogicalOperator.And)
            {
                Conditions =
                    {
                        new ConditionExpression(SCS.Connection.CONNECTIONROLE, ConditionOperator.In, RoleNames.ToList<String>()),
                        new ConditionExpression(SCS.Connection.STATUS, ConditionOperator.Equal, 0)
                    }
            };
            var query = new QueryExpression(SCS.Connection.CONNECTIONROLE)
            {
                ColumnSet = new ColumnSet(SCS.Connection.CONNECTIONROLEID),
                Criteria = filter
            };
            var records = localcontext.OrganizationService.RetrieveMultiple(query).Entities;
            localcontext.Trace(String.Format("roles exists count value {}" ,records.Count));
            return records.ToList<Entity>();
        }


        /// <summary>
        /// This method will check if reveser connection exists
        /// </summary>
        /// <param name="LocalExecutionContext"></param>
        /// <param name="FromRole"></param>
        /// <param name="ToRole"></param>
        /// <returns></returns>
        private Boolean CheckIfReverseConnectionExists(LocalWorkflowContext LocalExecutionContext, EntityReference FromRole, EntityReference ToRole)
        {
            localcontext.Trace("inside checking reverse connection exists");

            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(localcontext.OrganizationService);
           
                var Connections = from role in orgSvcContext.CreateQuery(SCS.Connection.CONNECTIONROLE)
                                  join reverserole in orgSvcContext.CreateQuery(SCS.Connection.REVERSECONNECTIONENTITY)
                                  on role[SCS.Connection.ASSOCIATEDCONNECTIONROLEID] equals reverserole[SCS.Connection.CONNECTIONROLEID]
                                  where role.Id.Equals(FromRole.Id) && reverserole.Id.Equals(ToRole.Id)
                                  select new { RoleId = role[SCS.Connection.CONNECTIONROLEID] };
            localcontext.Trace("getting count of reverse conntecitons" );

            return Connections.Count() > 0 ? true : false;
        }

        private Boolean CheckIfSameConnectionExists( EntityReference FromEntity, EntityReference ToEntity
            , Guid? FromConnectionRoleId, Guid? ToConnectionRoleId = null)
        {

            Boolean returnVal = false;
            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(localcontext.OrganizationService);
            //checking if both from and to id provided
            if (FromConnectionRoleId.HasValue && ToConnectionRoleId.HasValue && ToConnectionRoleId.Value != Guid.Empty)
            {


                localcontext.Trace("checkign if both from and to ids are provided");
                var RelateConneciton = from connection in orgSvcContext.CreateQuery(SCS.Connection.CONNECTIONENTITY)
                                       where connection[SCS.Connection.RECORD1ID] == 
                                       FromEntity && connection[SCS.Connection.RECORD2ID] == ToEntity
                                       && (Guid)connection[SCS.Connection.RECORD1ROLEID] == FromConnectionRoleId
                                       && (Guid)connection[SCS.Connection.RECORD2ROLEID] == ToConnectionRoleId
                                       select new { ConnectionID = connection[SCS.Connection.CONNECTIONID] };
                localcontext.Trace(String.Format("record count: {0} " + RelateConneciton.Count()));

                returnVal = RelateConneciton.Count() > 0 ? true : false;
            }

            else
            {
                localcontext.Trace("checkign if both from id provided");

                var RelateConneciton = from connection in orgSvcContext.CreateQuery(SCS.Connection.CONNECTIONENTITY)
                                       where connection[SCS.Connection.RECORD1ID] == FromEntity && connection[SCS.Connection.RECORD2ID] == ToEntity
                                       && (Guid)connection[SCS.Connection.RECORD1ROLEID] == FromConnectionRoleId
                                       select new { ConnectionID = connection[SCS.Connection.CONNECTIONID] };
                localcontext.Trace(String.Format("record count: {0} " + RelateConneciton.Count()));

                returnVal = RelateConneciton.Count() > 0 ? true : false;

            }
            return returnVal;
        }


        private Boolean IsPrimaryUserExists(  EntityReference ToEntity, Guid PrimaryUserConnectionRoleId)
        {

            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(localcontext.OrganizationService);
            //get all the connection of an organisation
            var PrimaryUserConnection = from connection in orgSvcContext.CreateQuery(SCS.Connection.CONNECTIONENTITY)
                                   where
                                   (EntityReference)connection[SCS.Connection.RECORD2ID] == ToEntity &&
                                   (Guid)connection[SCS.Connection.RECORD2ROLEID] == PrimaryUserConnectionRoleId &&
                                   (int) connection[SCS.Connection.STATUS] == 0
                                   select new { ConnectionID = connection[SCS.Connection.CONNECTIONID] };
            localcontext.Trace(String.Format("record count: {0} " + PrimaryUserConnection.Count()));

            return  PrimaryUserConnection.Count() > 0 ? true : false;

        }

        private Guid CreateConnection( EntityReference FromEntity, EntityReference ToEntity
            , Guid? FromConnectionRoleId, Guid? ToConnectionRoleId = null)
        {
            Guid returnVal = Guid.Empty;
            Entity ConnectionToCreate;
            localcontext.Trace("inside create connectoin");


            if (FromConnectionRoleId.HasValue && FromConnectionRoleId.HasValue)
            {
                localcontext.Trace("two way connection");

                ConnectionToCreate = new Entity
                {
                    LogicalName = SCS.Connection.CONNECTIONENTITY,
                    [SCS.Connection.RECORD1ID] = FromEntity,
                    [SCS.Connection.RECORD2ID] = ToEntity,
                    [SCS.Connection.RECORD1ROLEID] = new EntityReference(SCS.Connection.CONNECTIONROLE,
                               FromConnectionRoleId.Value),
                    [SCS.Connection.RECORD2ROLEID] = new EntityReference("connectionrole",
                               ToConnectionRoleId.Value)
                };
                }
            else
            {
                ConnectionToCreate = new Entity
                {
                    LogicalName = SCS.Connection.CONNECTIONENTITY,
                    [SCS.Connection.RECORD1ID] = FromEntity,
                    [SCS.Connection.RECORD2ID] = ToEntity,
                    [SCS.Connection.RECORD1ROLEID] = new EntityReference(SCS.Connection.CONNECTIONROLE,
                               FromConnectionRoleId.Value)

                };
                localcontext.Trace("one way connection");

            }

            returnVal = localcontext.OrganizationService.Create(ConnectionToCreate);
            return returnVal;
        }

        
    }

}
