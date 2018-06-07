using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Defra.Customer.Plugins.Model
{
    [DataContract]
    public class Contact
    {
        [DataMember]
        public string b2cobjectid;
        [DataMember]
        public Nullable<int> title;
        [DataMember]
        public string firstname;
        [DataMember]
        public string middlename;
        [DataMember]
        public string lastname;
        [DataMember]
        public string email;
        [DataMember]
        public string dob;
        [DataMember]
        public Nullable<int> gender;
        [DataMember]
        public string telephone;
        [DataMember]
        public string tacsacceptedversion;
        [DataMember]
        public string tacsacceptedon;
        [DataMember]
        public Address address;
    }
}
