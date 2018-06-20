using System.ComponentModel.DataAnnotations;

namespace Defra.CustMaster.D365.Common.Ints.Idm
{
    public partial class OrganisationRequest
    {
        
        [DataType(DataType.Text)]
        [MaxLength(160,ErrorMessage = "Organisation name is more than 160 characters.")]
        [Required(ErrorMessage = "Organisation name is mandatory.")]
        public string name { get; set; }
        [Required(ErrorMessage = "Business type cannot be empty.")]
        [DataType(DataType.Text)]
        public int? type { get; set; }
        [MaxLength(8,ErrorMessage = "Company House Id cannot be more than 8 characters.")]
        public string crn { get; set; }
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [MaxLength(100, ErrorMessage = "Email address cannot be more than 100 characters long.")]
        public string email { get; set; }
        
        public bool? validatedwithcompanieshouse { get; set; }
        public Address address { get; set; }
        public string telephone { get; set; }

        
        public int hierarchylevel { get; set; }
        public ParentOrganisation parentorganisation { get; set; }
    }

}
