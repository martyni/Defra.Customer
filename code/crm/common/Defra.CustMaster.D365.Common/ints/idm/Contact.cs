using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Defra.CustMaster.D365.Common.Ints.Idm
{
    [DataContract]
    public partial class Contact
    {
        [DataMember]
        [Required(ErrorMessage ="B2cObject is required and can not be empty.")]
        public string b2cobjectid;

        [DataMember]
        public int? title;

        [DataMember]
        [MaxLength(50, ErrorMessage = "First Name cannot be greater than 50")]
        public string firstname;

        [DataMember]
        [MaxLength(50, ErrorMessage = "Middle Name cannot be greater than 50")]
        public string middlename;

        [DataMember]
        [MaxLength(50, ErrorMessage = "Last Name cannot be greater than 50")]
        public string lastname;

        [DataMember]
        [MaxLength(100, ErrorMessage = "Email cannot be greater than 100")]
        public string email;

        [DataMember]
        public string dob;

        [DataMember]
        public int? gender;

        [DataMember]
        [MaxLength(50, ErrorMessage = "Telephone cannot be greater than 50")]
        public string telephone;

        [DataMember]
        [MaxLength(5, ErrorMessage = "T&C Accepted Version cannot be greater than 5")]
        public string tacsacceptedversion;

        [DataMember]
        public string tacsacceptedon;

        [DataMember]
        public Address address;
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


