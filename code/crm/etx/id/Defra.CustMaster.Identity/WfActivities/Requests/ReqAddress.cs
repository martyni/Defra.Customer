using System.Runtime.Serialization;

namespace Defra.CustMaster.Identity.WfActivities
{
    [DataContract]
    public class ReqAddress
    {
        [DataMember]
        public string type;
        [DataMember]
        public string uprn;
        [DataMember]
        public string buildingnumber;
        [DataMember]
        public string buildingname;
        [DataMember]
        public string street;
        [DataMember]
        public string locality;
        [DataMember]
        public string town;
        [DataMember]
        public string postcode;
        [DataMember]
        public string county;
        [DataMember]
        public string fromcompanieshouse;
    }
}
