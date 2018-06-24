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

                localcontext.Trace("before seriallising1");
                SCII.ConnectContactRequest contactPayload = JsonConvert.DeserializeObject<SCII.ConnectContactRequest>(PayloadDetails);
                localcontext.Trace("after seriallising");

                EntityReference FromEntityContact;
                EntityReference ToEntityAccount;

                var ValidationContext = new ValidationContext(contactPayload, serviceProvider: null, items: null);
                ICollection<ValidationResult> ValidationResultsConnectContact = null;
                ICollection<ValidationResult> ValidationResultsRoles= null;
                localcontext.Trace("before validating");

                Boolean isValid = objCommon.Validate(contactPayload, out ValidationResultsConnectContact);
                Boolean isValidRoles = objCommon.Validate(contactPayload.relations, out ValidationResultsRoles);


                if (isValid && isValidRoles)
                {
                    //get role ids
                    localcontext.Trace("inside valid schema");

                    localcontext.Trace(string.Format("contact id {0} org id {1}.", contactPayload.contactid, contactPayload.organisationid ));

                    #region Validate record exists
                    //check if account record exists
                    FromEntityContact = new EntityReference(SCS.Contact.ENTITY, Guid.Parse(contactPayload.contactid));
                    ToEntityAccount = new EntityReference(SCS.AccountContants.ENTITY_NAME, Guid.Parse(contactPayload.organisationid));
                    localcontext.Trace("before validating details new");

                    Boolean ContactExists = CheckIfRecordExists(FromEntityContact);
                    Boolean AccountExists = CheckIfRecordExists(ToEntityAccount);
                    localcontext.Trace("after validating");

                    if (ContactExists && AccountExists)
                    {
                        #region Getting connection role IDs

                        List<String> RoleNames = new List<string>();
                        if (contactPayload.relations.fromrole != null)
                        {
                            RoleNames.Add(contactPayload.relations.fromrole);
                            RoleCountToCheck = RoleCountToCheck + 1;
                        }
                        if (contactPayload.relations.torole != null)
                        {
                            RoleNames.Add(contactPayload.relations.torole);
                            RoleCountToCheck = RoleCountToCheck + 1;

                        }
                        RoleNames.Add(SCS.Connection.PRIMARYUSERROLENAME);
                        localcontext.Trace("before getting role name");

                        List<Entity> RolesList = GetRoles(RoleNames);
                        localcontext.Trace("after getting role name");

                        EntityReference FromEntityRole = null;
                        EntityReference ToEntityRole = null;
                        EntityReference PrimaryUserRole = null;

                        foreach (Entity ConnectionRoles in RolesList)
                        {
                            if (contactPayload.relations.fromrole == (string)ConnectionRoles[SCS.Connection.NAME])
                            {
                                localcontext.Trace("received from role id");

                                FromEntityRole = new EntityReference(ConnectionRoles.LogicalName, ConnectionRoles.Id);
                            }

                            if (contactPayload.relations.torole == (string)ConnectionRoles[SCS.Connection.NAME])
                            {
                                localcontext.Trace("received to role id");

                                ToEntityRole = new EntityReference(ConnectionRoles.LogicalName, ConnectionRoles.Id);
                            }

                            if (SCS.Connection.PRIMARYUSERROLENAME == (string)ConnectionRoles[SCS.Connection.NAME])
                            {
                                localcontext.Trace("received to primary role id");

                                PrimaryUserRole = new EntityReference(ConnectionRoles.LogicalName, ConnectionRoles.Id);
                            }
                        } 

                        if(!String.IsNullOrEmpty(contactPayload.relations.fromrole) && FromEntityRole == null)
                        {
                            //from role not found
                            ErrorCode = 404;
                            _ErrorMessage = String.Format( "From role {0} not found.", contactPayload.relations.fromrole);
                        }
                        

                        if (!String.IsNullOrEmpty(contactPayload.relations.torole) && ToEntityAccount == null)
                        {
                            //to role not found
                            ErrorCode = 404;
                            _ErrorMessage = String.Format("To role {0} not found.", contactPayload.relations.torole);
                        }

                        if (!String.IsNullOrEmpty(contactPayload.relations.torole) && ToEntityAccount == null)
                        {
                            //primary role not found
                            ErrorCode = 404;
                            _ErrorMessage = String.Format("Primary role {0} not found.") ;
                        }

                        #endregion

                        localcontext.Trace("after performing common chencks");
                        localcontext.Trace("error" + _ErrorMessage);
                        localcontext.Trace("roles count" + RoleCountToCheck);

                        Guid? ToEntityRoleID = ToEntityRole == null ? Guid.Empty : ToEntityRole.Id;

                        if (RoleCountToCheck > 1 && _ErrorMessage == string.Empty)
                        {
                            if (contactPayload.relations.fromrole != null && contactPayload.relations.torole != null)
                            {
                                localcontext.Trace("case when both role ids present");

                                // check if reverse connection exists
                                if (CheckIfReverseConnectionRoleExists( FromEntityRole, ToEntityRole))
                                {
                                    localcontext.Trace("checking for reverse connection");
                                    if ( CheckIfSameConnectionExists(FromEntityContact, ToEntityAccount,FromEntityRole.Id
                                        , ToEntityRole.Id))
                                        {
                                        //connection already exists
                                        ErrorCode = 412;
                                        _ErrorMessage = "Connection already exists";
                                    }

                                   else
                                    {
                                        //check if there is any other contact as a primary user for the same account
                                localcontext.Trace("before primary check");

                                        if (!IsPrimaryUserExists(ToEntityAccount, PrimaryUserRole.Id))
                                        {
                                            //create primary connection
                                            localcontext.Trace("before creating primary connection");
                                            CreateConnection(FromEntityContact, ToEntityAccount, FromEntityRole.Id);
                                        }

                                        ToConnectId =  CreateConnection(FromEntityContact, ToEntityAccount, FromEntityRole.Id, ToEntityRole.Id);
                                        ErrorCode = 200;

                                    }
                                }
                                else
                                {
                                    _ErrorMessage = String.Format("From role {0} and reverse role {1} combination doesn't exists.",
                                    contactPayload.relations.fromrole, contactPayload.relations.torole);
                                }

                            }

                            else
                            {
                                //single conneciton
                                localcontext.Trace("single connection check");

                                if (CheckIfSameConnectionExists( FromEntityContact, ToEntityAccount, Guid.Parse(contactPayload.relations.fromrole) ))
                                          
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

                                    ToConnectId = CreateConnection( FromEntityContact, ToEntityAccount, FromEntityRole.Id, ToEntityRole.Id);
                                    ErrorCode = 200;
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
                                and Account id{1} does not exists", contactPayload.contactid, contactPayload.organisationid);
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
                            _ErrorMessage = String.Format("Account id {0} does not exists", contactPayload.organisationid);
                        }
                    }
                }

                else
                {
                    localcontext.Trace("inside validation result");


                    //this will throw an error
                    foreach (ValidationResult vr in ValidationResultsConnectContact)
                    {
                        ErrorMessage.Append(vr.ErrorMessage + " ");
                    }
                    if (contactPayload.relations != null)
                        foreach (ValidationResult vr in ValidationResultsRoles)
                        {
                            ErrorMessage.Append(vr.ErrorMessage + " ");
                        }
                    ErrorCode = 400;
                    _ErrorMessage = ErrorMessage.ToString();
                }
                localcontext.Trace("CreateContact activity:setting output params like error code etc.. started");
                localcontext.Trace("CreateContact activity:setting output params like error code etc.. ended");
            }

            #region Catch Exception
            catch (Exception ex)
            {
                crmWorkflowContext.Trace("inside catch");
                ErrorCode = 500;
                _ErrorMessage = "Error occured while processing request";

                crmWorkflowContext.Trace(String.Format("message details {0}", ex.Message));
                _ErrorMessageDetail = ex.Message;
                ErrorCode = 400;
                this.ReturnMessageDetails.Set(executionContext, _ErrorMessageDetail);
                throw ex;
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
                        connectionid = ToConnectId.HasValue ? ToConnectId.Value.ToString() : String.Empty,
                       
                    }

                };

                string resPayload = JsonConvert.SerializeObject(responsePayload);
                ReturnMessageDetails.Set(executionContext, resPayload);
                localcontext.Trace("finally block end");

            } 
            #endregion

            
        }

        /// <summary>
        /// this method will check if record exists or not
        /// </summary>
        /// <param name="Record"></param>
        /// <returns></returns>
        private Boolean CheckIfRecordExists( EntityReference Record)
        {
            localcontext.Trace("entity logical name " + Record.LogicalName);
            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(localcontext.OrganizationService);
            var record = from c in orgSvcContext.CreateQuery(Record.LogicalName )
                         where (Guid)c[Record.LogicalName + "id"] == Record.Id && (int)c[SCS.Connection.STATECODE] == 0
                         select new { c.Id};

            return record.ToList().Count() > 0 ? true : false;
        }
        /// <summary>
        /// This method will return list of active connection roles
        /// </summary>
        /// <param name="LocalExecutionContext"></param>
        /// <param name="RoleNames"></param>
        /// <returns></returns>
        private List<Entity> GetRoles( List<String> RoleNames)
        {
            localcontext.Trace("inside get roles");

            FilterExpression filter = new FilterExpression(LogicalOperator.And)
            {
                Conditions =
                    {
                        new ConditionExpression(SCS.Connection.STATECODE, ConditionOperator.Equal, 0),
                        new ConditionExpression(SCS.Connection.NAME, ConditionOperator.In, RoleNames.ToArray()),
                    }
            };
            //FilterExpression Rolefilter = new FilterExpression(LogicalOperator.Or);


            //foreach (String RoleName in RoleNames)
            //{
            //    Rolefilter.Conditions.Add(new ConditionExpression(SCS.Connection.NAME, ConditionOperator.Like, RoleName));
            //}

            QueryExpression query = new QueryExpression(SCS.Connection.CONNECTIONROLE)
            {
                ColumnSet = new ColumnSet(SCS.Connection.CONNECTIONROLEID, SCS.Connection.NAME),
                Criteria = filter 
                
            };

           // query.Criteria.Filters.Add(Rolefilter);
            DataCollection<Entity> records = localcontext.OrganizationService.RetrieveMultiple(query).Entities;
            //localcontext.Trace(String.Format("roles exists count value {}" ,records.Count));
            return records.ToList<Entity>();
        }


        /// <summary>
        /// This method will check if reveser connection exists
        /// </summary>
        /// <param name="LocalExecutionContext"></param>
        /// <param name="FromRole"></param>
        /// <param name="ToRole"></param>
        /// <returns></returns>
        private Boolean CheckIfReverseConnectionRoleExists( EntityReference FromRole, EntityReference ToRole)
        {
            localcontext.Trace("inside checking reverse connection exists");

            localcontext.Trace("from role id:" + FromRole.Id);
            localcontext.Trace("to role id:" + ToRole.Id);


            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(localcontext.OrganizationService);
           
                var Connections = from role in orgSvcContext.CreateQuery(SCS.Connection.CONNECTIONROLE)
                                  join reverserole in orgSvcContext.CreateQuery(SCS.Connection.CONNECTIONROLEASSOCIATION)
                                  on role[SCS.Connection.CONNECTIONROLEID] equals reverserole[SCS.Connection.ASSOCIATEDCONNECTIONROLEID]
                                  where role[SCS.Connection.CONNECTIONROLEID].Equals(FromRole.Id) 
                                  && reverserole[SCS.Connection.CONNECTIONROLEID].Equals(ToRole.Id)
                                  select new { RoleId = role[SCS.Connection.CONNECTIONROLEID] };

           // from role in orgSvcContext.CreateQuery(SCS.Connection.CONNECTIONROLE)
           // join reverserole in orgSvcContext.CreateQuery("connectionroleassociation")
           // on role[SCS.Connection.CONNECTIONROLEID] equals reverserole["associatedconnectionroleid"]
           // where role[SCS.Connection.CONNECTIONROLEID].Equals(new Guid("caaf4df7-0229-e811-a831-000d3a2b29f8"))
           //&& reverserole[SCS.Connection.CONNECTIONROLEID].Equals(new Guid("776e1b5a-1268-e811-a83b-000d3ab4f7af"))
           // select new { RoleId = role[SCS.Connection.CONNECTIONROLEID] };

            localcontext.Trace("getting count of reverse conntecitons" );

            return Connections.ToList().Count > 0 ? true : false;


        }

        private Boolean CheckIfSameConnectionExists( EntityReference FromEntity, EntityReference ToEntity
            , Guid? FromConnectionRoleId, Guid? ToConnectionRoleId = null)
        {

            Boolean returnVal = false;
            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(localcontext.OrganizationService);
            //checking if both from and to id provided
            if (FromConnectionRoleId.HasValue && ToConnectionRoleId.HasValue && ToConnectionRoleId.Value != Guid.Empty)
            {
                localcontext.Trace("checking if both from and to ids are provided");
                var RelateConneciton = from connection in orgSvcContext.CreateQuery(SCS.Connection.CONNECTIONENTITY)
                                       where (Guid)connection[SCS.Connection.RECORD1ID] ==
                                       FromEntity.Id && (Guid)connection[SCS.Connection.RECORD2ID] == ToEntity.Id
                                       && (Guid)connection[SCS.Connection.RECORD2ROLEID] == FromConnectionRoleId
                                       && (Guid)connection[SCS.Connection.RECORD1ROLEID] == ToConnectionRoleId
                                       select new
                                       {
                                           ConnectionID = connection[SCS.Connection.CONNECTIONID]
                                       };


                returnVal = RelateConneciton.ToList().Count > 0 ? true : false;
            }

            else
            {
                localcontext.Trace("checkign if from id provided");

                var RelateConneciton = from connection in orgSvcContext.CreateQuery(SCS.Connection.CONNECTIONENTITY)
                                       where (Guid)connection[SCS.Connection.RECORD1ID] == FromEntity.Id && connection[SCS.Connection.RECORD2ID] == ToEntity
                                       && (Guid)connection[SCS.Connection.RECORD2ROLEID] == FromConnectionRoleId
                                       select new { ConnectionID = connection[SCS.Connection.CONNECTIONID] };
                localcontext.Trace(String.Format("record count: {0} " + RelateConneciton.Count()));

                returnVal = RelateConneciton.ToList().Count > 0 ? true : false;

            }
            return returnVal;
        }


        private Boolean IsPrimaryUserExists(  EntityReference ToEntity, Guid PrimaryUserConnectionRoleId)
        {

            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(localcontext.OrganizationService);
            //get all the connection of an organisation
            var PrimaryUserConnection = from connection in orgSvcContext.CreateQuery(SCS.Connection.CONNECTIONENTITY)
                                   where
                                   (Guid)connection[SCS.Connection.RECORD2ID] == ToEntity.Id &&
                                   (Guid)connection[SCS.Connection.RECORD2ROLEID] == PrimaryUserConnectionRoleId &&
                                   (int) connection[SCS.Connection.STATECODE] == 0
                                   select new { ConnectionID = connection[SCS.Connection.CONNECTIONID] };
           // localcontext.Trace(String.Format("record count: {0} " + PrimaryUserConnection.Count()));

            return  PrimaryUserConnection.ToList().Count > 0 ? true : false;

        }

        private Guid CreateConnection( EntityReference FromEntity, EntityReference ToEntity
            , Guid? FromConnectionRoleId, Guid? ToConnectionRoleId = null)
        {
            Guid returnVal = Guid.Empty;
            Entity ConnectionToCreate;
            localcontext.Trace("inside create connectoin");


            if (FromConnectionRoleId.HasValue && ToConnectionRoleId.HasValue)
            {
                localcontext.Trace("two way connection");

                localcontext.Trace("from connection role id " + FromConnectionRoleId.Value);
                localcontext.Trace("to connection role id " + ToConnectionRoleId.Value);


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
