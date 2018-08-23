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
    public class CreateRelationship : WorkFlowActivityBase
    {
        #region "Parameter Definition"
        [RequiredArgument]
        [Input("request")]
        public InArgument<String> request { get; set; }
        [Output("response")]
        public OutArgument<string> response { get; set; }

        #endregion


        LocalWorkflowContext localcontext = null;

        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            localcontext = crmWorkflowContext;
            String PayloadDetails = request.Get(executionContext);
            int ErrorCode = 400; //400 -- bad request
            int RoleCountToCheck = 0;
            String _ErrorMessage = string.Empty;
            String _ErrorMessageDetail = string.Empty;
            Guid ContactId = Guid.Empty;
            Guid AccountId = Guid.Empty;
            StringBuilder ErrorMessage = new StringBuilder();
            String UniqueReference = string.Empty;
            Guid? ToConnectId = Guid.Empty;
            Guid? ConnectionDetailsId = Guid.Empty;
            SCII.Helper objCommon = new SCII.Helper(executionContext);
            try
            {

                localcontext.Trace("before seriallising1");
                SCII.CreateRelationshipRequest ConnectContact = JsonConvert.DeserializeObject<SCII.CreateRelationshipRequest>(PayloadDetails);
                localcontext.Trace("after seriallising");

                EntityReference FromEntityContact = null;
                EntityReference ToEntityAccount;

                var ValidationContext = new ValidationContext(ConnectContact, serviceProvider: null, items: null);
                ICollection<ValidationResult> ValidationResultsConnectContact = null;
                ICollection<ValidationResult> ValidationResultsRoles= null;
                localcontext.Trace("before validating");

                Boolean isValid = objCommon.Validate(ConnectContact, out ValidationResultsConnectContact);
                Boolean isValidRoles = objCommon.Validate(ConnectContact.relations, out ValidationResultsRoles);


                string FromEntityName = ConnectContact.fromrecordtype == SCII.RecordTypeName.contact ? SCS.Contact.ENTITY : SCS.AccountContants.ENTITY_NAME;
                string ToEntityName = ConnectContact.torecordtype == SCII.RecordTypeName.contact ? SCS.Contact.ENTITY : SCS.AccountContants.ENTITY_NAME;

                if (isValid && isValidRoles)
                {
                    //get role ids
                    localcontext.Trace("inside valid schema");

                    localcontext.Trace(string.Format("contact id {0} org id {1}.", ConnectContact.fromrecordid, ConnectContact.torecordid ));

                    #region Validate record exists
                    //check if account record exists
                    Boolean ContactExists = false;
                    if (!String.IsNullOrEmpty(ConnectContact.fromrecordid))
                    {
                        FromEntityContact = new EntityReference(FromEntityName, Guid.Parse(ConnectContact.fromrecordid));
                        ContactExists = CheckIfRecordExists(FromEntityContact);
                    }
                    ToEntityAccount = new EntityReference(ToEntityName, Guid.Parse(ConnectContact.torecordid));
                    localcontext.Trace("before validating details new");

                    Boolean AccountExists = CheckIfRecordExists(ToEntityAccount);
                    localcontext.Trace("after validating");

                    if (ContactExists && AccountExists)
                    {
                        #region Getting connection role IDs

                        List<String> RoleNames = new List<string>();
                        if (ConnectContact.relations.fromrole != null)
                        {
                            RoleNames.Add(ConnectContact.relations.fromrole);
                            RoleCountToCheck = RoleCountToCheck + 1;
                        }
                        if (ConnectContact.relations.torole != null)
                        {
                            RoleNames.Add(ConnectContact.relations.torole);
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
                            if (ConnectContact.relations.fromrole == (string)ConnectionRoles[SCS.Connection.NAME])
                            {
                                localcontext.Trace("received from role id");
                                FromEntityRole = new EntityReference(ConnectionRoles.LogicalName, ConnectionRoles.Id);
                            }

                            if (ConnectContact.relations.torole == (string)ConnectionRoles[SCS.Connection.NAME])
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

                        if(!String.IsNullOrEmpty(ConnectContact.relations.fromrole) && FromEntityRole == null)
                        {
                            //from role not found
                            ErrorCode = 404;
                            _ErrorMessage = String.Format( "From role {0} not found.", ConnectContact.relations.fromrole);
                        }

                        if (ToEntityRole == null)
                        {
                            //to role not found
                            ErrorCode = 404;
                            _ErrorMessage = String.Format("To role {0} not found.", ConnectContact.relations.torole);
                        }

                        if (PrimaryUserRole == null)
                        {
                            //primary role not found
                            ErrorCode = 404;
                            _ErrorMessage = String.Format("Primary rolenot found.");
                        }

                        #endregion

                        localcontext.Trace("after performing common chencks");
                        localcontext.Trace("error" + _ErrorMessage);
                        localcontext.Trace("roles count" + RoleCountToCheck);

                        Guid? ToEntityRoleID = ToEntityRole == null ? Guid.Empty : ToEntityRole.Id;

                        if (_ErrorMessage == string.Empty)
                        {
                            if (FromEntityRole != null && ToEntityRole!= null)
                            {
                                localcontext.Trace("case when both role ids present");

                                // check if reverse connection exists
                                if (CheckIfReverseConnectionRoleExists( FromEntityRole, ToEntityRole))
                                {
                                    localcontext.Trace("checking for reverse connection");
                                    if ( IsSameConnectionExists(FromEntityContact, ToEntityAccount,FromEntityRole.Id
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
                                        if (!CheckifSingleRoleAlreadyExists(ToEntityAccount, PrimaryUserRole.Id))
                                        {
                                            //create primary connection
                                            localcontext.Trace("before creating primary connection");
                                            CreateSingleConnection(FromEntityContact, ToEntityAccount, PrimaryUserRole.Id);
                                        }

                                        if (FromEntityRole != null && ToEntityRoleID != null)
                                        {
                                            ToConnectId = CreateDoubleConnection(FromEntityContact, ToEntityAccount, FromEntityRole.Id, ToEntityRole.Id);
                                        }
                                        else if(FromEntityRole == null && ToEntityRole != null)
                                        {
                                            ToConnectId = CreateSingleConnection(FromEntityContact, ToEntityAccount,  ToEntityRole.Id);

                                        }
                                        else if(FromEntityRole != null && ToEntityRole == null)
                                        {
                                            ToConnectId = CreateSingleConnection(FromEntityContact, ToEntityAccount, FromEntityRole.Id);

                                        }
                                        ErrorCode = 200;

                                    }
                                }
                                else
                                {
                                    _ErrorMessage = String.Format("From role {0} and reverse role {1} combination doesn't exists.",
                                    ConnectContact.relations.fromrole, ConnectContact.relations.torole);
                                }

                            }

                            else
                            {
                                //single conneciton
                                localcontext.Trace("checking if single connection exists");

                                if (CheckifSingleRoleAlreadyExists(  ToEntityAccount, ToEntityRole.Id))
                                          
                                {
                                    //connection already
                                    ErrorCode = 412;
                                    _ErrorMessage = "Connection already exists";
                                }

                                else
                                {
                                    //check if there are any other contact as a primary user for the same account

                                    if (!CheckifSingleRoleAlreadyExists(ToEntityAccount, PrimaryUserRole.Id))
                                    {
                                        //create primary connection
                                        CreateSingleConnection(FromEntityContact, ToEntityAccount, PrimaryUserRole.Id);
                                    }

                                    ToConnectId = CreateSingleConnection( FromEntityContact, ToEntityAccount,  ToEntityRole.Id);
                                    ErrorCode = 200;
                                }
                            }
                        }
                        
                        #endregion

                    }

                    {
                        if(!ContactExists & !AccountExists)
                        {
                            ErrorCode = 404;
                            _ErrorMessage = String.Format(@"Contact id {0} does not exists
                                and Account id{1} does not exists", ConnectContact.fromrecordid, ConnectContact.torecordid);
                        }
                        else
                        if (!ContactExists)
                        {
                            ErrorCode = 404;
                            _ErrorMessage = String.Format("Contact id {0} does not exists", ConnectContact.fromrecordid);
                        }
                        else if(!AccountExists)
                        {
                            ErrorCode = 404;
                            _ErrorMessage = String.Format("Account id {0} does not exists", ConnectContact.torecordid);
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
                    if (ConnectContact.relations != null)
                        foreach (ValidationResult vr in ValidationResultsRoles)
                        {
                            ErrorMessage.Append(vr.ErrorMessage + " ");
                        }
                    ErrorCode = 400;
                    _ErrorMessage = ErrorMessage.ToString();
                }
            }

            #region Catch Exception
            catch (Exception ex)
            {
                crmWorkflowContext.Trace("inside catch");
                crmWorkflowContext.Trace("exception message: " + ex.Message);

                ErrorCode = 500;
                _ErrorMessage = ex.Message;
                _ErrorMessageDetail = ex.Message;
                // crmWorkflowContext.Trace(String.Format("message details {0}", ex.Message));
                //_ErrorMessageDetail = ex.Message ;
                ErrorCode = 400;
                this.response.Set(executionContext, _ErrorMessageDetail);
                //throw ex;
            }
            #endregion

            #region Finally Block
            finally
            {
                if (ToConnectId.HasValue && !ToConnectId.Value.Equals(Guid.Empty))
                {
                    localcontext.Trace("started retreiving connection detailsid");
                    Entity connectionEntity = localcontext.OrganizationService.Retrieve(SCS.Connection.CONNECTIONENTITY, new Guid(ToConnectId.ToString()), new ColumnSet("defra_connectiondetailsid"));
                    EntityReference connectionDetails = (EntityReference)connectionEntity.Attributes["defra_connectiondetailsid"];
                    ConnectionDetailsId = connectionDetails.Id;
                    localcontext.Trace("started retreiving connection detailsid:" + ConnectionDetailsId);

                }

                SCIIR.ConnectContactResponse responsePayload = new SCIIR.ConnectContactResponse()
                {
                    code = ErrorCode,
                    message = _ErrorMessage,
                    datetime = DateTime.UtcNow,
                    version = "1.0.0.2",
                    program = "Create relatioship",
                    status = ErrorCode == 200 || ErrorCode == 412 ? "success" : "failure",
                    data = new SCIIR.ConnectContactData()
                    {
                        connectionid = ToConnectId.HasValue ? ToConnectId.Value.ToString() : string.Empty,
                        connectiondetailsid = ConnectionDetailsId.HasValue ? ConnectionDetailsId.Value.ToString() : string.Empty                        
                       
                    }

                };

                string resPayload = JsonConvert.SerializeObject(responsePayload);
                response.Set(executionContext, resPayload);
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

        private Boolean IsSameConnectionExists( EntityReference FromEntity, EntityReference ToEntity
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
                                       && (Guid)connection[SCS.Connection.RECORD2ROLEID] == ToConnectionRoleId.Value
                                       && (Guid)connection[SCS.Connection.RECORD1ROLEID] == FromConnectionRoleId.Value
                                       select new
                                       {
                                           ConnectionID = connection[SCS.Connection.CONNECTIONID]
                                       };
                localcontext.Trace(String.Format("from role id {0} to role id {1} " , FromConnectionRoleId.Value, ToConnectionRoleId.Value));

                localcontext.Trace("both ids provided count: " + RelateConneciton.ToList().Count);

                returnVal = RelateConneciton.ToList().Count > 0 ? true : false;
            }

            else
            {
                localcontext.Trace("checkign if from id provided");

                var RelateConneciton = from connection in orgSvcContext.CreateQuery(SCS.Connection.CONNECTIONENTITY)
                                       where (Guid)connection[SCS.Connection.RECORD1ID] ==
                                       FromEntity.Id && (Guid)connection[SCS.Connection.RECORD2ID] == ToEntity.Id
                                       && (Guid)connection[SCS.Connection.RECORD1ROLEID] == FromConnectionRoleId.Value
                                       select new
                                       {
                                           ConnectionID = connection[SCS.Connection.CONNECTIONID]
                                       };

                localcontext.Trace(String.Format("from role id {0}", FromConnectionRoleId.Value));

                localcontext.Trace(String.Format("record count: {0} " + RelateConneciton.ToList().Count()));

                returnVal = RelateConneciton.ToList().Count > 0 ? true : false;

            }
            return returnVal;
        }


        private Boolean CheckifSingleRoleAlreadyExists(  EntityReference ToEntity, Guid PrimaryUserConnectionRoleId)
        {

            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(localcontext.OrganizationService);
            //get all the connection of an organisation
            var PrimaryUserConnection = from connection in orgSvcContext.CreateQuery(SCS.Connection.CONNECTIONENTITY)
                                   where
                                   (Guid)connection[SCS.Connection.RECORD2ID] == ToEntity.Id &&
                                   (Guid)connection[SCS.Connection.RECORD2ROLEID] == PrimaryUserConnectionRoleId &&
                                   (int) connection[SCS.Connection.STATECODE] == 0
                                   select new { ConnectionID = connection[SCS.Connection.CONNECTIONID] };


           localcontext.Trace(String.Format("primary user role id {0} " , PrimaryUserConnectionRoleId));

            return  PrimaryUserConnection.ToList().Count > 0 ? true : false;

        }


        private Guid CreateSingleConnection(EntityReference FromEntity, EntityReference ToEntity
            ,  Guid? ToConnectionRoleId)
        {
            Guid returnVal;
            Entity ConnectionToCreate = new Entity
            {
                LogicalName = SCS.Connection.CONNECTIONENTITY,
                [SCS.Connection.RECORD1ID] = FromEntity,
                [SCS.Connection.RECORD2ID] = ToEntity,
                [SCS.Connection.RECORD2ROLEID] = new EntityReference(SCS.Connection.CONNECTIONROLE,
                             ToConnectionRoleId.Value)

            };
            localcontext.Trace("one single connection connection");
            returnVal = localcontext.OrganizationService.Create(ConnectionToCreate);
            return returnVal;
        }

        private Guid CreateDoubleConnection( EntityReference FromEntity, EntityReference ToEntity
            , Guid? FromConnectionRoleId, Guid? ToConnectionRoleId )
        {
            Guid returnVal = Guid.Empty;
            Entity ConnectionToCreate;
            localcontext.Trace("inside create connectoin");


            if (FromConnectionRoleId.HasValue && ToConnectionRoleId.HasValue)
            {
                localcontext.Trace("two way connection");

                localcontext.Trace("from connection role id " + FromConnectionRoleId.Value);
               // localcontext.Trace("to connection role id " + ToConnectionRoleId.Value);


                ConnectionToCreate = new Entity
                {
                    LogicalName = SCS.Connection.CONNECTIONENTITY,
                    [SCS.Connection.RECORD1ID] = FromEntity,
                    [SCS.Connection.RECORD2ID] = ToEntity,
                    [SCS.Connection.RECORD1ROLEID] = new EntityReference(SCS.Connection.CONNECTIONROLE,
                               FromConnectionRoleId.Value),
                    [SCS.Connection.RECORD2ROLEID] = new EntityReference(SCS.Connection.CONNECTIONROLE,
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
