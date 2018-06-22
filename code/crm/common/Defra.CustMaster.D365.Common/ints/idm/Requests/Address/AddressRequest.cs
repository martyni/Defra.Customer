using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Defra.CustMaster.D365.Common.Ints.Idm
{
    public class AddressRequest
    {
        [Required(ErrorMessage = "record type can not be empty")]
        public RecordType recordtype { get; set; }

        [Required(ErrorMessage = "recordid can not be empty")]
        [MaxLength(36, ErrorMessage = "record id exceeded the max length(36);")]
        public string recordid { get; set; }
        public Address address { get; set;}

    }

    public enum AddressTypes
    {
        RegisteredAddress = 1, BusinessActivityAddress = 2, CorrespondenceAddress = 3, BillingorPaymentAddress = 4
    }
    public enum RecordType
    {
        Organisation = 1, Contact = 2
    }
}