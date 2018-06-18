using System;
using System.Runtime.Serialization;

namespace Defra.CustMaster.Identity.WfActivities
{
    [DataContract]
    public class ReqParentOrganisation
    {
        [DataMember]
        public String parentorganisationcrmid;
    }
}
