using System.Runtime.Serialization;

namespace Defra.CustMaster.Identity.WfActivities
{
    [DataContract]
    public  class ReqAccount
    {
        [DataMember]
        public string name;
        [DataMember]
        public int type;
        [DataMember]
        public string crn;
        [DataMember]
        public string email;
        [DataMember]
        public ReqAddress address;
        [DataMember]
        public string telephone;
        [DataMember]
        public string hierarchylevel;
        [DataMember]
        public string validatedwithcompanieshouse;
        [DataMember]
        public ReqParentOrganisation parentorganisation;
        
    }
}
