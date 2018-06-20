﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Defra.CustMaster.D365.Common.Ints.Idm
{
    [DataContract]
    public partial class ContactUpdate
    {
        [DataMember]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Contact ID does not exist;")]
        [DataType(DataType.Text)]
        public Guid ContactId { get; set; }

        [DataMember]
        public int? title { get; set; }

        [DataMember]
        [MaxLength(50, ErrorMessage = "First Name cannot be greater than 50;")]
        public string firstname { get; set; }

        [DataMember]
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

        [DataMember]
        public Address address { get; set; }
    }
   
}


