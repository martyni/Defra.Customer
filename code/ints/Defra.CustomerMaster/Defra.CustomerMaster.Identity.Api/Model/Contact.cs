﻿using System;
using System.Net;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Defra.CustomerMaster.Identity.Api.Model
{
    [DataContract]
    public class Contact
    {
        [DataMember]
        
        public string contactid;

        [DataMember]
        public string firstname;

        [DataMember]
        [Required]
        public string lastname;

        [DataMember]
        public string emailid;

        [DataMember]
        [Required]
        public string UPN;

        [DataMember]
        public int Code;

        [DataMember]
        public string Message;

        [DataMember]
        public string MessageDetail;

        [DataMember]
        public HttpStatusCode HttpStatusCode;

    }
   
}