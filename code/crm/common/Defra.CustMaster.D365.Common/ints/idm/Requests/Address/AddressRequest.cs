using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Defra.CustMaster.D365.Common.Ints.Idm
{
    public class AddressRequest
    {
        [Required(ErrorMessage = "record type can not be empty")]
        [DataMember]
        public RecordType recordtype { get; set; }

        [DataMember]
        [Required(ErrorMessage = "recordid can not be empty")]
        [MaxLength(36, ErrorMessage = "record id exceeded the max length(36);")]
        public string recordid { get; set; }

        [DataMember]
        public Address address { get; set;}

    }

    public enum AddressTypes
    {
        RegisteredAddress = 1, BusinessActivityAddress = 2, CorrespondenceAddress = 3, BillingorPaymentAddress = 4
    }
    public enum EmailTypes
    {
        PrincipalEmailAddress = 6, AdditionalEmailAddress = 10 
    }

    public enum PhoneTypes
    {
        PrincipalPhoneNumber = 7, AdditionalPhoneNumber = 11
    }
    public enum RecordType
    {
        organisation = 1, contact = 2
    }
}