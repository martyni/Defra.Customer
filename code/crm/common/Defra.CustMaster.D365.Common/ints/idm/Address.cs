using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Defra.CustMaster.D365.Common.Ints.Idm
{
    public partial class Address
    {
        public int? type { get; set; }

        [DataMember]
        [MaxLength(20, ErrorMessage = "UPRN cannot be greater than 20")]
        [DataType(DataType.Text)]
        public string uprn { get; set; }

        [DataMember]
        [MaxLength(450, ErrorMessage = "Building Name cannot be greater than 450")]
        public string buildingname { get; set; }

        [DataMember]
        [MaxLength(20, ErrorMessage = "Building Number cannot be greater than 20")]
        public string buildingnumber { get; set; }

        [DataMember]
        [MaxLength(100, ErrorMessage = "Street cannot be greater than 100")]
        public string street { get; set; }

        [DataMember]
        [MaxLength(100, ErrorMessage = "Locality cannot be greater than 100")]
        public string locality { get; set; }

        [DataMember]
        [MaxLength(70, ErrorMessage = "Town cannot be greater than 70")]
        public string town { get; set; }

        [DataMember]
        [MaxLength(8, ErrorMessage = "Postcode cannot be greater than 8")]
        public string postcode { get; set; }

        [DataMember]       
        public string country { get; set; }

        [DataMember]   
        public string fromcompanieshouse { get; set; }
    }
}
