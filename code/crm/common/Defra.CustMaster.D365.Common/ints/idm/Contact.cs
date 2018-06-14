using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Defra.CustMaster.D365.Common.Ints.Idm
{
    [DataContract]
    public partial class Contact
    {
        [DataMember]
        [Required(AllowEmptyStrings = false, ErrorMessage = "B2cObject is required and can not be empty.")]
        public string b2cobjectid { get; set; }

        [DataMember]
        public int? title { get; set; }

        [DataMember]
        [MaxLength(50, ErrorMessage = "First Name cannot be greater than 50")]
        [Required(ErrorMessage = "Firstname is required")]
        public string firstname { get; set; }

        [DataMember]
        [MaxLength(50, ErrorMessage = "Middle Name cannot be greater than 50")]

        public string middlename { get; set; }

        [DataMember]
        [Required(ErrorMessage = "Lastname is required")]
        [MaxLength(50, ErrorMessage = "Last Name cannot be greater than 50")]
        public string lastname { get; set; }

        [DataMember]
        [MaxLength(100, ErrorMessage = "Email cannot be greater than 100")]
        public string email { get; set; }

        [DataMember]
        public string dob { get; set; }

        [DataMember]
        public int? gender { get; set; }

        [DataMember]
        [MaxLength(50, ErrorMessage = "Telephone cannot be greater than 50")]
        public string telephone { get; set; }

        [DataMember]
        [MaxLength(5, ErrorMessage = "T&C Accepted Version cannot be greater than 5")]
        public string tacsacceptedversion { get; set; }

        [DataMember]
        public string tacsacceptedon { get; set; }

        [DataMember]
        public Address address { get; set; }
    }
    public enum ContactGenderCodes
    {
        Male = 1, Female = 2
    };
    public enum ContactTitles
    {
        Mr = 1, Mrs = 2
    };
}


