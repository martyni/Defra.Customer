using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Defra.CustMaster.Identity.Plugins.Security
{
    public class PreValidateLobServiceUserLinkCmSec : IPlugin
    {
        private static string _assemblyTypeName;
        private static ITracingService _tracingService;

        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        public void Execute(IServiceProvider serviceProvider)
        {

            _assemblyTypeName = this.GetType().AssemblyQualifiedName;
            _tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") &&
                 context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity targetEntity = (Entity)context.InputParameters["Target"];

                // Verify that the target entity represents an account.
                // If not, this plug-in was not registered correctly.
                if (targetEntity.LogicalName != "defra_lobserviceuserlink")
                    return;
                try
                {
                    if (targetEntity.Attributes.Contains("defra_servicerole") && targetEntity.Attributes["defra_servicerole"] != null)
                    {
                        EntityReference serviceRoleEntityRef = (EntityReference)targetEntity.Attributes["defra_servicerole"];
                        QueryExpression serviceQuery = new QueryExpression("defra_lobservice");
                        serviceQuery.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                        // take all columns
                        serviceQuery.ColumnSet = new ColumnSet(true);
                        // this is the intersect condition
                        LinkEntity serviceLink = new LinkEntity("defra_lobservice", "defra_lobserivcerole", "defra_lobserviceid", "defra_lobservice", JoinOperator.Inner);
                        // this is the condition to use the specific Team
                        ConditionExpression queryCondition = new ConditionExpression("defra_lobserivceroleid", ConditionOperator.Equal, serviceRoleEntityRef.Id);
                        ConditionExpression queryCondition1 = new ConditionExpression("statecode", ConditionOperator.Equal, 0);
                        // add the condition to the intersect
                        serviceLink.LinkCriteria.AddCondition(queryCondition);
                        serviceLink.LinkCriteria.AddCondition(queryCondition1);
                        // add the intersect to the query
                        serviceQuery.LinkEntities.Add(serviceLink);
                   
                        //get the results
                        EntityCollection retrievedServices = service.RetrieveMultiple(serviceQuery);
                        _tracingService.Trace("services record count" + retrievedServices.Entities.Count);
                        if (retrievedServices.Entities.Count > 0)
                        {
                            Entity serviceEntity = retrievedServices.Entities[0];
                            if (serviceEntity.Attributes.Contains("defra_ownerbu") && serviceEntity.Attributes["defra_ownerbu"] != null)
                            {
                                EntityReference serviceBusinessUnit = (EntityReference)serviceEntity.Attributes["defra_ownerbu"];
                                if (serviceBusinessUnit != null)
                                {
                                    string ownerBusinessUnitName = serviceBusinessUnit.Name;
                                    _tracingService.Trace("Service Business Unit Name" + ownerBusinessUnitName);
                                    string ownerTeam = ownerBusinessUnitName + "-CM-OWNER";
                                    _tracingService.Trace("Service Business Unit Owner team" + ownerTeam);
                                    QueryExpression teamQuery = new QueryExpression("team");
                                    teamQuery.ColumnSet = new ColumnSet(true);
                                    teamQuery.Criteria.AddCondition("name", ConditionOperator.Equal, ownerTeam);
                                 
                                    EntityCollection entities = service.RetrieveMultiple(teamQuery);
                                    Entity teamEntity = entities.Entities.Count > 0 ? entities[0] : null;

                                    if (teamEntity != null)
                                    {
                                        _tracingService.Trace("Enrloment Owner Team ID:" + teamEntity.Id);
                                        //Share the contact record
                                        if (targetEntity.Attributes.Contains("defra_serviceuser") && targetEntity.Attributes["defra_serviceuser"] != null)
                                        {
                                            EntityReference serviceUser = (EntityReference)targetEntity.Attributes["defra_serviceuser"];
                                            ShareRecord(teamEntity.Id, serviceUser);
                                        }

                                        //Assign the ownership
                                        SetRecordOwner(teamEntity.Id, targetEntity);

                                    }
                                }

                            }

                        }
                    }
                }

                // Catch any service fault exceptions that Microsoft Dynamics CRM throws.
                catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                {
                    // You can handle an exception here or pass it back to the calling method.
                    throw ex;
                }
            }
        }

        /// <summary>
        /// ShareRecord
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="targetEntity"></param>

        private void ShareRecord(Guid teamId, EntityReference targetEntity)
        {
            _tracingService.Trace("Sharing started with:" + teamId);
            _tracingService.Trace("Sharing started with:" + targetEntity.LogicalName + ":" + targetEntity.Id);
            //sharing
            var grantAccessRequest = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess | AccessRights.WriteAccess,
                    Principal = new EntityReference
                    {
                        LogicalName = "team",
                        Id = teamId//new Guid("C7BC381B-EC5E-E811-A86F-000D3AB17C7B")
                    }
                },
                Target = new EntityReference(targetEntity.LogicalName, targetEntity.Id)
            };
            service.Execute(grantAccessRequest);
            _tracingService.Trace("Shared the record:" + targetEntity.Id);

        }

        /// <summary>
        /// Set Record Owner
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="targetEntity"></param>
        private void SetRecordOwner(Guid teamId, Entity targetEntity)
        {
            _tracingService.Trace("Assign started with:" + teamId);
            if (targetEntity.Attributes.Contains("ownerid"))
            {
                _tracingService.Trace("Before assign the owner:" + ((EntityReference)targetEntity.Attributes["ownerid"]).Id);
                targetEntity.Attributes["ownerid"] = new EntityReference("team", teamId);
                _tracingService.Trace("after assign the owner:" + ((EntityReference)targetEntity.Attributes["ownerid"]).Id);
            }

            //// Assign the record to a team. i.e. owner               
            //AssignRequest assignRequest = new AssignRequest()
            //{
            //    Assignee = new EntityReference
            //    {
            //        LogicalName = "team",
            //        Id = teamId//new Guid("C7BC381B-EC5E-E811-A86F-000D3AB17C7B")
            //    },

            //    Target = new EntityReference(targetEntity.LogicalName, targetEntity.Id)
            //};
            //service.Execute(assignRequest);

            _tracingService.Trace("Assigned the record:" + targetEntity.Id);

        }
    }
}