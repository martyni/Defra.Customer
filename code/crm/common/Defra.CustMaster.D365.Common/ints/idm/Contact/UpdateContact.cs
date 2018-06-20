using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Defra.CustMaster.D365.Common.Ints.Idm
{
    public class UpdateContact
    {

        [Required(AllowEmptyStrings = false, ErrorMessage = "Contact ID is required and can not be empty;")]
        [MaxLength(36, ErrorMessage = "Contact ID is invalid/exceed the max length(50);")]
        public String contactid { get; set; }

        public UpdateContactDetails updates { get; set; }
    }
    public class UpdateContactDetails
    {
        [DataType(DataType.Text)]
        public int? title { get; set; }

        [DataType(DataType.Text)]
        [MaxLength(50, ErrorMessage = "First Name cannot be greater than 50;")]

        public string firstname { get; set; }

        [DataType(DataType.Text)]

        [MaxLength(50, ErrorMessage = "Middle Name cannot be greater than 50;")]

        public string middlename { get; set; }

        [DataMember]
        [MaxLength(50, ErrorMessage = "Last Name cannot be greater than 50")]
        public string lastname { get; set; }

        [DataMember]
        [MaxLength(100, ErrorMessage = "Email cannot be greater than 100;")]
        [EmailAddress]
        public string email { get; set; }

        [DataMember]
        public string dob { get; set; }

        [DataMember]
        public int? gender { get; set; }

        [DataMember]
        [MaxLength(50, ErrorMessage = "Telephone cannot be greater than 50;")]
        public string telephone { get; set; }

        [DataMember]
        [MaxLength(5, ErrorMessage = "T&C Accepted Version cannot be greater than 5;")]
        public string tacsacceptedversion { get; set; }

        [DataMember]


        public string tacsacceptedon { get; set; }
    }

}
