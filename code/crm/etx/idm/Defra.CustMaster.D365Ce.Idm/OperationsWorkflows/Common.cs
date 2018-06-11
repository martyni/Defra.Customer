using Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Defra.CustMaster.D365Ce.Idm.OperationsWorkflows
{
    public class Common
    {
        public ITracingService tracingService;
        public IWorkflowContext context;
        public IOrganizationServiceFactory serviceFactory;
        public IOrganizationService service;

        public Common(CodeActivityContext executionContext)
        {
            tracingService = executionContext.GetExtension<ITracingService>();
            context = executionContext.GetExtension<IWorkflowContext>();
            serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            service = serviceFactory.CreateOrganizationService(context.UserId);
        }

        /// <summary>
        /// Query the Metadata to get the Entity Schema Name from the Object Type Code
        /// </summary>
        /// <param name="ObjectTypeCode"></param>
        /// <param name="service"></param>
        /// <returns>Entity Schema Name</returns>
        public string sGetEntityNameFromCode(string ObjectTypeCode, IOrganizationService service)
        {
            MetadataFilterExpression entityFilter = new MetadataFilterExpression(LogicalOperator.And);
            entityFilter.Conditions.Add(new MetadataConditionExpression("ObjectTypeCode", MetadataConditionOperator.Equals, Convert.ToInt32(ObjectTypeCode)));
            EntityQueryExpression entityQueryExpression = new EntityQueryExpression()
            {
                Criteria = entityFilter
            };
            RetrieveMetadataChangesRequest retrieveMetadataChangesRequest = new RetrieveMetadataChangesRequest()
            {
                Query = entityQueryExpression,
                ClientVersionStamp = null
            };
            RetrieveMetadataChangesResponse response = (RetrieveMetadataChangesResponse)service.Execute(retrieveMetadataChangesRequest);

            EntityMetadata entityMetadata = (EntityMetadata)response.EntityMetadata[0];
            return entityMetadata.SchemaName.ToLower();
        }


        public EntityCollection getAssociations(string PrimaryEntityName, Guid PrimaryEntityId, string _relationshipName, string entityName, string ParentId)
        {
            //
            string fetchXML = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                      <entity name='" + PrimaryEntityName + @"'>
                                        <link-entity name='" + _relationshipName + @"' from='" + PrimaryEntityName + @"id' to='" + PrimaryEntityName + @"id' visible='false' intersect='true'>
                                        <link-entity name='opportunity' from='opportunityid' to='opportunityid' alias='ab'>
                                            <filter type='and'>
                                            <condition attribute='opportunityid' operator='eq' value='" + PrimaryEntityId.ToString() + @"' />
                                            </filter>
                                        </link-entity> 
                                        <link-entity name='" + entityName + @"' from='" + entityName + @"id' to='" + entityName + @"id' alias='ac'>
                                                <filter type='and'>
                                                  <condition attribute='" + entityName + @"id' operator='eq' value='" + ParentId + @"' />
                                                </filter>
                                              </link-entity>
                                        </link-entity>
                                      </entity>
                                    </fetch>";
            tracingService.Trace(String.Format("FetchXML: {0} ", fetchXML));
            EntityCollection relations = service.RetrieveMultiple(new FetchExpression(fetchXML));

            return relations;
        }

        public List<string> getEntityAttributesToClone(string entityName, IOrganizationService service,
            ref string PrimaryIdAttribute, ref string PrimaryNameAttribute)
        {


            List<string> atts = new List<string>();
            RetrieveEntityRequest req = new RetrieveEntityRequest()
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = entityName
            };

            RetrieveEntityResponse res = (RetrieveEntityResponse)service.Execute(req);
            PrimaryIdAttribute = res.EntityMetadata.PrimaryIdAttribute;

            foreach (AttributeMetadata attMetadata in res.EntityMetadata.Attributes)
            {
                if (attMetadata.IsPrimaryName.Value)
                {
                    PrimaryNameAttribute = attMetadata.LogicalName;
                }
                if ((attMetadata.IsValidForCreate.Value || attMetadata.IsValidForUpdate.Value)
                    && !attMetadata.IsPrimaryId.Value)
                {
                    //tracingService.Trace("Tipo:{0}", attMetadata.AttributeTypeName.Value.ToLower());
                    if (attMetadata.AttributeTypeName.Value.ToLower() == "partylisttype")
                    {
                        atts.Add("partylist-" + attMetadata.LogicalName);
                        //atts.Add(attMetadata.LogicalName);
                    }
                    else
                    {
                        atts.Add(attMetadata.LogicalName);
                    }
                }
            }

            return (atts);
        }

        public Guid CloneRecord(string entityName, string objectId, string fieldstoIgnore, string prefix)
        {
            fieldstoIgnore = fieldstoIgnore.ToLower();
            Entity retrievedObject = service.Retrieve(entityName, new Guid(objectId), new ColumnSet(allColumns: true));
            tracingService.Trace("retrieved object OK");

            Entity newEntity = new Entity(entityName);
            string PrimaryIdAttribute = "";
            string PrimaryNameAttribute = "";
            List<string> atts = getEntityAttributesToClone(entityName, service, ref PrimaryIdAttribute, ref PrimaryNameAttribute);



            foreach (string att in atts)
            {
                if (fieldstoIgnore != null && fieldstoIgnore != "")
                {
                    if (Array.IndexOf(fieldstoIgnore.Split(';'), att) >= 0 || Array.IndexOf(fieldstoIgnore.Split(','), att) >= 0)
                    {
                        continue;
                    }
                }


                if (retrievedObject.Attributes.Contains(att) && att != "statuscode" && att != "statecode"
                    || att.StartsWith("partylist-"))
                {
                    if (att.StartsWith("partylist-"))
                    {
                        string att2 = att.Replace("partylist-", "");

                        string fetchParty = @"<fetch version='1.0' output-format='xml - platform' mapping='logical' distinct='true'>
                                                <entity name='activityparty'>
                                                    <attribute name = 'partyid'/>
                                                        <filter type = 'and' >
                                                            <condition attribute = 'activityid' operator= 'eq' value = '" + objectId + @"' />
                                                            <condition attribute = 'participationtypemask' operator= 'eq' value = '" + getParticipation(att2) + @"' />
                                                         </filter>
                                                </entity>
                                            </fetch> ";

                        RetrieveMultipleRequest fetchRequest1 = new RetrieveMultipleRequest
                        {
                            Query = new FetchExpression(fetchParty)
                        };
                        tracingService.Trace(fetchParty);
                        EntityCollection returnCollection = ((RetrieveMultipleResponse)service.Execute(fetchRequest1)).EntityCollection;


                        EntityCollection arrPartiesNew = new EntityCollection();
                        tracingService.Trace("attribute:{0}", att2);

                        foreach (Entity ent in returnCollection.Entities)
                        {
                            Entity party = new Entity("activityparty");
                            EntityReference partyid = (EntityReference)ent.Attributes["partyid"];


                            party.Attributes.Add("partyid", new EntityReference(partyid.LogicalName, partyid.Id));
                            tracingService.Trace("attribute:{0}:{1}:{2}", att2, partyid.LogicalName, partyid.Id.ToString());
                            arrPartiesNew.Entities.Add(party);
                        }

                        newEntity.Attributes.Add(att2, arrPartiesNew);
                        continue;

                    }

                    tracingService.Trace("attribute:{0}", att);
                    if (att == PrimaryNameAttribute && prefix != null)
                    {
                        retrievedObject.Attributes[att] = prefix + retrievedObject.Attributes[att];
                    }
                    newEntity.Attributes.Add(att, retrievedObject.Attributes[att]);
                }
            }

            tracingService.Trace("creating cloned object...");
            Guid createdGUID = service.Create(newEntity);
            tracingService.Trace("created cloned object OK");

            if (newEntity.Attributes.Contains("statuscode") && newEntity.Attributes.Contains("statecode"))
            {
                Entity record = service.Retrieve(entityName, createdGUID, new ColumnSet("statuscode", "statecode"));


                if (retrievedObject.Attributes["statuscode"] != record.Attributes["statuscode"] ||
                    retrievedObject.Attributes["statecode"] != record.Attributes["statecode"])
                {
                    Entity setStatusEnt = new Entity(entityName, createdGUID);
                    setStatusEnt.Attributes.Add("statuscode", retrievedObject.Attributes["statuscode"]);
                    setStatusEnt.Attributes.Add("statecode", retrievedObject.Attributes["statecode"]);

                    service.Update(setStatusEnt);
                }
            }

            tracingService.Trace("cloned object OK");
            return createdGUID;
        }


        protected string getParticipation(string attributeName)
        {
            string sReturn = "";
            switch (attributeName)
            {
                case "from":
                    sReturn = "1";
                    break;
                case "to":
                    sReturn = "2";
                    break;
                case "cc":
                    sReturn = "3";
                    break;
                case "bcc":
                    sReturn = "4";
                    break;

                case "organizer":
                    sReturn = "7";
                    break;
                case "requiredattendees":
                    sReturn = "5";
                    break;
                case "optionalattendees":
                    sReturn = "6";
                    break;
                case "customer":
                    sReturn = "11";
                    break;
                case "resources":
                    sReturn = "10";
                    break;
            }
            return sReturn;
            /*Sender  1
                Specifies the sender.

                ToRecipient
                2
                Specifies the recipient in the To field.

                CCRecipient
                3
                Specifies the recipient in the Cc field.

                BccRecipient
                4
                Specifies the recipient in the Bcc field.

                RequiredAttendee
                5
                Specifies a required attendee.

                OptionalAttendee
                6
                Specifies an optional attendee.

                Organizer
                7
                Specifies the activity organizer.

                Regarding
                8
                Specifies the regarding item.

                Owner
                9
                Specifies the activity owner.

                Resource
                10
                Specifies a resource.

                Customer
                11

            */
        }

        public void CreateAddress(Address addressDetails, EntityReference Customer)
        {
            Guid addressId = Guid.Empty;
            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(this.service);
            if (addressDetails.uprn != null)
            {
                var propertyWithUPRN = from c in orgSvcContext.CreateQuery("defra_address")
                                       where ((string)c["defra_uprn"]).Equals((addressDetails.uprn.Trim()))
                                       select new { AddressId = c.Id };
                addressId = propertyWithUPRN != null && propertyWithUPRN.FirstOrDefault() != null ? propertyWithUPRN.FirstOrDefault().AddressId : Guid.Empty;
            }
            if (addressId == Guid.Empty && addressDetails.street != null&&addressDetails.postcode!=null&&addressDetails.buildingnumber!=null)
            {
                var propertyWithDuplicate = from c in orgSvcContext.CreateQuery("defra_address")
                                       where ((string)c["defra_street"]).Equals((addressDetails.street.Trim())) && ((string)c["defra_postcode"]).Equals((addressDetails.postcode.Trim())) && ((string)c["defra_premises"]).Equals((addressDetails.buildingnumber.Trim()))
                                       select new { AddressId = c.Id };
                addressId = propertyWithDuplicate != null && propertyWithDuplicate.FirstOrDefault() != null ? propertyWithDuplicate.FirstOrDefault().AddressId : Guid.Empty;
            }
            if (addressId == Guid.Empty)
            {
                Entity address = new Entity("defra_address");
                if (addressDetails.uprn != null)
                    address["defra_uprn"] = addressDetails.uprn;
                if (addressDetails.buildingname != null)
                    address["defra_name"] = addressDetails.buildingname;
                if (addressDetails.buildingnumber != null)
                    address["defra_premises"] = addressDetails.buildingnumber;// + "," + addressDetails.buildingname;
                if (addressDetails.street != null)
                    address["defra_street"] = addressDetails.street;
                if (addressDetails.locality != null)
                    address["defra_locality"] = addressDetails.locality;
                if (addressDetails.town != null)
                    address["defra_towntext"] = addressDetails.town;
                if (addressDetails.postcode != null)
                    address["defra_postcode"] = addressDetails.postcode;
                bool resultCompanyHouse;
                if (addressDetails.fromcompanieshouse != null)
                    if (Boolean.TryParse(addressDetails.fromcompanieshouse, out resultCompanyHouse))
                        address["defra_fromcompanieshouse"] = resultCompanyHouse;
                if (addressDetails.county != null)
                {
                    var CountryRecord = from c in orgSvcContext.CreateQuery("defra_country")
                                        where ((string)c["defra_name"]).ToLower().Contains((addressDetails.county.Trim().ToLower()))
                                        select new { CountryId = c.Id };
                    Guid countryGuid = CountryRecord != null && CountryRecord.FirstOrDefault() != null ? CountryRecord.FirstOrDefault().CountryId : Guid.Empty;
                    if (countryGuid != Guid.Empty)
                        address["defra_country"] = new EntityReference("defra_country", countryGuid);
                }
                 addressId = this.service.Create(address);
            }
            if (addressId != Guid.Empty)
            {
                Entity contactDetails = new Entity("defra_addressdetails");
                contactDetails["defra_address"] = new EntityReference("defra_address", addressId);
                int resultAddressType;
                if (addressDetails.type != null)
                    if (int.TryParse(addressDetails.type, out resultAddressType))
                    {
                        contactDetails["defra_addresstype"] = new OptionSetValue(resultAddressType);
                    }

                contactDetails["defra_customer"] = Customer;
                Guid contactDetailId = this.service.Create(contactDetails);
            }

        }

    }
}
