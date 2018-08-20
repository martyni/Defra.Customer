using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Defra.CustMaster.D365.Common.Ints.Idm
{
    public partial class Address
    {

        [DataMember]
        [Required(ErrorMessage = "type can not be empty")]
        public int? type { get; set; }

        [DataMember]
        [MaxLength(20, ErrorMessage = "UPRN cannot be greater than 20;")]
        [DataType(DataType.Text)]
        public string uprn { get; set; }

        [DataMember]
        [MaxLength(450, ErrorMessage = "Building Name cannot be greater than 450;")]
        public string buildingname { get; set; }

        [DataMember]
        public string subbuildingname { get; set; }

        [DataMember]
        [MaxLength(20, ErrorMessage = "Building Number cannot be greater than 20;")]
        public string buildingnumber { get; set; }

        [DataMember]
        [MaxLength(100, ErrorMessage = "Street cannot be greater than 100;")]
        public string street { get; set; }

        [DataMember]
        [MaxLength(100, ErrorMessage = "Locality cannot be greater than 100;")]
        public string locality { get; set; }

        [DataMember]
        [MaxLength(70, ErrorMessage = "Town cannot be greater than 70;")]
        public string town { get; set; }

        [DataMember]
        [Required(ErrorMessage = "postcode can not be empty")]
        public string postcode { get; set; }

        [DataMember]
        public string county { get; set; }

        [DataMember]
        public string dependentlocality { get; set; }

        [DataMember]
        [Required(ErrorMessage = "country can not be empty")]
        [MaxLength(3, ErrorMessage = "Country ISO ALPHA-3 Code cannot be greater than 3;")]
        public string country { get; set; }

        [DataMember]
        [DataType(DataType.Text)]
        public bool? fromcompanieshouse { get; set; }
    }
}
